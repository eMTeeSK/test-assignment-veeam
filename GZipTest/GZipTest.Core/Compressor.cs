using GZipTest.Core.Processors;
using System;
using System.IO;

namespace GZipTest.Core
{
    public class Compressor
    {
        private readonly int _blockSize;
        private readonly ILogger _logger;


        /// <summary>
        /// Initializes a new instance of the <see cref="Compressor"/> class.
        /// </summary>
        /// <param name="blockSize">Size of the block in MB.</param>
        public Compressor(ILogger logger, int blockSize = 1)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));


            this._blockSize = blockSize * 1024 * 1024;
            //this._blockSize = 1024 * 20; //20kB
            this._logger = logger;
        }

        public bool CompressFile(string sourceFile, string archiveFile)
        {
            return CompressFile(new FileInfo(sourceFile), new FileInfo(archiveFile));
        }

        public bool CompressFile(FileInfo sourceFile, FileInfo archiveFile)
        {
            return ProcessFile(sourceFile, archiveFile, (src, trg) => new CompressionProcessor(this._logger, src, trg, this._blockSize));
        }

        public bool DecompressFile(string archiveFile, string destinationFile)
            => this.DecompressFile(new FileInfo(archiveFile), new FileInfo(destinationFile));

        public bool DecompressFile(FileInfo sourceFile, FileInfo targetFile)
        {
            return ProcessFile(sourceFile, targetFile, (src, trg) => new DecompressionProcessor(this._logger, src, trg));
        }

        private bool ProcessFile(FileInfo sourceFile, FileInfo targetFile, Func<FileStream, FileStream, ProcessorBase> getProcessor)
        {

            if (sourceFile == null)
                throw new ArgumentNullException(nameof(sourceFile));
            if (targetFile == null)
                throw new ArgumentNullException(nameof(targetFile));

            using (FileStream sourceStream = sourceFile.OpenRead())
            {
                if (!sourceStream.CanRead)
                {
                    this._logger.ErrorLog($"Can't read file {sourceFile.FullName}");
                    return false;
                }
                using (FileStream writeFileStream = File.Create(targetFile.FullName))
                {
                    ProcessorBase processor = getProcessor(sourceStream, writeFileStream);
                    processor.Process();
                    writeFileStream.Flush();
                }
                this._logger.Log($"Processed {sourceFile.Name} from {sourceFile.Length} to {targetFile.Length} bytes.");
            }
            return true;
        }
    }
}
