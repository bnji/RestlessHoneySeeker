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
    public class RhsApi
    {
        //private static string publicKey;
        //private static string privateKey;
        private static readonly string dateformat = "MMM ddd d HH:mm:ss yyyy";// "Y-m-d H:i:s";

        public static Keys Generate()
        {
            return new Keys()
            {
                Public = Security.SHA1(Security.mt_rand_str(40)),
                Private = Security.SHA1(Security.mt_rand_str(40))
            };
        }

        public static string GetFileContents(string path, string file)
        {
            var fullName = GetFile(path, file);
            return System.IO.File.ReadAllText(fullName);
        }

        public static string GetFileContents(string fullpath)
        {
            return System.IO.File.ReadAllText(fullpath);
        }

        public static string GetFile(string path, string file)
        {
            // Some browsers send file names with full path. We only care about the file name.
            return Path.Combine(HttpContext.Current.Server.MapPath(path), Path.GetFileName(file));
        }

        private static Keys GetKeys()
        {
            return JsonConvert.DeserializeObject<Keys>(GetFileContents("~/App_Data", "keys.json"));
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
                    var computersJsonFile = GetFile("~/App_Data/", "computers.json");
                    var computers = new List<ComputerData>();
                    try
                    {
                        computers = JsonConvert.DeserializeObject<List<ComputerData>>(GetFileContents(computersJsonFile));
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
                        computers.Add(computerData);
                    }

                    var computersJson = JsonConvert.SerializeObject(computers);
                    File.WriteAllText(computersJsonFile, computersJson);

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
                    var computersJsonFile = GetFile("~/App_Data/", "computers.json");

                    var computers = new List<ComputerData>();
                    try
                    {
                        computers = JsonConvert.DeserializeObject<List<ComputerData>>(GetFileContents(computersJsonFile));
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
                    System.Threading.Thread.Sleep(250);
                    File.WriteAllText(computersJsonFile, computersJson);

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

        public static string UploadImage(ImageData data)
        {
            try
            {
                var file = GetFile("~/DataFromClient/", data.FileName);
                var content = Convert.FromBase64String(data.Image);
                File.WriteAllBytes(file, content);
                if (data.Image.Length > 0)
                {
                    return file;
                }
                return null;
            }
            catch { }
            return null;
        }

        public static int? UploadFile(FileData data)
        {
            try
            {
                var file = GetFile("~/DataFromClient/", data.FileNameWithExtension);
                byte[] bytes = Convert.FromBase64String(data.Data);
                File.WriteAllBytes(file, bytes);
                return bytes.Length;
            }
            catch { }
            return null;
        }

        public static string DownloadFile(string file)
        {
            byte[] result = null;
            try
            {
                result = File.ReadAllBytes(GetFile("~/DataFromHost/", file));
            }
            catch { }
            return Convert.ToBase64String(result);
        }

        public static Settings GetSettings()
        {
            return JsonConvert.DeserializeObject<Settings>(GetFileContents("~/App_Data", "settings.json"));
        }

        internal static int? SaveSettings(Settings settingsEncoded)
        {
            try
            {
                var file = GetFile("~/App_Data/", "settings.json");
                byte[] bytes = Encoding.Default.GetBytes(JsonConvert.SerializeObject(settingsEncoded));
                File.WriteAllBytes(file, bytes);
                return bytes.Length;
            }
            catch { }
            return null;
        }

        public static string GetComputerHash(string computerName)
        {
            var data = JsonConvert.DeserializeObject<List<ComputerData>>(GetFileContents("~/App_Data", "computers.json"));
            var computer = data.Where(c => c.Name == computerName).FirstOrDefault();
            if (computer != null)
            {
                return Transmitter.GetComputerHash(computer);
            }
            return null;
        }

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
    }
}