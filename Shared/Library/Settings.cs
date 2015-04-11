using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Library
{
    [DataContract]
    public class Settings
    {
        [DataMember]
        public string ComputerHash { get; set; }

        private ECommand _command = ECommand.DO_NOTHING;
        
        [DataMember]
        public ECommand Command
        {
            get { return _command; }
            set
            {
                if (Enum.IsDefined(typeof(ECommand), value))
                    _command = (ECommand)value;
                else
                    _command = value;
            }
        }
        //public long ImageQuality { get; set; }
        //public string FileName { get; set; }
        //public string FileArgs { get; set; }
        //public string FileToUpload { get; set; }
        //public string FileToDownload { get; set; }
        [DataMember]
        public bool HasExectuted { get; set; }

        [DataMember]
        public string File { get; set; }

        [DataMember]
        public string Parameters { get; set; }

        [DataMember]
        public int CursorX { get; set; }

        [DataMember]
        public int CursorY { get; set; }

        [DataMember]
        public string KeyCode { get; set; }

        public Settings()
        {
            Command = ECommand.DO_NOTHING;
            //ImageQuality = 20L
        }
    }
}
