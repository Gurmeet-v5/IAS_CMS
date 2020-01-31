using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using KISD.Areas.BlogAdmin.Contexts;
using KISD.Areas.BlogAdmin.Models;

namespace KISD.Areas.BlogAdmin.Controllers
{
    public class ContentController : Controller
    {
        //
        // GET: /BlogAdmin/Content/

        /// <summary>
        /// Get data for content page
        /// </summary>
        /// <param name="ContentType"></param>
        /// <returns></returns>
        /// 
        [Authorize]
        [SessionExpire]
        public ActionResult CreateContent(int? ContentType)
        {
            var _ContentContext = new ContentContexts();
            var _ContentModel = new ContentModel();
            var ContentTypeName = _ContentContext.GetContentType(ContentType.Value);
            _ContentModel.ContentTypeID = ContentType.Value;
            var objContent = _ContentContext.GetContent(ContentType).FirstOrDefault();
            if (objContent != null)
            {
                _ContentModel = objContent;
                _ContentModel.ContentTypeTitle = "Edit " + ContentTypeName + (ContentType != 2 ? " Content" : " Content");
                ViewBag.Submit = "Update";
            }
            else if (ContentType == Convert.ToInt32(ContentContexts.ContentType.BasicSetting))
            {
                var _settingModel = new SettingModel();
                var _settingContext = new SettingContexts();
                _settingModel = _settingContext.GetSettings().FirstOrDefault();
                var rvd = new RouteValueDictionary();
                rvd.Add("SettingID", _settingModel == null ? 0 : _settingModel.SettingID);
                rvd.Add("ContentType", ContentType);
                return RedirectToAction("Setting", "Setting", rvd);
            }
            else if (ContentType == Convert.ToInt32(ContentContexts.ContentType.EmailSetting))
            {
                var _settingModel = new SettingModel();
                var _settingContext = new SettingContexts();
                _settingModel = _settingContext.GetSettings().FirstOrDefault();
                return RedirectToAction("Setting", "Setting", new { SettingID = _settingModel == null ? 0 : _settingModel.SettingID, ContentType = Convert.ToInt32(ContentContexts.ContentType.EmailSetting) });
            }
            else if (ContentType == Convert.ToInt32(ContentContexts.ContentType.ThemeSetting))
            {
                var _settingModel = new SettingModel();
                var _settingContext = new SettingContexts();
                _settingModel = _settingContext.GetSettings().FirstOrDefault();
                return RedirectToAction("Setting", "Setting", new { SettingID = _settingModel == null ? 0 : _settingModel.SettingID, ContentType = Convert.ToInt32(ContentContexts.ContentType.ThemeSetting) });
            }
            else
            {
                ViewBag.Submit = "Save";
                _ContentModel.ContentTypeTitle = "Add " + ContentTypeName + (ContentType != 2 ? " Content" : " Content");
            }
            return View(_ContentModel);
        }

        /// <summary>
        /// Method to save/ update the added rentals into the database.
        /// </summary>
        /// <param name="_Contentmodel">ContentModel that will contain the values saved in the Content View</param>
        /// <param name="command">This will contain value on click of Cancel button</param>
        /// <param name="fm">It contains the form collection (hidden field etc) values.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [SessionExpire]
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateContent(ContentModel _Contentmodel, string command, FormCollection fm, int ContentType)
        {
            var file = Request.Files.Count > 0 ? Request.Files[0] : null;
            var ContentContext = new ContentContexts();
            var ContentTypeName = ContentContext.GetContentType(ContentType);
            ViewBag.Title = (_Contentmodel.ContentID > 0 ? "Edit " : "Add ") + ContentTypeName;
            ViewBag.Submit = _Contentmodel.ContentID > 0 ? "Update" : "Save";
            if (string.IsNullOrEmpty(command))
            {
                try
                {
                    if (ContentType == Convert.ToInt32(ContentContexts.ContentType.Header) || ContentType == Convert.ToInt32(ContentContexts.ContentType.Footer))
                    {
                        //Save image path
                        if (file != null && file.ContentLength > 0)
                        {
                            #region Upload Image
                            Models.Common.CreateFolder();
                            TempData["CroppedImage"] = null;
                            var fileName = Guid.NewGuid().ToString() + "-" + Path.GetFileName(file.FileName).Substring(file.FileName.LastIndexOf("."), file.FileName.Length - file.FileName.LastIndexOf("."));
                            file.SaveAs(Server.MapPath("~/WebData/images/" + fileName));
                            _Contentmodel.ImagePathTxt = "~/WebData/images/" + fileName;
                            var width = 250;
                            var fileExtension = fileName.Substring(fileName.LastIndexOf("."), fileName.Length - fileName.LastIndexOf("."));
                            var strPath = Request.PhysicalApplicationPath + "WebData\\images\\" + fileName;
                            var myImage = Models.Common.CreateImageThumbnail(strPath, width);
                            myImage.Save(Request.PhysicalApplicationPath + "WebData\\thumbnails\\" + fileName,
                                           fileExtension.ToLower() == ".png" ?
                                           System.Drawing.Imaging.ImageFormat.Png :
                                           fileExtension.ToLower() == ".gif" ?
                                           System.Drawing.Imaging.ImageFormat.Gif :
                                           System.Drawing.Imaging.ImageFormat.Jpeg
                                           );
                            myImage.Dispose();
                            var mysmallImage = Models.Common.CreateImageThumbnail(strPath, 200);
                            mysmallImage.Save(Request.PhysicalApplicationPath + "WebData\\thumbnails_Small\\" + fileName,
                                           fileExtension.ToLower() == ".png" ?
                                           System.Drawing.Imaging.ImageFormat.Png :
                                           fileExtension.ToLower() == ".gif" ?
                                           System.Drawing.Imaging.ImageFormat.Gif :
                                           System.Drawing.Imaging.ImageFormat.Jpeg
                                           );
                            mysmallImage.Dispose();
                            #endregion
                        }
                        else
                        {
                            _Contentmodel.ImagePathTxt = ContentContext.GetContent(ContentType).Select(x => x.ImagePathTxt).FirstOrDefault();
                        }
                    }
                    if (ViewBag.Submit == "Save")
                    {
                        _Contentmodel.ContentTypeID = ContentType;
                        ContentContext.AddContent(_Contentmodel);
                        TempData["AlertMessage"] = ContentTypeName + " details saved successfully.";
                    }
                    else
                    {
                        ContentContext.EditContents(_Contentmodel);
                        TempData["AlertMessage"] = ContentTypeName + " Content details updated successfully.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["AlertMessage"] = "Some error occured.Please try again later. " + ex.Message;
                }
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
