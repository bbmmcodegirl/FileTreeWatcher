using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileTreeWatcher
{
    public class FileTreeSizeTracker
    {
        private Dictionary<string, IEntry> mTrackedFiles = new Dictionary<string, IEntry>();

        private object mLock = new object();

        private Timer mPollTimer = null;
        private bool mTimerIsRunning = false;

        public FileTreeSizeTracker()
        {
            FileTreeWatcher.ChangeEvent += FileTreeWatcher_ChangeEvent;
        }

        private void FileTreeWatcher_ChangeEvent(object sender, EventArgs e)
        {
            if (!(e is FileSystemEntryChangedEventArgs))
            {
                return;
            }
            var args = e as FileSystemEntryChangedEventArgs;
            var fe = args.Change;
            if (fe.Change == EntryChange.Discovered || fe.Change == EntryChange.Creation)
            {
                if (fe.Entry != null && (fe.Entry is FileEntry))
                {
                    addEntry(fe.Entry.PathName, fe.Entry);
                }
                if (fe.Entries != null)
                {
                    foreach (var entry in fe.Entries.Where(f => f is FileEntry))
                    {
                        addEntry(entry.PathName, entry);
                    }
                }
            }
            else if (fe.Change == EntryChange.Dismissed || fe.Change == EntryChange.Deletion)
            {
                if (fe.Entry != null)
                {
                    removeEntry(fe.Entry.PathName);
                }
                if (fe.Entries != null)
                {
                    foreach (var entry in fe.Entries)
                    {
                        removeEntry(entry.PathName);
                    }
                }
            }
            else if (fe.Change == EntryChange.NameChange)
            {
                if (fe.Entry != null)
                {
                    string oldPath = Path.Combine(Path.GetDirectoryName(fe.Entry.PathName), fe.NameBefore);
                    string newPath = Path.Combine(Path.GetDirectoryName(fe.Entry.PathName), fe.NameAfter);

                    lock (mLock)
                    {
                        var oldKeys = mTrackedFiles.Keys.Where(k => k.StartsWith(oldPath)).ToList();
                        foreach (var key in oldKeys)
                        {
                            var oldKey = key;
                            var newKey = newPath + oldKey.Substring(oldPath.Length);
                            var value = mTrackedFiles[oldKey];
                            value.PathName = newKey;
                            mTrackedFiles.Remove(oldKey);
                            mTrackedFiles[newKey] = value;
                        }
                    }
                }
            }
        }

        private void addEntry(string name, IEntry entry)
        {
            lock (mLock)
            {
                mTrackedFiles[name] = entry;
            }
        }

        private void removeEntry(string name)
        {
            lock (mLock)
            {
                if (mTrackedFiles.ContainsKey(name))
                {
                    mTrackedFiles.Remove(name);
                }
            }
        }

        public void Start(int pollInterval)
        {
            if (pollInterval == 0)
            {
                return;
            }
            if (mPollTimer != null)
            {
                return;
            }
            mPollTimer = new Timer(pollFileSizes, null, pollInterval, pollInterval);
        }

        public void Stop()
        {
            if (mPollTimer == null)
            {
                return;
            }
            mPollTimer.Dispose();
            mPollTimer = null;
        }

        private void pollFileSizes(object state)
        {
            List<FileEntry> fileEntries = new List<FileEntry>();
            lock (mLock)
            {
                if (mTimerIsRunning)
                {
                    return;
                }
                mTimerIsRunning = true;
                fileEntries = mTrackedFiles.Keys
                    .Where(key => mTrackedFiles[key] is FileEntry)
                    .Select(key => mTrackedFiles[key] as FileEntry)
                    .ToList();
            }

            foreach (var fileEntry in fileEntries)
            {
                var fileInfo = new FileInfo(fileEntry.PathName);
                if (!fileInfo.Exists)
                {
                    continue;
                }
                long currentSize = fileInfo.Length;
                long oldSize = fileEntry.Size;
                if (oldSize < currentSize)
                {
                    fileEntry.Size = currentSize;
                    FileTreeWatcher.InvokeChangeEvent(fileEntry, new FileSystemEntryChange() { Entry = fileEntry, Change = EntryChange.Grown, SizeBefore = oldSize, SizeAfter = currentSize });
                }
                else if (oldSize > currentSize)
                {
                    fileEntry.Size = currentSize;
                    FileTreeWatcher.InvokeChangeEvent(fileEntry, new FileSystemEntryChange() { Entry = fileEntry, Change = EntryChange.Shrunk, SizeBefore = oldSize, SizeAfter = currentSize });
                }
            }
            mTimerIsRunning = false;
        }
    }
}
