using System;
using System.Threading.Tasks;
using Cake.Core.IO;

namespace Cake.Streaming.Core
{
    public interface ICakePipe
    {
        void Pipe(Action<PipeFile> processor);
        void Pipe(Func<PipeFile, Task> processor);
        void Destination(DirectoryPath directoryPath);

        ICakePipe Pipe(Func<PipeFile, PipeFile> processor);
        ICakePipe Pipe(Func<PipeFile, Task<PipeFile>> processor);

    }
}
