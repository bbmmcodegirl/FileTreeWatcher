using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTreeWatcher
{
    public class Entry : IEntry
    {
        private string mPathName;

        public Entry(string path)
        {
            mPathName = path;
            Children = new List<IEntry>();
            Size = 0;
        }

        public List<IEntry> Children { get; private set; }

        public long Size { get; set; }

        public string Name
        {
            get
            {
                return Path.GetFileName(mPathName);
            }

            set
            {
                if (File.Exists(mPathName))
                {
                    string newPathName = Path.Combine(Path.GetDirectoryName(mPathName), value);
                    File.Move(mPathName, newPathName);
                    PathName = newPathName;
                }
                if (Directory.Exists(mPathName))
                {
                    string newPathName = Path.Combine(Path.GetDirectoryName(mPathName), value);
                    Directory.Move(mPathName, newPathName);
                    PathName = newPathName;
                }
            }
        }

        public virtual string PathName
        {
            get
            {
                return mPathName;
            }
            set
            {
                mPathName = value;
            }
        }

        public virtual void Dispose()
        {
            foreach (var child in Children)
            {
                child.Dispose();
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
