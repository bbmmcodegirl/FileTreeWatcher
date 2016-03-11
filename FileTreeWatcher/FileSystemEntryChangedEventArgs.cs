using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTreeWatcher
{
    public class FileSystemEntryChangedEventArgs: EventArgs
    {
        public FileSystemEntryChangedEventArgs()
        {
            Change = new FileSystemEntryChange();
        }

        public FileSystemEntryChangedEventArgs(FileSystemEntryChange change)
        {
            Change = change;
        }

        public FileSystemEntryChange Change { get; }
    }
}
