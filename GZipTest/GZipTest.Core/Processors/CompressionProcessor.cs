using System;
using System.IO;
using System.IO.Compression;

namespace GZipTest.Core.Processors
{
    internal sealed class CompressionProcessor : ProcessorBase
    {
        private readonly int blockSize;

        public CompressionProcessor(ILogger logger, FileStream sourceStream, FileStream writeFileStream, int blockSize)
            : base(logger, sourceStream, writeFileStream)
        {
            this.blockSize = blockSize;
        }

        protected override void ProcessCore(Stream sourceStream, Stream targetstream)
        {
            using (GZipStream compressionStream = new GZipStream(targetstream, CompressionLevel.Optimal, true))
            {
                sourceStream.CopyTo(compressionStream);
            }
        }

        protected override void Read()
        {
            var size = _sourceStream.Length;
            this._blocksCount = 1;
            if (size > blockSize)
            {
                double blocksTemp = size / (double)blockSize;
                this._blocksCount = (int)Math.Ceiling(blocksTemp);
            }

            int i = 0;
            while (_sourceStream.Position < _sourceStream.Length)
            {
                i++;
                this._readingPool.WaitOne();
                byte[] buffer = new byte[blockSize];
                int readed = _sourceStream.Read(buffer, 0, blockSize);
                this.CommitBlock(new MemoryStream(buffer, 0, readed));
            }
        }

        protected override long WriteBlockCore(MemoryStream stream, int number)
        {
            const int extraBytesCount = 14; //2bytes XLEN, 2 bytes extra sub ID, 2 bytes extra sub length, 8 bytes of long size of block
            stream.Position = 0;
            var size = stream.Length + extraBytesCount;
            byte[] buffer = new byte[headerLength + extraBytesCount];
            stream.Read(buffer, 0, headerLength);
            GzipFlag flg = (GzipFlag)buffer[flgPosition];
            flg |= GzipFlag.FEXTRA;
            buffer[flgPosition] = (byte)flg;
            buffer[headerLength] = 0x0c; //XLEN - 14
            buffer[headerLength + 1] = 0x00; //XLEN - second byte
            buffer[headerLength + 2] = 0x43; //SI1 - sub id 1 'C'
            buffer[headerLength + 3] = 0x68; //SI2 - sub id 2 'h'
            buffer[headerLength + 4] = 0x08; //sub length - 8
            buffer[headerLength + 5] = 0x00; //sub length - sencond byte
            byte[] sizeBuffer = BitConverter.GetBytes(size);
            Array.Copy(sizeBuffer, 0, buffer, headerLength + 6, sizeBuffer.Length);

            this._writeFileStream.Write(buffer, 0, buffer.Length);

            //write remaining
            stream.CopyTo(this._writeFileStream);

            return size;
        }
    }
}
