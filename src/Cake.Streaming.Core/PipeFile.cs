using System;
using System.IO;
using Cake.Core.IO;
using Path = System.IO.Path;

namespace Cake.Streaming.Core
{
    public sealed class PipeFile
    {
        private Stream _backingStream;
        internal FilePath EndFilePath;
        private DirectoryPath _baseDirectory;

        public PipeFile(FilePath filePath, Stream stream = null, bool buffer = false)
        {
            EndFilePath = filePath;
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
                return new PipeFile(EndFilePath, memoryStream);
            }
            if (stream is MemoryStream)
            {
                return new PipeFile(EndFilePath, stream);
            }
            throw new InvalidOperationException("Only File and Memory Streams are supported.");
        }

        public string Name
        {
            get { return EndFilePath.GetFilename().FullPath; }
        }

        public string Extension
        {
            get { return EndFilePath.GetExtension(); }
        }

        public DirectoryPath BaseDirectory
        {
            get { return _baseDirectory; }
        }

        public string FilePath
        {
            get { return EndFilePath.FullPath; }
            set
            {
                var endFilePath = (FilePath) value;
                if (_baseDirectory != null && !endFilePath.IsRelative)
                {
                    endFilePath = GetRelativePath(endFilePath.FullPath, _baseDirectory.FullPath);
                    endFilePath = endFilePath.MakeAbsolute(_baseDirectory);
                }
                EndFilePath = endFilePath;
            }
        }

        internal string RelativeFilePath
        {
            get { return GetRelativePath(EndFilePath.FullPath, _baseDirectory.FullPath); }
        }

        internal void SetBaseDirectory(DirectoryPath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (path.IsRelative)
            {
                throw new ArgumentException("Base Directory path must not be relative.");
            }

            if (path == _baseDirectory)
                return;

            _baseDirectory = path;
            if (!EndFilePath.IsRelative)
            {
                FilePath endPath = GetRelativePath(EndFilePath.FullPath, _baseDirectory.FullPath);
                endPath = endPath.MakeAbsolute(_baseDirectory);
                EndFilePath = endPath;
            }
        }

        private static string GetRelativePath(string filespec, string folder)
        {
            var pathUri = new Uri(filespec, UriKind.Absolute);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            var folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
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
