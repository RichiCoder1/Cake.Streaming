using System;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;

namespace Cake.Streaming
{
    /// <summary>
    /// Contains functionality related to running Streaming.
    /// </summary>
    [CakeAliasCategory("Streaming")]
    public static class StreamingExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="globber"></param>
        [CakeMethodAlias]
        public static void Source(this ICakeContext context, string globber)
        {
            if (string.IsNullOrWhiteSpace(globber))
            {
                throw new ArgumentNullException(globber);
            }

            var files = context.Globber.GetFiles(globber);
        }
    }
}
