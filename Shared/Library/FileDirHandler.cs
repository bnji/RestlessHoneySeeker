using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Library
{
    public class FileDirHandler
    {
        public List<FileDirInfo> FileDirInfoList { get; private set; }

        public FileDirHandler()
        {
            FileDirInfoList = new List<FileDirInfo>();
        }

        public void CreateFileDirInfoEntry(FileSystemEventArgs e)
        {
            FileDirInfo fdi = FileDirInfoList.Find((FileDirInfo _fdi) => _fdi.ChangeType == e.ChangeType && _fdi.FileInfo.Name == e.Name);
            if (fdi != null)
            {
                fdi.ChangeCount++;
            }
            else
            {
                fdi = new FileDirInfo()
                {
                    ChangeCount = 0,
                    DateTime = DateTime.Now,
                    ChangeType = e.ChangeType,
                    FileInfo = new FileInfo(e.FullPath)
                };
                FileDirInfoList.Add(fdi);
            }
            Console.WriteLine(e.ChangeType);
        }
    }
}
