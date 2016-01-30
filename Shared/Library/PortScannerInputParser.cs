using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Library
{
    public class PortScannerInputParser
    {
        private int portTimeoutTreshold = 10;
        public int PortTimeoutTreshold { get { return portTimeoutTreshold; } private set { portTimeoutTreshold = value; } }
        public int PortStart { get; private set; }
        public int PortEnd { get; private set; }
        public string IPAddressString { get; private set; }
        public IPAddress IPAddress { get; private set; }
        public EPortType PortType { get; private set; }

        private string[] data = null;

        public PortScannerInputParser(string input)
        {
            ParseIPAddress("127.0.0.1");
            PortType = EPortType.TCP;
            data = !string.IsNullOrEmpty(input) ? Regex.Replace(input, @"\s+", "").Split(';') : null;
            Parse();
        }

        /*
         * 80 
         * 80-1024
         * 80;192.168.100.10
         * 80-1024;192.168.100.10
         * 80;192.168.100.10;TCP
         * 80-1024;192.168.100.10;UDP
         * 80;192.168.100.10;TCP;10
         * 80-1024;192.168.100.10;UDP;100
         */
        void Parse()
        {
            if (data == null)
                return;

            if (data.Length >= 1)
            {
                ParsePorts(data[0]);
            }
            if (data.Length >= 2)
            {
                ParseIPAddress(data[1]);
            }
            if (data.Length >= 3)
            {
                PortType = data[2].ToLower().Equals("tcp") ? EPortType.TCP : EPortType.UDP;
            }
            if (data.Length >= 4)
            {
                ParseTimeoutTreshold(data[3]);
            }
        }

        void ParseIPAddress(string input)
        {
            if (NetUtility.isValidUrl(input) || NetUtility.IsValidIP(input))
            {
                IPAddressString = NetUtility.HostToIp(input).ToString();
                IPAddress = IPAddress.Parse(IPAddressString);
            }
        }

        void ParsePorts(string input)
        {
            var ports = input.Split('-');
            if (ports.Length == 1)
            {
                int portStart = 80;
                if (int.TryParse(ports[0], out portStart))
                {
                    PortStart = portStart;
                }
                PortEnd = PortStart;
            }
            else if (ports.Length == 2)
            {
                int portStart = 80;
                if (int.TryParse(ports[0], out portStart))
                {
                    PortStart = portStart;
                }
                int portEnd = 80;
                if (int.TryParse(ports[1], out portEnd))
                {
                    PortEnd = portEnd;
                }
            }
            //var portMax = (int)Math.Pow(8, 2) * 1024 - 1;
            //Check wether if the specified minPort and maxPort are within the bounds of available ports.
            PortStart = PortStart >= IPEndPoint.MinPort && PortStart <= IPEndPoint.MaxPort ? PortStart : 80;
            PortEnd = PortEnd >= IPEndPoint.MinPort && PortEnd <= IPEndPoint.MaxPort ? PortEnd : 80;
            PortEnd = PortEnd >= PortStart ? PortEnd : PortStart;
        }

        void ParseTimeoutTreshold(string input)
        {
            int.TryParse(input, out portTimeoutTreshold);
            PortTimeoutTreshold = portTimeoutTreshold > 1 ? portTimeoutTreshold : 10;
        }
    }
}
