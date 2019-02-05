using System.IO;
using System.Text;

namespace Vostok.ClusterConfig.Core.Parsers.Content
{
    internal class FileContent : IFileContent
    {
        private readonly FileStream stream;
        private readonly int size;

        private volatile string stringContent;
        private volatile byte[] bytesContent;

        public FileContent(FileStream stream, int size)
        {
            this.stream = stream;
            this.size = size;
        }

        public string AsString => stringContent ?? (stringContent = ReadAsString());

        public byte[] AsBytes => bytesContent ?? (bytesContent = ReadAsBytes());

        public Stream AsStream => stream;

        private string ReadAsString()
            => new StreamReader(stream, Encoding.UTF8).ReadToEnd();

        private byte[] ReadAsBytes()
        {
            var result = new byte[size];
            var bytesToRead = size;
            var offset = 0;

            while (bytesToRead > 0)
            {
                var bytesRead = stream.Read(result, offset, bytesToRead);
                if (bytesRead == 0)
                    throw new EndOfStreamException();

                offset += bytesRead;
                bytesToRead -= bytesRead;
            }

            return result;
        }
    }
}
