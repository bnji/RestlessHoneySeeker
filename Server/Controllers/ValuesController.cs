using Models;
using Newtonsoft.Json;
using Library;
using Server.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace Server.Controllers
{
    public class ValuesController : ApiController
    {
        private static string responseType = "json";

        [HttpGet]
        public HttpResponseMessage Generate()
        {
            return Request.GetResponse(RhsApi.Generate(), responseType);
        }

        [HttpPost]
        public HttpResponseMessage Authorize(AuthData data)
        {
            return Request.GetResponse<AuthResult>(RhsApi.Authorize(data), responseType);
        }

        [HttpPost]
        public HttpResponseMessage DeAuthorize(AuthData data)
        {
            return Request.GetResponse(RhsApi.DeAuthorize(data), responseType);
        }

        [HttpGet]
        public HttpResponseMessage GetDateTime(AuthData data)
        {
            return Request.GetResponse(RhsApi.GetDateTimeFormatted(), responseType);
        }

        [HttpPost]
        public HttpResponseMessage UploadImage(ImageData data)
        {
            //data = new ImageData() { Image = Library.ScreenMan.Instance.Grab(true, System.Drawing.Imaging.PixelFormat.Format24bppRgb) };
            return Request.GetResponse<int?>(RhsApi.UploadImage(data.Image), responseType);
        }

        [HttpPost]
        public HttpResponseMessage UploadFile(FileData data)
        {
            return Request.GetResponse<int?>(RhsApi.UploadFile(data), responseType);
        }

        [HttpGet]
        public HttpResponseMessage GetSettings()
        {
            return Request.GetResponse(RhsApi.GetSettings(), responseType);
        }

        [HttpPost]
        public HttpResponseMessage SaveSettings(Settings settingsEncoded)
        {
            return Request.GetResponse(RhsApi.SaveSettings(settingsEncoded), responseType);
        }

        [HttpGet]
        public HttpResponseMessage GetComputerHash(string computerName)
        {
            return Request.GetResponse(RhsApi.GetComputerHash(computerName), responseType);
        }

        //[HttpPost]
        //public HttpResponseMessage DownloadFile(string file)
        //{
        //    return Request.GetResponse(RhsApi.DownloadFile(file), responseType);
        //}

        [HttpPost]
        public HttpResponseMessage DownloadFile(Settings settings)
        {
            return Request.GetResponse<string>(RhsApi.DownloadFile(settings.File), responseType);
        }
    }
}