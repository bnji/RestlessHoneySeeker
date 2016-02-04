using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library
{
    public class JobWorker
    {
        public ECommand Command { get; set; }

        public bool IsDone { get; set; }

        public UploadResult Result { get; set; }

        public JobWorker()
        {
            Result = new UploadResult()
            {
                FileName = string.Empty,
                FileSize = -1,
                Percentage = 0
            };
            IsDone = true;
        }

        public void Start(ECommand command = ECommand.DoNothing)
        {
            this.Command = command;
            IsDone = false;
        }

        public void Stop()
        {
            if (Result != null)
            {
                Result.Percentage = 100;
            }
            this.Command = ECommand.DoNothing;
            IsDone = true;
        }
    }
}
