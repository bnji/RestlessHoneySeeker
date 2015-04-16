using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Models
{
    public class FileData
    {
        public FileInfo FileInfo { get; set; }
        private string _fileNameWithExtension;
        public string FileNameWithExtension
        {
            get
            {
                if (FileInfo == null)
                    return _fileNameWithExtension;

                return FileInfo.Name + "." + FileInfo.Extension;
            }
            set { _fileNameWithExtension = value; }
        }
        public string Data { get; set; }

        public FileData(string fileNameWithExtension, byte[] data, string computerHash)
        {
            _fileNameWithExtension = fileNameWithExtension;
            Data = Convert.ToBase64String(data);
            ComputerHash = computerHash;
        }

        public FileData(FileInfo fileInfo, byte[] data, string computerHash)
        {
            FileInfo = FileInfo;
            _fileNameWithExtension = null;
            Data = Convert.ToBase64String(data);
            ComputerHash = computerHash;
        }

        public string ComputerHash { get; set; }
    }
}
