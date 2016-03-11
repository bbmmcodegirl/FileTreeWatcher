using System;
using System.IO;

namespace FileTreeWatcher
{
    public class FileEntry : Entry
    {
        public FileEntry(string path) : base(path)
        {
            if (File.Exists(path))
            {
                Size = new FileInfo(path).Length;
            }
        }
    }
}