using System.IO;
using System.Text;

namespace Cake.Streaming.Core
{
    public static class PipeFileExtensions
    {
        public static StreamReader ToReader(this PipeFile pipeFile, Encoding enc = null)
        {
            return new StreamReader(pipeFile.Contents, enc ?? Encoding.Default, true, 1024, true);
        }

        public static StreamWriter ToWriter(this PipeFile pipeFile, Encoding enc = null)
        {
            return new StreamWriter(pipeFile.Contents, enc ?? new UTF8Encoding(true, false), 1024, true);
        }
    }
}
