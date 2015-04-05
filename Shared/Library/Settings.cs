using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library
{
    public class Settings
    {
        public string ComputerHash { get; set; }
        private ECommand _command = ECommand.DO_NOTHING;
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
        public bool HasExectuted { get; set; }
        public string File { get; set; }
        public string Parameters { get; set; }
        public int CursorX { get; set; }
        public int CursorY { get; set; }
        public string KeyCode { get; set; }

        public Settings()
        {
            Command = ECommand.DO_NOTHING;
            //ImageQuality = 20L
        }
    }
}
