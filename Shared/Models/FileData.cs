using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Models
{
    [DataContract]
    public class FileData
    {
        [DataMember]
        public string ComputerHash { get; set; }

        [DataMember]
        public FileInfo FileInfo { get; private set; }

        private string fileNameWithExtension;

        [DataMember]
        public string FileNameWithExtension
        {
            get
            {
                if (FileInfo == null)
                    return fileNameWithExtension;

                return FileInfo.Name + "." + FileInfo.Extension;
            }
            set { fileNameWithExtension = value; }
        }

        [DataMember]
        public string Data { get; set; }

        public FileData(string fileNameWithExtension, byte[] data, string computerHash)
        {
            this.FileInfo = null;
            this.fileNameWithExtension = fileNameWithExtension;
            this.Data = Convert.ToBase64String(data);
            this.ComputerHash = computerHash;
        }

        public FileData(FileInfo fileInfo, byte[] data, string computerHash)
        {
            this.FileInfo = fileInfo;
            this.fileNameWithExtension = null;
            this.Data = Convert.ToBase64String(data);
            this.ComputerHash = computerHash;
        }

        public FileData() { }
    }
}
