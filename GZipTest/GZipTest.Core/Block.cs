using System;
using System.IO;
using System.Threading;

namespace GZipTest.Core
{
    internal class Block : IDisposable
    {
        private readonly object _lockSourceStream = new object();

        private MemoryStream _resultStream;
        private MemoryStream _sourceStream;
        private readonly ManualResetEvent _readyForWriteEvent = new ManualResetEvent(false);

        public MemoryStream SourceStream
        {
            get
            {
                lock (_lockSourceStream)
                {
                    return this._sourceStream;
                }
            }

            set
            {
                lock (_lockSourceStream)
                {
                    this._sourceStream = value;
                }
            }
        }


        public void Dispose()
        {
            this._resultStream?.Dispose();
            this._resultStream = null;
            this._sourceStream?.Dispose();
            this._sourceStream = null;
        }
        internal void SetProcessedStream(MemoryStream resultStream)
        {
            this._resultStream = resultStream;
            this._readyForWriteEvent.Set();
        }

        internal MemoryStream GetProcessedStream()
        {
            this._readyForWriteEvent.WaitOne();
            var stream = this._resultStream;
            this._resultStream = null;
            return stream;
        }
    }
}
