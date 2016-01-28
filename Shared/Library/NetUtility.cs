using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Library
{
    public class NetUtility
    {
        public static bool isValidUrl(string url)
        {
            if (url.StartsWith("www."))
            {
                url = "http://" + url;
            }
            string pattern = @"^(http|https|ftp)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$";
            Regex reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            return reg.IsMatch(url);
        }
        
        ///<summary>
        /// Validate that String is an IP address.
        /// Between 0.0.0.0 and 255.255.255.255
        ///</summary>
        public static bool IsValidIP(string ipString)
        {
            IPAddress ipAdd;
            return IPAddress.TryParse(ipString, out ipAdd);
        }

        public static string HostToIp(string hostname)
        {
            string ip = "";
            try
            {
                IPAddress[] addresslist = Dns.GetHostAddresses(hostname);
                foreach (IPAddress theaddress in addresslist)
                {
                    ip = theaddress.ToString();
                }
            }
            catch (SocketException socketEx)
            {
                // Handle exception... nah....
            }
            return ip;
        }

        public static bool IsHostAlive(string _address, int _pingTimeOut)
        {
            bool isAlive = false;
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            // Use the default Ttl value which is 128,
            // but change the fragmentation behavior.
            options.DontFragment = true;
            // Create a buffer of 32 bytes of data to be transmitted.
            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = _pingTimeOut;
            PingReply reply = pingSender.Send(_address, timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
            {
                isAlive = true;
            }
            return isAlive;
        }

        public static bool CheckConnection()
        {
            try
            {
                System.Net.IPHostEntry objIPHE = System.Net.Dns.GetHostEntry("www.google.com");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
