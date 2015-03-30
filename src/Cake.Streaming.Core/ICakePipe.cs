using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cake.Core.IO;

namespace Cake.Streaming.Core
{
    public interface ICakePipe
    {
        /// <summary>
        /// Pipe each file through a processor and end the pipe.
        /// </summary>
        /// <param name="processor"></param>
        void Pipe(Action<PipeFile> processor);
        /// <summary>
        /// Pipe each file through a processor and end the pipe.
        /// </summary>
        /// <param name="processor"></param>
        Task PipeAsync(Func<PipeFile, Task> processor);
        /// <summary>
        /// Pipe all files into a processor and end the pipe.
        /// </summary>
        /// <param name="processor"></param>
        void PipeAll(Action<IReadOnlyList<PipeFile>> processor);
        /// <summary>
        /// Pipe all files into a processor and end the pipe.
        /// </summary>
        /// <param name="processor"></param>
        Task PipeAllAsync(Func<IReadOnlyList<PipeFile>, Task> processor);
        /// <summary>
        /// Write all files in the pipe to a directory. Ends the pipe.
        /// </summary>
        /// <param name="directoryPath"></param>
        void Destination(DirectoryPath directoryPath);

        /// <summary>
        /// Pipe each file through a processor.
        /// </summary>
        /// <param name="processor"></param>
        /// <returns></returns>
        ICakePipe Pipe(Func<PipeFile, PipeFile> processor);
        /// <summary>
        /// Pipe each file through a processor.
        /// </summary>
        /// <param name="processor"></param>
        /// <returns></returns>
        Task<ICakePipe> PipeAsync(Func<PipeFile, Task<PipeFile>> processor);
        /// <summary>
        /// Pipe all files through a processor.
        /// </summary>
        /// <param name="processor"></param>
        Task<ICakePipe> PipeAllAsync(Func<IReadOnlyList<PipeFile>, Task<IReadOnlyList<PipeFile>>> processor);

    }
}
