using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTreeWatcher
{
    public class FolderEntry : Entry
    {
        private FileSystemWatcher mFolderWatcher;

        public FolderEntry(string path) : base(path)
        {
            mFolderWatcher = null;
        }

        public override string PathName
        {
            get
            {
                return base.PathName;
            }
            set
            {
                if (base.PathName == value)
                {
                    return;
                }
                base.PathName = value;
                mFolderWatcher.EnableRaisingEvents = false;
                mFolderWatcher.Path = value ;
                mFolderWatcher.EnableRaisingEvents = true;
            }
        }

        public override void Dispose()
        {
            if (mFolderWatcher != null)
            {
                mFolderWatcher.Created -= folderWatcher_Created;
                mFolderWatcher.Deleted -= folderWatcher_Deleted;
                mFolderWatcher.Renamed -= folderWatcher_Renamed;
                mFolderWatcher.Dispose();
            }
        }

        internal void clear()
        {
            if (mFolderWatcher != null)
            {
                mFolderWatcher.EnableRaisingEvents = false;
                mFolderWatcher.Created -= folderWatcher_Created;
                mFolderWatcher.Deleted -= folderWatcher_Deleted;
                mFolderWatcher.Renamed -= folderWatcher_Renamed;
                mFolderWatcher.Dispose();
                mFolderWatcher = null;
            }
            foreach (var fileEntry in Children.OfType<FileEntry>())
            {
            }
            foreach (var folderEntry in Children.OfType<FolderEntry>())
            {
                folderEntry.clear();
            }
            FileTreeWatcher.InvokeChangeEvent(this, new FileSystemEntryChange() { Entries = Children.ToList(), Change = EntryChange.Dismissed });
            Children.Clear();
        }

        internal void fill()
        {
            if (mFolderWatcher == null)
            {
                mFolderWatcher = new FileSystemWatcher();
                mFolderWatcher.Created += folderWatcher_Created;
                mFolderWatcher.Deleted += folderWatcher_Deleted;
                mFolderWatcher.Renamed += folderWatcher_Renamed;
            }
            mFolderWatcher.EnableRaisingEvents = false;
            mFolderWatcher.Path = PathName;
            Children.Clear();
            try
            {
                string[] subFiles = Directory.GetFiles(PathName).Distinct().ToArray();
                foreach (var subFile in subFiles)
                {
                    Children.Add(new FileEntry(subFile));
                }
                string[] subFolders = Directory.GetDirectories(PathName).Distinct().ToArray();
                foreach (var subFolder in subFolders)
                {
                    Children.Add(new FolderEntry(subFolder));
                }
                FileTreeWatcher.InvokeChangeEvent(this, new FileSystemEntryChange() { Entries = Children.ToList(), Change = EntryChange.Discovered });
                mFolderWatcher.EnableRaisingEvents = true;
                foreach (var folderEntry in Children.OfType<FolderEntry>())
                {
                    folderEntry.fill();
                }
            }
            catch(Exception ex)
            {
                return;
            }
        }

        private void folderWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            IEntry changedEntry = Children.FirstOrDefault(child => child.Name == e.OldName);
            if (changedEntry != null)
            {
                changedEntry.Name = e.Name;
                FileTreeWatcher.InvokeChangeEvent(this, new FileSystemEntryChange() { Entry = changedEntry, Change = EntryChange.NameChange, NameBefore = e.OldName, NameAfter = e.Name });
                return;
            }

            IEntry newEntry = addChild(e.FullPath);
            if (newEntry == null)
            {
                return;
            }
            FileTreeWatcher.InvokeChangeEvent(this, new FileSystemEntryChange() { Entry = newEntry, Change = EntryChange.Discovered, NameBefore = e.OldName, NameAfter = e.Name, SizeAfter = newEntry.Size });
        }

        private void folderWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            var removedChild = Children.FirstOrDefault(child => child.Name == e.Name);
            if (removedChild != null)
            {
                Children.Remove(removedChild);
                FileTreeWatcher.InvokeChangeEvent(this, new FileSystemEntryChange() { Entry = removedChild, Change = EntryChange.Deletion, SizeBefore = removedChild.Size });
            }
        }

        private void folderWatcher_Created(object sender, FileSystemEventArgs e)
        {
            IEntry newEntry = addChild(e.FullPath);
            if (newEntry == null)
            {
                return;
            }
            FileTreeWatcher.InvokeChangeEvent(this, new FileSystemEntryChange() { Entry = newEntry, Change = EntryChange.Creation, SizeAfter = newEntry.Size });
            if (newEntry is FolderEntry)
            {
                (newEntry as FolderEntry).fill();
            }
        }

        private IEntry addChild(string path)
        {
            IEntry newEntry = null;
            if (File.Exists(path))
            {
                newEntry = new FileEntry(path);
            }
            if (Directory.Exists(path))
            {
                newEntry = new FolderEntry(path);
            }
            if (newEntry == null)
            {
                return null;
            }
            Children.Add(newEntry);
            return newEntry;
        }

        public override bool Equals(object obj)
        {
            return obj.ToString() == PathName;
        }

        public override int GetHashCode()
        {
            return PathName.GetHashCode();
        }
    }
}
