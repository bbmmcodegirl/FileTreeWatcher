using System;
using System.Collections.Generic;

namespace FileTreeWatcher
{
    public interface IEntry : IDisposable
    {
        string Name { get; set; }
        string PathName { get; set; }
        List<IEntry> Children { get; }
        long Size { get; set; }
    }
}