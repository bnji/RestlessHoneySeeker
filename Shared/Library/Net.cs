using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Library
{
    public class Net
    {
        public static IPAddress GetExternalIpAddress(int timeout)
        {
            IPAddress ipAddress = IPAddress.Loopback; // should be localhost or null to determine failure to obtain ip?
            try
            {
                string ipAddressString = "";
                WebRequest wr = HttpWebRequest.Create("http://icanhazip.com");
                wr.UseDefaultCredentials = true;
                wr.Timeout = timeout;
                HttpWebResponse response = (HttpWebResponse)wr.GetResponse();
                using (Stream dataStream = response.GetResponseStream())
                {
                    using (StreamReader sr = new StreamReader(dataStream))
                    {
                        ipAddressString = sr.ReadToEnd();
                        IPAddress.TryParse(ipAddressString.Trim(), out ipAddress);
                        sr.Close();
                    }
                    dataStream.Close();
                }
            }
            catch (WebException webex) { }
            catch (Exception ex) { }
            return ipAddress;
        }

        public static IPAddress GetInternalIpAddress()
        {
            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                return null;
            }
            IPHostEntry ipHostEntry = Dns.GetHostEntry(Dns.GetHostName());
            return ipHostEntry
                .AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        }
    }
}
