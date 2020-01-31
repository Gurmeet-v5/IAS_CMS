using System;
using System.IO;
using System.Web;

namespace KISD.Common
{
    /// <summary>
    /// Summary description for fileupload.
    /// </summary>
    public class fileupload : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            if (context.Request.Files.Count > 0)
            {
                var fileName = "";
                HttpFileCollection files = context.Request.Files;
                for (int i = 0; i < files.Count; i++)
                {
                    HttpPostedFile file = files[i];
                    KISD.Areas.Admin.Models.Common.CreateDocFolder();
                    fileName = Path.GetFileName(file.FileName);
                    var fileExtension = fileName.Substring(fileName.LastIndexOf("."), fileName.Length - fileName.LastIndexOf("."));
                    Guid gui;
                    gui = Guid.NewGuid();
                    fileName = gui.ToString() + fileExtension;

                    var fname = context.Server.MapPath("~/Webdata/Resume/" + fileName);
                    file.SaveAs(fname);
                }
                context.Response.ContentType = "text/plain";
                context.Response.Write("~/Webdata/Resume/" + fileName);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}