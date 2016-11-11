//using Microsoft.TeamFoundation.Common;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.DirectoryServices;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.NetworkInformation;
//using System.Text;

//namespace Library
//{
//    public class FirewallManager
//    {
//        private static FirewallManager instance;

//        public static FirewallManager Instance
//        {
//            get
//            {
//                lock (typeof(FirewallManager))
//                {
//                    if (instance == null)
//                    {
//                        instance = new FirewallManager();
//                    }
//                    return instance;
//                }
//            }
//        }

//        private FirewallManager() { }

//        /// <summary>
//        /// Obtain the list of authorized ports
//        /// </summary>
//        /// <returns></returns>
//        public List<INetFwOpenPort> GetAuthOpenPortsList()
//        {
//            List<INetFwOpenPort> openPorts = new List<INetFwOpenPort>();
//            try
//            {
//                INetFwOpenPorts ports;
//                INetFwOpenPort port;
//                Type NetFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
//                INetFwMgr mgr = (INetFwMgr)Activator.CreateInstance(NetFwMgrType);
//                ports = (INetFwOpenPorts)mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;
//                IEnumerator enumerate = ports.GetEnumerator();
//                while (enumerate.MoveNext())
//                {
//                    port = (INetFwOpenPort)enumerate.Current;
//                    openPorts.Add(port);
//                }
//            }
//            catch (Exception ex) { }
//            return openPorts;
//        }

//        public List<ShareInfo> GetShareInfo()
//        {
//            var shareInfo = new List<ShareInfo>();
//            var gns = new GetNetShares();
//            var shares = gns.EnumNetShares("127.0.0.1");
//            foreach (var share in shares)
//            {
//                shareInfo.Add(new ShareInfo() { Name = share.shi1_netname, Remark = share.shi1_remark, Type = share.shi1_type });
//            }
//            return shareInfo;
//        }

//        public List<PortInfo> GetPortInfo()
//        {
//            List<PortInfo> portInfo = new List<PortInfo>();

//            string ip = Convert.ToString(Net.GetExternalIpAddress(30000));
//            foreach (INetFwOpenPort port in GetAuthOpenPortsList())
//            {
//                portInfo.Add(new PortInfo()
//                {
//                    IP = ip,
//                    Port = port.Port,
//                    Name = port.Name
//                });
//                //sb.AppendLine(ip + ":" + port.Port + " - " + port.Name + " - " + port.Enabled + " - " + port.IpVersion);
//                Console.WriteLine(port.Port + ", " + port.Name);
//            }
//            /*StringBuilder sb = new StringBuilder();
//            foreach (INetFwOpenPort port in GetAuthOpenPortsList())
//            {
                
//                sb.AppendLine(ip + ":" + port.Port + " - " + port.Name + " - " + port.Enabled + " - " + port.IpVersion);
//                Console.WriteLine(port.Port + ", " + port.Name);
//            }
//            System.Windows.Forms.MessageBox.Show(sb.ToString());*/
//            return portInfo;
//        }

//        public bool AddPort(ushort portNumber, String appName)
//        {
//            bool result = false;
//            try
//            {
//                INetFwMgr fwMgr = (INetFwMgr)getInstance("INetFwMgr");
//                INetFwPolicy fwPolicy = fwMgr.LocalPolicy;
//                INetFwProfile fwProfile = fwPolicy.CurrentProfile;
//                INetFwOpenPorts ports = fwProfile.GloballyOpenPorts;
//                INetFwOpenPort port = (INetFwOpenPort)getInstance("INetOpenPort");
//                port.Port = portNumber; /* port no */
//                port.Name = appName; /*name of the application using the port */
//                port.Enabled = true; /* enable the port */
//                /*other properties like Protocol, IP Version can also be set accordingly
//                now add this to the GloballyOpenPorts collection */

//                Type NetFwMgrType = Type.GetTypeFromProgID("HNetCfg.FwMgr", false);
//                INetFwMgr mgr = (INetFwMgr)Activator.CreateInstance(NetFwMgrType);
//                ports = (INetFwOpenPorts)mgr.LocalPolicy.CurrentProfile.GloballyOpenPorts;

//                ports.Add(port);
//                result = true;
//            }
//            catch (UnauthorizedAccessException ex) { result = false; }
//            return result;
//        }

//        protected Object getInstance(String typeName)
//        {
//            if (typeName == "INetFwMgr")
//            {
//                Type type = Type.GetTypeFromCLSID(new Guid("{304CE942-6E39-40D8-943A-B913C40C9CD4}"));
//                return Activator.CreateInstance(type);
//            }
//            else if (typeName == "INetAuthApp")
//            {
//                Type type = Type.GetTypeFromCLSID(new Guid("{EC9846B3-2762-4A6B-A214-6ACB603462D2}"));
//                return Activator.CreateInstance(type);
//            }
//            else if (typeName == "INetOpenPort")
//            {
//                Type type = Type.GetTypeFromCLSID(new Guid("{0CA545C6-37AD-4A6C-BF92-9F7610067EF5}"));
//                return Activator.CreateInstance(type);
//            }
//            else
//                return null;
//        }

//        public List<LANComputerInfo> GetLANComputers()
//        {
//            var lanComputers = new List<LANComputerInfo>();
//            DirectoryEntry root = new DirectoryEntry("WinNT:");
//            foreach (DirectoryEntry computers in root.Children)
//            {
//                foreach (DirectoryEntry computer in computers.Children)
//                {
//                    if (computer.Name != "Schema")
//                    {
//                        lanComputers.Add(new LANComputerInfo() { Name = computer.Name });
//                    }
//                }
//            }
//            return lanComputers;
//        }

//        public List<GatewayInfo> GetGetwayInfo()
//        {
//            var gateways = new List<GatewayInfo>();
//            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
//            foreach (NetworkInterface adapter in adapters)
//            {
//                IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
//                GatewayIPAddressInformationCollection addresses = adapterProperties.GatewayAddresses;
//                if (addresses.Count > 0)
//                {
//                    //Console.WriteLine(adapter.Description);
//                    foreach (GatewayIPAddressInformation address in addresses)
//                    {
//                        gateways.Add(new GatewayInfo() { AdapterDescription = adapter.Description, Address = address.Address.ToString() });
//                    }
//                }
//            }
//            return gateways;
//        }
//    }

//    public class GatewayInfo
//    {
//        public string AdapterDescription { get; set; }
//        public string Address { get; set; }
//    }

//    public class LANComputerInfo
//    {

//        public string Name { get; set; }
//    }

//    public class PortInfo
//    {
//        public string IP { get; set; }
//        public int Port { get; set; }
//        public string Name { get; set; }

//    }

//    public class ShareInfo
//    {
//        public string Name { get; set; }

//        public string Remark { get; set; }

//        public uint Type { get; set; }
//    }
//}
