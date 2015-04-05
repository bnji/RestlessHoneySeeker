using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Library
{
    public class FileDirInfo
    {
        public uint ChangeCount { get; set; }
        public DateTime DateTime { get; set; }
        public WatcherChangeTypes ChangeType { get; set; }
        public FileInfo FileInfo { get; set; }
    }
}
