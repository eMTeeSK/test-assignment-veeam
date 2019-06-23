using System;
using System.IO;
using System.IO.Compression;

namespace GZipTest.Core.Processors
{
    internal class DecompressionProcessor : ProcessorBase
    {
        const int readBufferSize = 8192;

        private long _sourceSize;
        private long _readedBytes;
        public long ReadedBytes
        {
            get
            {
                lock (_numbersLock)
                {
                    return this._readedBytes;
                }

            }
            protected set
            {
                lock (_numbersLock)
                {
                    this._readedBytes = value;
                    Tick();
                }

            }
        }


        public DecompressionProcessor(ILogger logger, FileStream sourceStream, FileStream writeFileStream)
            : base(logger, sourceStream, writeFileStream) { }


        protected override void Read()
        {
            this._readingPool.WaitOne();
            this._sourceSize = _sourceStream.Length;
            this._readedBytes = 0;
            this._blocksCount = 0;
            var ms = new MemoryStream();
            byte[] buffer = new byte[readBufferSize];
            //bool lastByteIsID1 = false;
            while (_sourceStream.Position < _sourceStream.Length)
            {

                var readed = _sourceStream.Read(buffer, 0, readBufferSize);
                GzipFlag flg = (GzipFlag)buffer[flgPosition];
                if (!flg.HasFlag(GzipFlag.FEXTRA))
                {
                    throw new FileLoadException("This file is not compressed with this program, therefore is not supported");
                }
                long size = BitConverter.ToInt64(buffer, headerLength + 6);
                if (size < readed)
                {
                    ms.Write(buffer, 0, (int)size);
                    //we have to move position in stream back to end of current block
                    _sourceStream.Seek(size - readed, SeekOrigin.Current);
                }
                else
                {
                    ms.Write(buffer, 0, readed);
                    long remaining = size - readed;
                    while (remaining > 0)
                    {
                        int toRead = (int)Math.Min(remaining, readBufferSize);
                        readed = _sourceStream.Read(buffer, 0, toRead);
                        remaining -= readed;
                        ms.Write(buffer, 0, readed);
                    }
                }
                this._readedBytes += ms.Length;
                CommitBlock(ms);

                this._readingPool.WaitOne();
                ms = new MemoryStream();
            }
            if (ms.Length > 0)
                CommitBlock(ms);//Commit last block
        }

        protected override void CommitBlock(MemoryStream ms)
        {
            this._blocksCount++;
            base.CommitBlock(ms);
        }


        protected override void ProcessCore(Stream sourceStream, Stream targetstream)
        {
            using (GZipStream compressionStream = new GZipStream(sourceStream, CompressionMode.Decompress, true))
            {
                compressionStream.CopyTo(targetstream);
            }
        }

        protected override string GetPrintContent()
        {

            var total = this._sourceSize;
            var totalBlocks = this._blocksCount;
            var readed = this.ReadedBytes;
            int processed = this.ProcessedCount;
            int finished = this.FinishedCount;


            int perc1 = 0;
            if (total > 0)
            {
                perc1 = (int)(readed * 100 / total);
            }
            var s1 = $"Reading   {perc1,5:##0}% ... {readed,9:# ##0} kB / {total,9:# ##0} kB in {totalBlocks} blocks";
            var s2 = $"Processed {processed} blocks";
            var s3 = $"Finished  {finished} blocks";

            string s = $"{s1}\r\n{s2}\r\n{s3}";
            return s;
        }
    }
}
