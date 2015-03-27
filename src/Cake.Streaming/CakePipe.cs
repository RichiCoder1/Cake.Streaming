using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cake.Core.IO;
using Cake.Streaming.Core;

namespace Cake.Streaming
{
    public sealed class CakePipe : ICakePipe
    {
        private List<PipeFile> _files;

        internal CakePipe(IEnumerable<FilePath> files)
        {
            _files = files.Select(filePath => new PipeFile(filePath)).ToList();
        }

        public void Pipe(Action<PipeFile> processor)
        {
            foreach (var pipeFile in _files)
            {
                processor(pipeFile);
            }
        }


        public void Pipe(Func<PipeFile, Task> processor)
        {
            AsyncHelper.RunSync(() => Task.WhenAll(_files.Select(processor)));
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

        public ICakePipe Pipe(Func<PipeFile, Task<PipeFile>> processor)
        {
            var newPipes = AsyncHelper.RunSync(() => Task.WhenAll(_files.Select(processor))).ToList();
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
    }
}
