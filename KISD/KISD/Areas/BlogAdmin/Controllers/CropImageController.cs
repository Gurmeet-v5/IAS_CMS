using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KISD.Areas.BlogAdmin.Models;

namespace KISD.Areas.BlogAdmin.Controllers
{
    [Authorize]
    [SessionExpire]
    public class CropImageController : Controller
    {
        /// <summary>
        /// This Method will get uploaded image and show full image to crop.
        /// </summary>
        /// <param name="width"> Ideal width for image </param>
        /// <param name="height">Ideal height for image </param>
        /// <param name="imagename">Image name as guid.</param>
        /// <returns></returns>
        public ActionResult Index(int width, int height, string imagename, string FileuploaderCss)
        {
            var model = new CropImageModel();
            model.ImageName = imagename;
            var image = System.Drawing.Image.FromFile(Server.MapPath("~/WebData/Cropped/" + imagename));
            if (image != null && (image.Width < width || image.Height < height))
            {
                var dimensionwidth = 0;
                var dimensionheight = 0;
                dimensionwidth = (image.Width < width) ? width + 100 : image.Width + 100;
                dimensionheight = (image.Height < height) ? height + 100 : image.Height + 100;
                var szDimensions = new Size(dimensionwidth, dimensionheight);
                Bitmap resizedImg = new Bitmap(szDimensions.Width, szDimensions.Height);
                Graphics gfx = Graphics.FromImage(resizedImg);
                gfx.FillRectangle(Brushes.White, 0, 0, resizedImg.Width, resizedImg.Height);
                // Paste source image on blank canvas, then save it as .png
                var xrectangle = (dimensionwidth) / 2;
                var yrectangle = (dimensionheight) / 2;
                var ximage = image.Width / 2;
                var yimage = image.Height / 2;
                gfx.DrawImageUnscaled(image, xrectangle - ximage, yrectangle - yimage);
                image.Dispose();
                image = null;
                resizedImg.Save(Request.PhysicalApplicationPath + "WebData\\Cropped\\" + imagename);
                resizedImg.Dispose();
                gfx.Dispose();
            }
            else
            {
                image.Dispose();
                image = null;
            }
            model.FileUploaderCss = FileuploaderCss;
            model.Imagepath = "~/WebData/Cropped/" + imagename;
            model.Height = height;
            model.Width = width;
            return View(model);
        }
        /// <summary>
        /// Get the model of cropped image,save the cropeed image with ideal size. delete the original image which was uploaded.
        /// </summary>
        /// <param name="model">Crop Image Model</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(CropImageModel model, string command)
        {
            try
            {
                if (string.IsNullOrEmpty(command))
                {
                    try
                    {
                        var image = System.Drawing.Image.FromFile(Server.MapPath("~/WebData/Cropped/" + model.ImageName));
                        var cropcords = new Rectangle(model.Xaxis, model.Yaxis, model.Width, model.Height);
                        string cfname, cfpath;
                        var bitMap = new Bitmap(cropcords.Width, cropcords.Height);
                        var grph = Graphics.FromImage(bitMap);
                        grph.DrawImage(image, new Rectangle(0, 0, bitMap.Width, bitMap.Height), cropcords, GraphicsUnit.Pixel);
                        cfname = "crop_" + model.ImageName;
                        cfpath = Path.Combine(Server.MapPath("~/WebData/Cropped"), cfname);
                        bitMap.Save(cfpath);
                        if (string.IsNullOrEmpty(model.FileUploaderCss))
                        {
                            TempData["CroppedImage"] = "~/WebData/Cropped/" + cfname;
                        }
                        else
                        {
                            TempData[model.FileUploaderCss] = "~/WebData/Cropped/" + cfname;
                        }
                        model.IsFileCroped = true;
                        image.Dispose();
                        bitMap.Dispose();
                        grph.Dispose();
                        try
                        {
                            System.IO.File.Delete(Server.MapPath("~/WebData/Cropped/" + model.ImageName));
                        }
                        catch
                        {
                        }
                        return View(model);
                    }
                    catch
                    {
                        return View(model);
                    }
                }
                else
                {
                    try
                    {
                        TempData["CroppedImage"] = null;
                        if (!string.IsNullOrEmpty(model.FileUploaderCss))
                        {
                            TempData[model.FileUploaderCss] = null;
                        }
                        System.IO.File.Delete(Server.MapPath("~/WebData/Cropped/" + model.ImageName));
                    }
                    catch (Exception ex)
                    {
                    }
                    model.IsCancelled = true;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                return new ContentResult() { Content = "Please try again later. <br />" + ex.Message };
            }
        }
        /// <summary>
        /// Here we get the image which is to be Cropped.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UploadCroppedImage()
        {
            var retunedFilename = "";
            try
            {
                Models.Common.CreateFolder();
                foreach (string file in Request.Files)
                {
                    HttpPostedFileBase fileContent = Request.Files[file];
                    if (fileContent != null && fileContent.ContentLength > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + "-" + Path.GetFileName(fileContent.FileName).Substring(fileContent.FileName.LastIndexOf("."), fileContent.FileName.Length - fileContent.FileName.LastIndexOf("."));
                        var path = Request.PhysicalApplicationPath + "WebData\\Cropped\\" + fileName;
                        fileContent.SaveAs(path);
                        retunedFilename = fileName;
                    }
                }
            }
            catch (Exception)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Upload failed");
            }
            return Json(retunedFilename);
        }
    }
}
