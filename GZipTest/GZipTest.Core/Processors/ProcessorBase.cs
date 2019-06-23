using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace GZipTest.Core.Processors
{

    internal abstract class ProcessorBase
    {
        //Basic length of GZIP file
        protected const int headerLength = 10;
        //Position of FLG byte in GZIP header
        protected const int flgPosition = 3;

        protected int _workingThreads;

        protected readonly Semaphore _readingPool;
        protected readonly FileStream _writeFileStream;
        private readonly ILogger _logger;
        protected readonly FileStream _sourceStream;
        protected int _blocksCount;
        protected bool _readerWorking;

        protected readonly AutoResetEvent printEvent = new AutoResetEvent(false);
        protected BlockingCollection<Block> _readedBlocks;
        protected BlockingCollection<Block> _blocksToWrite;

        protected readonly object _blocksLock = new object();


        #region Stats

        protected int _readed;
        protected int _processed;
        protected int _finished;
        protected bool _isFinished;
        protected readonly object _numbersLock = new object();

        public int ReadedCount
        {
            get
            {
                lock (_numbersLock)
                {
                    return this._readed;
                }

            }
            protected set
            {
                lock (_numbersLock)
                {
                    this._readed = value;
                    Tick();
                }

            }
        }
        public int ProcessedCount
        {
            get
            {
                lock (_numbersLock)
                {
                    return this._processed;
                }

            }
            protected set
            {
                lock (_numbersLock)
                {
                    this._processed = value;
                    Tick();
                }

            }
        }
        public int FinishedCount
        {
            get
            {
                lock (_numbersLock)
                {
                    return this._finished;
                }

            }
            protected set
            {
                lock (_numbersLock)
                {
                    this._finished = value;
                    Tick();
                }

            }
        }

        #endregion


        public ProcessorBase(ILogger logger, FileStream sourceStream, FileStream writeFileStream)
        {
            //Ensure that not a whole file will be loaded to memory,
            //but there will be enought data for processing threads
            this._workingThreads = Environment.ProcessorCount;
            this._readingPool = new Semaphore(_workingThreads, _workingThreads * 2);

            this._blocksToWrite = new BlockingCollection<Block>(_workingThreads * 4);
            this._readedBlocks = new BlockingCollection<Block>(_workingThreads * 3);
            this._logger = logger;
            this._sourceStream = sourceStream;
            this._writeFileStream = writeFileStream;
        }

        internal void Process()
        {
            var printerThread = new Thread(() => Print());

            printerThread.Name = "Printer Thread";
            printerThread.Priority = ThreadPriority.Normal;
            printerThread.Start();

            var readerThread = new Thread(ReadStart);
            readerThread.Name = "Reader Thread";
            readerThread.Priority = ThreadPriority.Highest;
            _readerWorking = true;
            readerThread.Start();

            var compressingThreads = new List<Thread>();

            for (int i = 0; i < _workingThreads; i++)
            {
                Thread compressingThread = new Thread(ProcessCore);
                compressingThreads.Add(compressingThread);
                compressingThread.Name = $"Compressor #{i + 1}";
                compressingThread.Start();
            }
            this.Write();

            _isFinished = true;

            Tick(); //to release printer thhread
            printerThread.Join(100);
            readerThread.Join(1000);
            foreach (var thr in compressingThreads)
            {
                thr.Join(100);
                Print(true);
            }
            //To ensure everything is written
            Print(true);
        }

        protected void ReadStart()
        {
            this._readerWorking = true;
            this.Read();
            this._readerWorking = false;
        }

        protected abstract void Read();

        protected virtual void CommitBlock(MemoryStream ms)
        {
            Block block = new Block();
            ms.Seek(0, SeekOrigin.Begin);
            block.SourceStream = ms;
            this._readedBlocks.Add(block);
            this._blocksToWrite.Add(block);
            this.ReadedCount++;
        }

        protected void ProcessCore()
        {
            while (!_isFinished)
            {
                if (!TryGetNextBlockForProcessing(out Block block))
                {
                    return;
                }

                MemoryStream resultStream = new MemoryStream();
                using (Stream sourceStream = block.SourceStream)
                {
                    block.SourceStream = null;
                    this.ProcessCore(sourceStream, resultStream);
                }
                resultStream.Seek(0, SeekOrigin.Begin);
                block.SetProcessedStream(resultStream);
                this.ProcessedCount++;
                this._readingPool.Release(); //release for reading another block

            }
        }

        protected abstract void ProcessCore(Stream sourceStream, Stream targetstream);

        protected virtual void Write()
        {
            int i = 0;

            //Final number of blocks can vary
            while (i < this._blocksCount || _readerWorking)
            {

                var block = this._blocksToWrite.Take();
                using (var stream = block.GetProcessedStream())
                {
                    if (stream != null)
                    {
                        var writedBytesCount = WriteBlockCore(stream, i);

                        block.Dispose();
                        i++;
                        this.FinishedCount++;
                    }
                    else
                    {
                        throw new NullReferenceException("Writing stream is null");
                    }
                }
            }
        }

        protected virtual long WriteBlockCore(MemoryStream stream, int number)
        {
            stream.CopyTo(this._writeFileStream);
            return stream.Length;
        }

        protected bool TryGetNextBlockForProcessing(out Block readedBlock)
        {
            while (!this._isFinished)
            {
                if (_readedBlocks.TryTake(out readedBlock, 50))
                    return true;
            }
            readedBlock = null;
            return false;
        }


        protected void Tick()
        {
            this.printEvent.Set();
        }

        private void Print(bool force = false)
        {
            do
            {
                if (!force)
                    printEvent.WaitOne();
                string s = GetPrintContent();
                _logger?.ProgressInfo(s);
            } while (!_isFinished);
        }

        protected virtual string GetPrintContent()
        {
            int total = this._blocksCount;
            int readed = this.ReadedCount;
            int processed = this.ProcessedCount;
            int finished = this.FinishedCount;


            int perc1 = 0;
            int perc2 = 0;
            int perc3 = 0;
            if (total > 0)
            {
                perc1 = (readed * 100) / total;
                perc2 = (processed * 100) / total;
                perc3 = (finished * 100) / total;
            }
            string s = $"Reading    {perc1:##0}% ... {readed:###0} / {total:###0}\r\nProcessing {perc2:##0}% ... {processed:###0} / {total:###0}\r\nFinished   {perc3:##0}% ... {finished:###0} / {total:###0}";
            return s;
        }
    }
}
