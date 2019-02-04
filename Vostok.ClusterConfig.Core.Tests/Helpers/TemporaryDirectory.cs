using System;
using System.IO;

namespace Vostok.ClusterConfig.Core.Tests.Helpers
{
    internal class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Info = new DirectoryInfo(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Guid.NewGuid().ToString()));
            Info.Create();
            Info.Refresh();
        }

        public void Dispose()
        {
            Info.Refresh();

            foreach (var file in Info.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                if (file.Attributes.HasFlag(FileAttributes.ReadOnly))
                    file.Attributes = FileAttributes.Normal;
            }

            Info.Delete(true);
        }

        public DirectoryInfo Info { get; }

        public string Path => Info.FullName;
    }
}