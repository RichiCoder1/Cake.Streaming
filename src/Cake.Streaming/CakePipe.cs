using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cake.Core.IO;
using Cake.Streaming.Core;

namespace Cake.Streaming
{
    public sealed class CakePipe : ICakePipe, IDisposable
    {
        private List<PipeFile> _files;

        internal CakePipe(IEnumerable<FilePath> files, bool buffer = false)
        {
            _files = files.Select(filePath => new PipeFile(filePath, buffer: buffer)).ToList();
        }

        public void Pipe(Action<PipeFile> processor)
        {
            foreach (var pipeFile in _files)
            {
                processor(pipeFile);
            }
            Dispose();
        }


        public Task PipeAsync(Func<PipeFile, Task> processor)
        {
            return Task.WhenAll(_files.Select(processor)).ContinueWith(t => Dispose());
        }
        

        public void PipeAll(Action<IReadOnlyList<PipeFile>> processor)
        {
            processor(_files);
            Dispose();
        }

        public Task PipeAllAsync(Func<IReadOnlyList<PipeFile>, Task> processor)
        {
            return processor(_files).ContinueWith(t => Dispose());
        }

        public ICakePipe Pipe(Func<PipeFile, PipeFile> processor)
        {
            var newPipes = _files.Select(processor).ToList();
            var oldPipes = _files.Except(newPipes);
            foreach (var pipeFile in oldPipes)
            {
                pipeFile.Dispose();
            }
            _files = newPipes;
            return this;
        }

        public async Task<ICakePipe> PipeAsync(Func<PipeFile, Task<PipeFile>> processor)
        {
            var newPipes = (await Task.WhenAll(_files.Select(processor)).ConfigureAwait(false)).ToList();
            var oldPipes = _files.Except(newPipes);
            foreach (var pipeFile in oldPipes)
            {
                pipeFile.Dispose();
            }
            _files = newPipes;
            return this;
        }

        public async Task<ICakePipe> PipeAllAsync(Func<IReadOnlyList<PipeFile>, Task<IReadOnlyList<PipeFile>>> processor)
        {
            var newPipes = (await processor(_files).ConfigureAwait(false)).ToList();
            var oldPipes = _files.Except(newPipes);
            foreach (var pipeFile in oldPipes)
            {
                pipeFile.Dispose();
            }
            _files = newPipes;
            return this;
        }


        public void Destination(DirectoryPath directoryPath)
        {
            foreach (var pipeFile in _files)
            {
                using (var filestream = File.OpenWrite(directoryPath.GetFilePath(pipeFile.FilePath).FullPath))
                {
                    var pipeFileStream = pipeFile.Contents;

                    pipeFileStream.Seek(0, SeekOrigin.Begin);
                    pipeFileStream.CopyTo(filestream);

                    pipeFile.Dispose();
                }

            }
        }

        #region IDisposable

        private bool _disposed;

        ~CakePipe()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (var file in _files)
                    {
                        file.Dispose();
                    }
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
