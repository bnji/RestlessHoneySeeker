using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ServerV2.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Index2()
        {
            return View();
        }

        public ActionResult Console()
        {
            return View();
        }

        public ActionResult GetProcesses()
        {
            return Json(GetFileContents("~/App_Data/DataFromClient/" , "processes.txt"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFileEvents()
        {
            return Json(GetFileContents("~/App_Data/DataFromClient/", "fileevents.txt"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPortInfo()
        {
            return Json(GetFileContents("~/App_Data/DataFromClient/", "portinfo.txt"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetComputers()
        {
            var contents = GetFileContents("~/App_Data/", "computers.json");
            return Json(contents, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetImageLastAccessTime(string image)
        {
            var path = GetFileFullPath("~/Data/DataFromClient/", image);
            var fi = new FileInfo(path);
            return Content("" + fi.LastWriteTimeUtc.Ticks);
        }

        [HttpGet]
        public ActionResult GetAvailableCommands()
        {
            return Json(Enum.GetNames(typeof(Library.ECommand)).ToList(), JsonRequestBehavior.AllowGet);//.Cast<Library.ECommand>(), JsonRequestBehavior.AllowGet);
        }

        //[NoCache]
        [HttpGet]
        //[AcceptVerbs(HttpVerbs.Get)]
        //[OutputCache(CacheProfile = "Images")]
        public FileResult GetImage(string image)
        {
            //return Content(GetFileContents("~/DataFromClient", "latest.jpg"));
            var path = GetFileFullPath("~/App_Data/DataFromClient/", image);
            return base.File(path, "image/jpeg");
            //using (var fs = new FileStream(path, FileMode.Open))
            //{
            //    return new FileStreamResult(fs, "image/jpeg");
            //}

            //var cd = new System.Net.Mime.ContentDisposition
            //{
            //    FileName = GetFile("DataFromClient", image),
            //    Inline = false
            //};
            //Response.AppendHeader("Content-Disposition", cd.ToString());
            //try
            //{
            //    return base.File(System.IO.File.ReadAllBytes(cd.FileName), System.Net.Mime.MediaTypeNames.Image.Jpeg, Path.GetFileName(cd.FileName));
            //}
            //catch (Exception)
            //{
            //    // handle
            //}
            //return null;
        }

        [HttpGet]
        public FileResult GetFile(string computerHash, string file, bool inline = false)
        {
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = GetFileFullPath(Path.Combine("~/App_Data/DataFromClient", computerHash), file),
                Inline = inline
            };
            //Response.AppendHeader("Content-Disposition", cd.ToString());
            try
            {
                return base.File(System.IO.File.ReadAllBytes(cd.FileName), System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(cd.FileName));
            }
            catch (Exception)
            {
                // handle
            }
            return null;
        }

        string GetFileContents(string path, string file)
        {
            return System.IO.File.ReadAllText(GetFileFullPath(path, file));
        }

        private string GetFileFullPath(string path, string file)
        {
            // Some browsers send file names with full path. We only care about the file name.
            return Path.Combine(Server.MapPath(path), Path.GetFileName(file));
        }

        [HttpPost]
        public ActionResult UploadFile()//IEnumerable<HttpPostedFileBase> files)
        {
            var file = HttpContext.Request.Files["UploadedFile"];
            if (file != null)
            {
                file.SaveAs(GetFileFullPath("~/App_Data/DataFromHost/", Path.GetFileName(file.FileName)));
                return Content("");
            }
            return Content("error");
            //return SaveFile(files);
        }

        //private ActionResult SaveFile(IEnumerable<HttpPostedFileBase> files)
        //{
        //    // The Name of the Upload component is "files"
        //    if (files != null)
        //    {
        //        foreach (var file in files)
        //        {
        //            file.SaveAs(GetFile("Data", Path.GetFileName(file.FileName)));
        //        }
        //    }
        //    return Content("");
        //}
    }
}
