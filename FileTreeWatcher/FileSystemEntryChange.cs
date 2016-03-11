using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTreeWatcher
{
    public class FileSystemEntryChange
    {
        public FileSystemEntryChange()
        {
            Time = DateTime.Now;
        }
        public IEntry Entry { get; set; }
        public List<IEntry> Entries { get; set; }
        public EntryChange Change { get; set; }
        public DateTime Time { get; set; }

        public string NameBefore { get; set; }
        public string NameAfter { get; set; }
        public long SizeBefore { get; set; }
        public long SizeAfter { get; set; }
    }
}
