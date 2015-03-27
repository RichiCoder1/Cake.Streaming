using System;
using System.IO;
using Cake.Core.IO;

namespace Cake.Streaming.Core
{
    public sealed class PipeFile
    {
        internal readonly FilePath FilePath;
        private Stream _backingStream;
        private FilePath _endFilePath;

        public PipeFile(FilePath filePath, Stream stream = null, bool buffer = false)
        {
            FilePath = _endFilePath = filePath;
            if (stream == null)
            {
                if (buffer)
                {
                    using (var fileStream = File.Open(filePath.FullPath, FileMode.Open, FileAccess.Read))
                    {
                        var memoryStream = new MemoryStream();
                        fileStream.CopyTo(memoryStream);
                        _backingStream = memoryStream;
                    }

                }
                else
                {
                    var fileStream = File.Open(filePath.FullPath, FileMode.Open, FileAccess.ReadWrite);
                    _backingStream = fileStream;
                }
            }
            else
            {
                if (!(stream is MemoryStream))
                {
                    var memoryStream = new MemoryStream();
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(memoryStream);
                    _backingStream = memoryStream;
                    stream.Dispose();
                }
                else
                {
                    _backingStream = stream;
                }
            }
        }

        public PipeFile(string fileName, MemoryStream stream)
            : this((FilePath)fileName, stream)
        { }

        /// <summary>
        /// Returns
        /// </summary>
        public Stream Contents
        {
            get
            {
                ThrowIfDisposed();
                return _backingStream;
            }
            set
            {
                ThrowIfDisposed();
                if (value == null || value == Stream.Null)
                {
                    throw new ArgumentNullException("value");
                }

                if (!(value is MemoryStream) && !(value is FileStream))
                {
                    throw new InvalidOperationException("Only Memory and File Streams are supported.");
                }

                _backingStream.Dispose();
                _backingStream = value;
            }
        }

        public bool IsFileStream
        {
            get
            {
                ThrowIfDisposed();
                return _backingStream is FileStream;
            }
        }

        public bool IsMemoryStream
        {
            get
            {
                ThrowIfDisposed();
                return _backingStream is MemoryStream;
            }
        }

        public PipeFile ToBuffer()
        {
            return IsMemoryStream ? this : new PipeFile(FilePath, _backingStream, true);
        }

        /// <summary>
        /// Creates a new PipeFile with passed stream as a buffer, retaining this PipeFile's file metadata.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public PipeFile ToBufferWith(Stream stream)
        {
            if (stream is FileStream)
            {
                var memoryStream = new MemoryStream();
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(memoryStream);
                return new PipeFile(_endFilePath, memoryStream);
            }
            if (stream is MemoryStream)
            {
                return new PipeFile(_endFilePath, stream);
            }
            throw new InvalidOperationException("Only File and Memory Streams are supported.");
        }

        public string Name
        {
            get { return _endFilePath.GetFilename().FullPath; }
        }

        public string Path
        {
            get { return _endFilePath.FullPath; }
            set { _endFilePath = value; }
        }


        #region IDisposable
        private bool _disposed;

        private void ThrowIfDisposed()
        {
            if(_disposed)
                throw new ObjectDisposedException("PipeFile");
        }

        ~PipeFile()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _backingStream.Dispose();
                }
            }
            _disposed = true;
        }

        // We don't implement IDisposable, because nothing outside CakePipe should ever dispose of these.
        internal void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
