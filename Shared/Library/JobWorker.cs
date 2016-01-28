using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Library
{
    public class JobWorker
    {
        public bool IsDone { get; set; }

        public UploadResult Result { get; set; }

        public JobWorker()
        {
            Result = null;
            IsDone = true;
        }

        public void Start(string fileName = "", int fileSize = -1)
        {
            UploadResult result = new UploadResult()
            {
                FileName = fileName,
                FileSize = fileSize,
                Percentage = 0
            };
            Result = result;
            IsDone = false;
            //return string.IsNullOrEmpty(fileName) && fileSize == -1;
        }

        public void Stop()
        {
            //Result = null;
            Result.Percentage = 100;
            IsDone = true;
        }
    }
}
