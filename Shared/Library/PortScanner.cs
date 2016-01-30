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
    /**
     * Port info overview: http://tools.ietf.org/html/rfc1340#page-7
     * */
    public class PortScanner
    {
        public Dictionary<int, PortScannerPortInfo[]> portInfo { get; set; }
        public PortScannerInputParser Data { get; private set; }
        private int portTimeoutTreshold = 10;
        private Queue<PortScannerResult> _scanResults = new Queue<PortScannerResult>();

        public int PortTimeoutTreshold { get { return portTimeoutTreshold; } set { portTimeoutTreshold = value; } }
        public Queue<PortScannerResult> ScanResults { get { return _scanResults; } set { _scanResults = value; } }

        public PortScannerPortInfo GetPortInfo(int port, EPortType type = EPortType.TCP)
        {
            if (portInfo != null)
            {
                if (portInfo.ContainsKey(port))
                {
                    var portInfoList = portInfo[port];
                    if (portInfoList.Length >= 1 && type == EPortType.TCP)
                    {
                        return portInfoList[0];
                    }
                    else if (portInfoList.Length == 2 && type == EPortType.UDP)
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

        public PortScanner(string input)
        {
            Data = new PortScannerInputParser(input);
        }

        private PortScannerResult GetTcpResult(int port)
        {
            return new PortScannerResult()
            {
                IsOpen = CheckPortStateTcp(Data.IPAddress, port, this.portTimeoutTreshold),
                Port = port,
                PortInfo = GetPortInfo(port, EPortType.TCP)
            };
        }

        private PortScannerResult GetUdpResult(int port)
        {
            return new PortScannerResult()
            {
                IsOpen = CheckPortStateUdp(Data.IPAddress, port, this.portTimeoutTreshold),
                Port = port,
                PortInfo = GetPortInfo(port, EPortType.UDP)
            };
        }

        public int CurrentPort { get; set; }
        public List<PortScannerResult> FoundPorts { get; private set; }

        public void Start()
        {
            FoundPorts = new List<PortScannerResult>();
            ScanResults.Clear();
            if (NetUtility.CheckConnection())
            {
                if (NetUtility.IsHostAlive(Data.IPAddressString, 3000))
                {
                    for (CurrentPort = Data.PortStart; CurrentPort <= Data.PortEnd; CurrentPort++)
                    {
                        PortScannerResult result;
                        if (Data.PortType == EPortType.TCP)
                        {
                            result = GetTcpResult(CurrentPort);
                        }
                        else
                        {
                            result = GetUdpResult(CurrentPort);
                        }
                        if (result != null)
                        {
                            if (result.IsOpen)
                            {
                                FoundPorts.Add(result);
                            }
                            ScanResults.Enqueue(result);
                        }
                    }
                }
                else
                {
                    //throw new HostUnreachableException();
                }
            }
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

        public double PercentageDone
        {
            get
            {
                return Extensions.Clamp((double)CurrentPort / (double)Data.PortEnd * 100, 0.0, 100.0);
            }
        }
    }
}