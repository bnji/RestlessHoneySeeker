using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System;
using System.Runtime.InteropServices;

namespace Library
{
    public class NoBitmapDataException : Exception
    {
        public NoBitmapDataException() : base() { }

        public NoBitmapDataException(string message) : base(message) { }

        public NoBitmapDataException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ScreenMan
    {
        #region WIN API for capturing cursor - source: http://stackoverflow.com/questions/6750056/how-to-capture-the-screen-and-mouse-pointer-using-windows-apis
        [StructLayout(LayoutKind.Sequential)]
        struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct POINTAPI
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        const Int32 CURSOR_SHOWING = 0x00000001;
        #endregion

        //public Bitmap BitmapImage { get; private set; }

        private static ScreenMan instance;

        public static ScreenMan Instance
        {
            get
            {
                lock (typeof(ScreenMan))
                {
                    if (instance == null)
                    {
                        instance = new ScreenMan();
                    }
                    return instance;
                }
            }
        }

        private ScreenMan() { }

        public Bitmap Grab(bool CaptureMouse, PixelFormat pixelFormat)
        {
            Bitmap result = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, pixelFormat);
            Rectangle bounds = Screen.GetBounds(Point.Empty);

            try
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    //g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);

                    if (CaptureMouse)
                    {
                        CURSORINFO pci;
                        pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CURSORINFO));

                        if (GetCursorInfo(out pci))
                        {
                            if (pci.flags == CURSOR_SHOWING)
                            {
                                DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                                g.ReleaseHdc();
                            }
                        }
                    }
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }

        public bool Save(Bitmap bitmap, string file, long compression = 90L)//, ImageFormat format)
        {
            if (bitmap == null)
                return false;

            try
            {
                string dir = Path.GetDirectoryName(file);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                EncoderParameters encParameters = new EncoderParameters(1);
                encParameters.Param[0] = new EncoderParameter(Encoder.Quality, compression);
                bitmap.Save(file, jpgEncoder, encParameters);
                return true;
            }
            catch (Exception ex) { }
            return false;
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        /*public void Save(string file, ImageFormat format)
        {
            Bitmap BitmapImage = Grab();
            if (BitmapImage != null)
                Save(BitmapImage, file, format);
            else
                throw new NoBitmapDataException();
        }

        public void Save(string file)
        {
            Save(file, ImageFormat.Jpeg);
        }*/
    }
}
