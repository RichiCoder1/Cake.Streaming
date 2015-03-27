using System;
using System.IO;
using System.Text;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;
using Cake.Streaming.Core;

namespace Cake.Streaming
{
    /// <summary>
    /// Contains functionality related to running Streaming.
    /// </summary>
    [CakeAliasCategory("Streaming")]
    public static class StreamingExtensions
    {
        /// <summary>
        /// Creates a Streaming CakePipe from a file glob.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="globber"></param>
        /// <param name="buffer"></param>
        [CakeMethodAlias]
        public static ICakePipe Source(this ICakeContext context, string globber, bool buffer = false)
        {
            if (string.IsNullOrWhiteSpace(globber))
            {
                throw new ArgumentNullException(globber);
            }

            var files = context.Globber.GetFiles(globber);
            return new CakePipe(files, buffer);
        }
    }
}
