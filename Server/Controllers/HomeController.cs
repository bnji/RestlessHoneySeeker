using Server.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Server.Controllers
{
    //[Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetData()
        {
            return Json(GetFileContents("DataFromClient", "data.txt"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSentences()
        {
            return Json(GetFileContents("DataFromClient", "latestsentences.txt"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPortInfo()
        {
            return Json(GetFileContents("DataFromClient", "portinfo.txt"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetComputers()
        {
            return Json(GetFileContents("Data", "computers.json"), JsonRequestBehavior.AllowGet);
        }

        //[NoCache]
        [HttpGet]
        //[AcceptVerbs(HttpVerbs.Get)]
        //[OutputCache(CacheProfile = "Images")]
        public FileResult GetImage(string image)
        {
            //return Content(GetFileContents("DataFromClient", "latest.jpg"));
            var path = GetFile("DataFromClient", image);
            return base.File(path, "image/jpeg");
            //using (var fs = new FileStream(path, FileMode.Open))
            //{
            //    return new FileStreamResult(fs, "image/jpeg");
            //}
        }

        string GetFileContents(string path, string file)
        {
            return System.IO.File.ReadAllText(GetFile(path, file));
        }

        private string GetFile(string path, string file)
        {
            // Some browsers send file names with full path. We only care about the file name.
            return Path.Combine(Server.MapPath("~/App_Data/" + path), Path.GetFileName(file));
        }
    }
}
