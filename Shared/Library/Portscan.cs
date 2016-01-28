using System;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Reflection;

namespace Library
{
    public enum PortType { TCP, UDP }

    public class PortInfo2
    {

        public string Name { get; set; }
        public int Port { get; set; }
        public PortType Type { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return "Port " + Port + (!string.IsNullOrEmpty(Name) ? (Name + " ") : "") + (!string.IsNullOrEmpty(Description) ? "(" + Description + ")" : "");
        }
    }

    /**
     * http://tools.ietf.org/html/rfc1340#page-7
     * */
    public class PortInfoParser
    {
        public static Dictionary<int, PortInfo2[]> Parse(Stream stream)
        {
            var result = new Dictionary<int, PortInfo2[]>();
            using (StreamReader reader = new StreamReader(stream))
            {
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    if (!string.IsNullOrEmpty(line.Trim()))
                    {
                        var portInfo = new PortInfo2();
                        var from = -1;
                        var lineLength = -1;
                        var totalLength = line.Length;

                        from = 0;
                        lineLength = line.Length >= 18 ? 18 : line.Length - from;
                        portInfo.Name = line.Substring(from, lineLength).Trim();

                        from = lineLength + 1;
                        lineLength = line.Length >= 29 ? 29 : line.Length - from;
                        var portStr = line.Substring(from, (lineLength - from) >= 0 ? (lineLength - from) : 0).Trim();
                        var portStrSplit = portStr.Split('/');
                        var portNumberStr = portStrSplit.Length >= 1 ? portStrSplit[0] : "-1";
                        var portTypeStr = portStrSplit.Length >= 2 ? portStrSplit[1] : "-1";
                        var port = -1;
                        int.TryParse(portNumberStr, out port);
                        portInfo.Port = port;
                        portInfo.Type = portTypeStr.Trim().ToLower().Equals("tcp") ? PortType.TCP : PortType.UDP;

                        from = lineLength + 1 <= line.Length ? lineLength + 1 : line.Length - 1;
                        lineLength = line.Length >= from ? line.Length - from : from;
                        portInfo.Description = Regex.Replace(line.Substring(from, lineLength).Trim(), @"\s+", " ");

                        if (result.ContainsKey(portInfo.Port))
                        {
                            if (string.IsNullOrEmpty(portInfo.Description))
                            {
                                portInfo.Description = result[portInfo.Port][0].Description;
                            }
                            result[portInfo.Port][1] = portInfo;
                        }
                        else
                        {
                            result.Add(portInfo.Port, new PortInfo2[] { portInfo, portInfo });
                        }
                    }
                }
            }
            return result;
        }
    }

    public class PortScanner
    {
        public bool IsReady { get; set; }
        public Dictionary<int, PortInfo2[]> portInfo { get; set; }
        private string scanAddress = "127.0.0.1";
        private int portStart = -1;
        private int portEnd = -1;
        private int portTimeoutTreshold = 10;
        private Queue<ScanResult> _scanResults = new Queue<ScanResult>();

        public string ScanAddress { get { return scanAddress; } set { scanAddress = value; } }
        public int PortStart { get { return portStart; } set { portStart = value; } }
        public int PortEnd { get { return portEnd; } set { portEnd = value; } }
        public int PortTimeoutTreshold { get { return portTimeoutTreshold; } set { portTimeoutTreshold = value; } }
        public Queue<ScanResult> ScanResults { get { return _scanResults; } set { _scanResults = value; } }

        public PortInfo2 GetPortInfo(int port, PortType type = PortType.TCP) {
            if (IsReady && portInfo != null)
            {
                if (portInfo.ContainsKey(port))
                {
                    var portInfoList = portInfo[port];
                    if (portInfoList.Length >= 1 && type == PortType.TCP)
                    {
                        return portInfoList[0];
                    }
                    else if (portInfoList.Length == 2 && type == PortType.UDP)
                    {
                        return portInfoList[1];
                    }
                }
            }
            return null;
        }

        public static bool CheckPortStateTcp(IPAddress _address, int _port, int _waitTimeoutMS)
        {
            TcpClient tcpClient = new TcpClient();
            try
            {
                tcpClient.BeginConnect(_address, _port, null, null);
                Thread.Sleep(_waitTimeoutMS);
                return tcpClient.Connected;
            }
            catch
            {
                return false;
            }
            finally
            {
                tcpClient.Close();
            }
        }

        public static bool CheckPortStateUdp(IPAddress _address, int _port, int _waitSeconds)
        {
            // This constructor arbitrarily assigns the local port number.
            UdpClient udpClient = new UdpClient(_port);
            try
            {
                udpClient.Connect(_address, _port);

                // Sends a message to the host to which you have connected.
                Byte[] sendBytes = Encoding.ASCII.GetBytes("hello?");

                udpClient.Send(sendBytes, sendBytes.Length);

                // Sends a message to a different host using optional hostname and port parameters.
                UdpClient udpClientB = new UdpClient();
                udpClientB.Send(sendBytes, sendBytes.Length, "AlternateHostMachineName", _port);

                //IPEndPoint object will allow us to read datagrams sent from any source.
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                // Blocks until a message returns on this socket from a remote host.
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);

                // Uses the IPEndPoint object to determine which of these two hosts responded.
                //Console.WriteLine("This is the message you received " + returnData.ToString());
                //Console.WriteLine("This message was sent from " + RemoteIpEndPoint.Address.ToString() + " on their port number " + RemoteIpEndPoint.Port.ToString());

                udpClient.Close();
                udpClientB.Close();
                Console.WriteLine(returnData.ToString());
                return returnData.Length > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public PortScanner(string _scanAddress, int _portTimeoutTreshold)
        {
            this.scanAddress = _scanAddress;
            this.portTimeoutTreshold = _portTimeoutTreshold;
            if (this.portTimeoutTreshold < 1)
            {
                throw new TooLowTimeoutValueException();
            }
        }

        private string logFileDest = "";

        public PortScanner(string _scanAddress, int _portStart, int _portEnd, int _portTimeoutTreshold, string _logFileDest = "")
        {
            var portMax = (int) Math.Pow(8, 2) * 1024 - 1;
            this.scanAddress = _scanAddress;
            this.portStart = _portStart;
            this.portEnd = _portEnd <= portMax ? _portEnd : portMax;
            this.portTimeoutTreshold = _portTimeoutTreshold;
            this.logFileDest = _logFileDest;
            if (this.portTimeoutTreshold < 1)
            {
                throw new TooLowTimeoutValueException();
            }

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "bendot.Net.port_info.txt";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                portInfo = PortInfoParser.Parse(stream);
                IsReady = true;
            }

        }

        public static string GetMinTresholdWarning { get { return "Scan timeout has to be atleast 1. Usually a value between 10 and 50 should show some results..."; } }


        IPAddress ScanIPAddress;

        private string openPorts;

        public string OpenPorts { get { return openPorts; } set { openPorts = value; } }

        public class ScanResult
        {
            public bool IsOpen { get; set; }
            public int Port { get; set; }

            public override string ToString()
            {
                var portDescription = PortInfo != null ? " (" + PortInfo.Description + ")" : "";
                var portIsOpen = "";
                if (IsOpen)
                {
                    portIsOpen += " is OPEN !!!";
                }
                else
                {
                    portIsOpen += "  is closed";
                }
                return Port + portDescription + portIsOpen;
                //return Port + " is " + (IsOpen == true ? "Open" : "Closed") + ".";
            }

            public PortInfo2 PortInfo { get; set; }
        }

        private ScanResult PortScanTcpResult(int port)
        {
            var isOpen = CheckPortStateTcp(ScanIPAddress, port, this.portTimeoutTreshold);
            var portInfo = GetPortInfo(port, PortType.TCP);
            if (isOpen)
            {
                openPorts += port + ", ";
                //return "Discovered open port *** " + port + " **** /tcp on " + ScanIPAddress;
            }
            else
            {
                //return "Port " + port + " /tcp is closed";
            }
            return new ScanResult() { IsOpen = isOpen, Port = port, PortInfo = portInfo };
        }

        private ScanResult PortScanUdpResult(int port)
        {
            var isOpen = CheckPortStateUdp(ScanIPAddress, port, this.portTimeoutTreshold);
            var portInfo = GetPortInfo(port, PortType.UDP);
            if (isOpen)
            {
                openPorts = port + " ";
                //return "Discovered open port " + port + "/udp on " + ScanIPAddress;
            }
            return new ScanResult() { IsOpen = isOpen, Port = port, PortInfo = portInfo };
        }

        public void setPort(string _portRange)
        {
            try
            {
                if (_portRange.Contains("-"))
                {
                    string[] temp = _portRange.Split('-');
                    portStart = Convert.ToInt32(temp[0]);
                    portEnd = Convert.ToInt32(temp[1]);
                }
                else
                {
                    portStart = Convert.ToInt32(_portRange);
                    portEnd = portStart;
                }
            }
            catch (Exception)
            {
                throw new NotValidInputException();
            }
        }

        private bool portScanFinished = false;
        public bool PortScanFinished { get { return portScanFinished; } }

        private bool displayClosedPorts = true;
        public bool DisplayClosedPorts { get { return displayClosedPorts; } set { displayClosedPorts = value; } }

        public int CurrentPort { get; set; }

        public List<ScanResult> FoundPorts { get; private set; }

        public bool RunScan(PortType portType = PortType.TCP)
        {
            FoundPorts = new List<ScanResult>();
            CurrentPort = 0;
            this.portScanFinished = false;
            openPorts = "";
            ScanResults.Clear();

            //General.DisplayMessage(NetUtility.isValidUrl(scanAddress));
            //General.DisplayMessage(bendot.Text.RegExValidate.Process(Text.ValidationTypes.URL2, "http://" + scanAddress));


            if (NetUtility.CheckConnection())
            {
                if (NetUtility.isValidUrl(scanAddress) || NetUtility.IsValidIP(scanAddress))
                {
                    scanAddress = NetUtility.HostToIp(scanAddress).ToString();

                    if (NetUtility.IsHostAlive(NetUtility.HostToIp(scanAddress), 3000))
                    {
                        ScanIPAddress = IPAddress.Parse(scanAddress);

                        //Check wether if the specified minPort and maxPort are within the bounds of available ports.
                        if (portStart >= IPEndPoint.MinPort && portEnd <= IPEndPoint.MaxPort)
                        {
                            if (portStart <= portEnd)
                            {
                                var data = new StringBuilder();
                                //while(portStart <= portEnd)
                                for (CurrentPort = portStart; CurrentPort <= portEnd + 1; CurrentPort++)
                                {
                                    //try {
                                    ScanResult result;
                                    //string udpResult;
                                    if (portType == PortType.TCP)
                                    {
                                        result = PortScanTcpResult(CurrentPort);
                                    }
                                    else
                                    {
                                        result = PortScanUdpResult(CurrentPort);
                                    }
                                    if (result != null)
                                    {
                                        if (result.IsOpen)
                                        {
                                            FoundPorts.Add(result);
                                        }
                                        data.AppendLine(result.ToString());
                                        ScanResults.Enqueue(result);
                                    }
                                }
                                var file = this.logFileDest;
                                if (file == ".")
                                {
                                    file = Environment.CurrentDirectory;
                                }
                                if (Directory.Exists(file))
                                {
                                    file = Path.Combine(file, "log.txt");
                                    File.WriteAllText(file, data.ToString());
                                }
                            }
                            else
                            {
                                throw new PortStartExceedsPortEndException();
                            }
                        }
                        else
                        {
                            throw new IndexOutOfRangeException();
                        }
                    }
                    else
                    {
                        throw new HostUnreachableException();
                    }
                }
            }


            /*else if (!LookupDNSName(scanAddress, out ScanIPAddress))
            {
                //return "Error looking up :" + scanAddress;
                //scanResults.Add("Error looking up :" + scanAddress);
                throw new NotValidIpAddressException();
            }*/
            this.portScanFinished = true;
            //if (openPorts.Contains(","))
            //    Console.WriteLine("\nOpen ports: " + openPorts.Remove(openPorts.LastIndexOf(",")) + " on target: " + this.scanAddress);
            //Console.WriteLine("Portscan finished!");
            //Console.WriteLine("Press any key to continue...");
            return this.portScanFinished;
        }

        /*static bool LookupDNSName(string ScanAddress, out IPAddress ScanIPAddress)
        {
            ScanIPAddress = null;
            IPHostEntry NameToIpAddress;

            try
            {
                // Lookup the address we are going to scan
                NameToIpAddress = Dns.GetHostEntry(ScanAddress);
            }
            catch (SocketException)
            {
                // Thrown when we are unable to lookup the name
                return false;
            }

            // Pick the first address in the list , there should be at least 1
            if (NameToIpAddress.AddressList.Length > 0)
            {
                ScanIPAddress = NameToIpAddress.AddressList[0];
                return true;
            }

            return false;
        }*/

        public bool CanPrintClosedPorts { get; set; }

        public double PercentageDone
        {
            get
            {
                return Extensions.Clamp((double)CurrentPort / (double)PortEnd * 100, 0.0, 100.0);
            }
        }
    }

    public class NotValidIpAddressException : Exception
    {
        public NotValidIpAddressException() : base() { }
        public NotValidIpAddressException(string message) : base(message) { }
        public NotValidIpAddressException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected NotValidIpAddressException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }

    public class NotValidInputException : Exception
    {
        public NotValidInputException() : base() { }
        public NotValidInputException(string message) : base(message) { }
        public NotValidInputException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected NotValidInputException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }

    public class HostUnreachableException : Exception
    {
        public HostUnreachableException() : base() { }
        public HostUnreachableException(string message) : base(message) { }
        public HostUnreachableException(string message, Exception inner) : base(message, inner) { }

        // A constructor is needed for serialization when an
        // exception propagates from a remoting server to the client. 
        protected HostUnreachableException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
    }
    public class TooLowTimeoutValueException : Exception
    {
        public TooLowTimeoutValueException() : base() { }
    }
    public class NoInternetConnectionException : Exception
    {
        public NoInternetConnectionException() : base() { }
    }
    public class PortStartExceedsPortEndException : Exception
    {
        public PortStartExceedsPortEndException() : base() { }
    }
}