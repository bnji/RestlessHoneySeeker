
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Library
{
    public class PathExt
    {        
        public static string ReformatName(string filename, char separator) {
            char[] invCharArr = Path.GetInvalidFileNameChars();
            foreach (char c in filename.ToCharArray())
            {
                if (invCharArr.Contains(c))
                {
                    filename = filename.Replace(c, separator);
                }
            }
            return filename;
        }

        public static string ReformatName(string filename)
        {
            return ReformatName(filename, '_');
        }
    }
}
