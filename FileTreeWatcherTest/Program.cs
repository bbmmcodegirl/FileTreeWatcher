using FileTreeWatcher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTreeWatcherTest
{
    class Program
    {
        private static bool firstScan = true;
        private static long totalFilesCreated = 0;
        private static long totalFilesDeleted = 0;
        private static long totalBytesAdded = 0;
        private static long totalBytesRemoved = 0;

        static void Main(string[] args)
        {
            Console.WriteLine($"FileTreeWatcherTest");
            FileTreeWatcher.FileTreeWatcher.ChangeEvent += FileTreeWatcher_ChangeEvent;

            var watcher = new FileTreeWatcher.FileTreeWatcher();
            var fileSizeTracker = new FileTreeSizeTracker();

            Console.WriteLine($"scanning...");
            watcher.AddWatchedPath(System.IO.Path.GetTempPath());

            Console.WriteLine($"finished initial scan.");
            firstScan = false;

            fileSizeTracker.Start(10000);

            Console.ReadLine();

            Console.WriteLine($" totalFilesCreated: { totalFilesCreated}");
            Console.WriteLine($" totalFilesDeleted: { totalFilesDeleted}");
            Console.WriteLine($" totalBytesAdded: { totalBytesAdded}");
            Console.WriteLine($" totalBytesRemoved: { totalBytesRemoved}");
            Console.ReadLine();
        }

        private static void FileTreeWatcher_ChangeEvent(object sender, EventArgs e)
        {
            if (!(e is FileTreeWatcher.FileSystemEntryChangedEventArgs))
            {
                return;
            }
            if (firstScan)
            {
                return;
            }

            var args = e as FileTreeWatcher.FileSystemEntryChangedEventArgs;
            if (args.Change.Entries != null)
            {
                Console.WriteLine($"{args.Change.Time} {args.Change.Change}:");
                Console.WriteLine("\t" + string.Join("\n\t", args.Change.Entries.Select(r => r.PathName)));
            }
            if (args.Change.Entry != null)
            {
                Console.WriteLine($"{args.Change.Time} {args.Change.Entry.PathName} {args.Change.Change}  size before:{args.Change.SizeBefore}  size after:{args.Change.SizeAfter}");
                if (args.Change.Change == FileTreeWatcher.EntryChange.Creation)
                {
                    totalFilesCreated++;
                    totalBytesAdded += args.Change.SizeAfter;
                }
                else if (args.Change.Change == FileTreeWatcher.EntryChange.Deletion)
                {
                    totalFilesDeleted++;
                    totalBytesRemoved -= args.Change.SizeBefore;
                }
                else if (args.Change.Change == FileTreeWatcher.EntryChange.Grown)
                {
                    totalBytesAdded += args.Change.SizeAfter - args.Change.SizeBefore;
                }
                else if (args.Change.Change == FileTreeWatcher.EntryChange.Shrunk)
                {
                    totalBytesRemoved += args.Change.SizeBefore - args.Change.SizeAfter;
                }
            }
        }
    }
}
