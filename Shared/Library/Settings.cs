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

        private ECommand command = ECommand.DO_NOTHING;
        
        [DataMember]
        public ECommand Command
        {
            get { return command; }
            set
            {
                if (Enum.IsDefined(typeof(ECommand), value))
                    command = (ECommand)value;
                else
                    command = value;
            }
        }

        //private TransmitterStatus status = TransmitterStatus.IDLE;

        //[DataMember]
        //public TransmitterStatus Status
        //{
        //    get { return status; }
        //    set
        //    {
        //        if (Enum.IsDefined(typeof(TransmitterStatus), value))
        //            status = (TransmitterStatus)value;
        //        else
        //            status = value;
        //    }
        //}

        //public long ImageQuality { get; set; }
        //public string FileName { get; set; }
        //public string FileArgs { get; set; }
        //public string FileToUpload { get; set; }
        //public string FileToDownload { get; set; }
        //[DataMember]
        //public bool HasExectuted { get; set; }

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
