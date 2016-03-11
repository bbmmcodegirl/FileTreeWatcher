using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileTreeWatcher
{
    public class FileTreeWatcher
    {
        private List<FolderEntry> rootEntries = new List<FolderEntry>();

        public static event EventHandler ChangeEvent;

        public FileTreeWatcher()
        {

        }

        public static void InvokeChangeEvent(IEntry sender, FileSystemEntryChange fe)
        {
            if (ChangeEvent != null)
            {
                ChangeEvent(sender, new FileSystemEntryChangedEventArgs(fe));
            }
        }

        public void AddWatchedPath(string watchedPath)
        {
            if (!Directory.Exists(watchedPath))
            {
                throw new ArgumentException("watchedPath");
            }
            if (rootEntries.Any(e => e.PathName == watchedPath))
            {
                return;
            }
            FolderEntry folderEntry = new FolderEntry(watchedPath);
            FileTreeWatcher.InvokeChangeEvent(null, new FileSystemEntryChange() { Entry = folderEntry, Change = EntryChange.Discovered });
            folderEntry.fill();
            rootEntries.Add(folderEntry);
        }

        public void RemoveWatchedPath(string watchedPath)
        {
            if (!Directory.Exists(watchedPath))
            {
                throw new ArgumentException("watchedPath");
            }
            var rootEntry = rootEntries.Where(e => e.PathName == watchedPath).FirstOrDefault();

            if (rootEntry == null)
            {
                return;
            }
            rootEntry.clear();
            rootEntries.Remove(rootEntry);
        }

    }
}
