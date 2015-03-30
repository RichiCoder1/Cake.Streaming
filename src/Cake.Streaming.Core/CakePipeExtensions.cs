using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cake.Streaming.Core
{
    public static class CakePipeExtensions
    {
        public static void Pipe(this ICakePipe pipe, Func<PipeFile, Task> processor)
        {
            AsyncHelper.RunSync(() => pipe.PipeAsync(processor));
        }

        public static ICakePipe Pipe(this ICakePipe pipe, Func<PipeFile, Task<PipeFile>> processor)
        {
            return AsyncHelper.RunSync(() => pipe.PipeAsync(processor));
        }

        public static ICakePipe PipeAll(this ICakePipe pipe, Func<IReadOnlyList<PipeFile>, Task<IReadOnlyList<PipeFile>>> processor)
        {
            return AsyncHelper.RunSync(() => pipe.PipeAllAsync(processor));
        }
    }
}
