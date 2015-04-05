using System;
using System.IO;

namespace Library
{
    /// <summary>
    /// http://www.codeproject.com/Articles/26528/C-Application-to-Watch-a-File-or-Directory-using-F
    /// </summary>
    public class Watcher
    {
        private FileSystemWatcher fsw;

        public FileSystemWatcher FileSysWatcher
        {
            get { return fsw; }
            set { fsw = value; }
        }

        public Watcher()
        {
            fsw = new FileSystemWatcher();
        }

        public Watcher(String _path, String _filter, bool _includeSubdirectories, bool _enableRaisingEvents)
            : this()
        {
            fsw.Path = _path;
            fsw.Filter = _filter;
            fsw.IncludeSubdirectories = _includeSubdirectories;
            fsw.EnableRaisingEvents = _enableRaisingEvents;
        }
    }
}
