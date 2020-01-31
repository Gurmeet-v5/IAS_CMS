using KISD.Areas.Admin.Models;
using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using static KISD.Areas.Admin.Models.Common;
using AnnouncementTypeAlias = KISD.Areas.Admin.Models.GalleryListingService.TypeMaster;

namespace KISD.Areas.Admin.Controllers
{
    public class AnnouncementController : Controller
    {
        private AnnouncementService _service;
        db_KISDEntities objContext = new db_KISDEntities();
        /// <summary>
        /// Code to create instance Announcement Service class in constructor
        /// </summary>
        public AnnouncementController()
        {
            _service = new AnnouncementService();
        }
        [SessionExpire]
        [Authorize]
        /// <summary>
        /// this method will show the Announcement listing with all Announcement type.
        /// </summary>
        /// <param name="Page">this parameter is used to get page number to be shown.</param>
        /// <param name="PageSize">this parameter is used to get no of recorde to be shown.</param>
        /// <param name="gridSortOptions">this parameter is used to get grid sorting option.</param>
        /// <param name="it">this parameter is used to get type id of the Announcement i.e. 1,2 or 3</param>
        /// <param name="formCollection">this parameter is used to get controls collection on the page.</param>
        /// <param name="ObjResult"></param>
        /// <returns>view to enter Announcement details.</returns>
        public ActionResult Index(int? Page, int? PageSize, GridSortOptions gridSortOptions, string it, FormCollection formCollection, string ObjResult)
        {
            var db_obj = new db_KISDEntities();
            //Check for valid TypeMasterID
            if (it == null)
            {
                return RedirectToAction("Index", "Home");
            }

            #region Check Tab is Accessible or Not
            db_KISDEntities objContext = new db_KISDEntities();
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Masters), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            //decrypt Announcement type id(it)
            if (!string.IsNullOrEmpty(Convert.ToString(it)))
            {
                it = Convert.ToString(EncryptDecrypt.Decrypt(it));
            }
            TempData["CroppedImage"] = null;
            var AnnouncementType = it != null ? Convert.ToInt32(it) : Convert.ToInt32(AnnouncementTypeAlias.Announcement);
            ViewBag.TypeMasterID = AnnouncementType;
            //*******************Fill Values if Display order contains null values***************************
            var displayOrderList = objContext.Announcements.Where(x => x.TypeMasterID == AnnouncementType && x.IsDeletedInd == false).ToList();
            foreach (var item in displayOrderList)
            {
                if (string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()))
                {
                    var objContentData = objContext.Announcements.Where(x => x.AnnouncementID == item.AnnouncementID).FirstOrDefault();
                    var NewdisplayOrder = (displayOrderList.Max(x => x.DisplayOrderNbr)) == null ? 1 : displayOrderList.Max(x => x.DisplayOrderNbr).Value + 1;
                    objContentData.DisplayOrderNbr = NewdisplayOrder;
                    objContext.SaveChanges();
                }
            }
            //***********************************************************

            #region Ajax Call
            if (ObjResult != null)
            {
                AjaxRequest objAjaxRequest = JsonConvert.DeserializeObject<AjaxRequest>(ObjResult);//Convert json String to object Model
                if (objAjaxRequest.ajaxcall != null && !string.IsNullOrEmpty(objAjaxRequest.ajaxcall) && ObjResult != null && !string.IsNullOrEmpty(ObjResult))
                {
                    if (objAjaxRequest.ajaxcall == "paging")//Ajax Call type = paging i.e. Next|Previous|Back|Last
                    {
                        Session["pageNo"] = Page;// stores the page no for status
                    }
                    else if (objAjaxRequest.ajaxcall == "sorting")//Ajax Call type = sorting i.e. column sorting Asc or Desc
                    {
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
                        Session["GridSortOption"] = gridSortOptions;
                        PageSize = (Session["PageSize"] != null ? Convert.ToInt32(Session["PageSize"].ToString()) : PageSize);
                    }
                    else if (objAjaxRequest.ajaxcall == "ddlPaging")//Ajax Call type = drop down paging i.e. drop down value 10, 25, 50, 100, ALL
                    {
                        Session["PageSize"] = (Request.QueryString["pagesize"] != null ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : PageSize);
                        Session["GridSortOption"] = gridSortOptions;
                        Session["pageNo"] = Page;
                    }
                    else if (objAjaxRequest.ajaxcall == "status")//Ajax Call type = status i.e. Active/Inactive
                    {
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                    else if (objAjaxRequest.ajaxcall == "displayorder")//Ajax Call type = Display Order i.e. drop down values
                    {
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }

                    objAjaxRequest.ajaxcall = null; ;//remove parameter value
                }

                //Ajax Call for update status for Announcements
                if (objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && ObjResult != null && !string.IsNullOrEmpty(ObjResult))
                {
                    var Announcementid = System.Convert.ToInt64(objAjaxRequest.hfid);
                    var Announcements = objContext.Announcements.Find(Announcementid);
                    if (Announcements != null)
                    {
                        #region System Change Log
                        var oldresult = (from a in objContext.Announcements
                                         where a.AnnouncementID == Announcementid
                                         select a).ToList();
                        DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                        #endregion
                        var isvalid = true;
                        if (AnnouncementType == Convert.ToInt32(AnnouncementTypeAlias.OnscreenAlert))
                        {
                            var onscreenalerttypeid = Convert.ToInt32(AnnouncementTypeAlias.OnscreenAlert);
                            var alertcnt = objContext.Announcements.Where(x => x.TypeMasterID == onscreenalerttypeid && x.StatusInd == true && x.IsDeletedInd == false).Count();
                            if (alertcnt == 3 && objAjaxRequest.hfvalue == "1")
                            {
                                TempData["Message"] = "Maximum three onscreen alerts can be set as active to show on website.";
                                isvalid = false;
                            }
                        }
                        if (isvalid)
                        {
                            Announcements.StatusInd = objAjaxRequest.hfvalue == "1";

                            if (objAjaxRequest.qs_Type == "displayorder")
                            {
                                if (AnnouncementService.ChangeAnnouncementDisplayOrder(Announcements.DisplayOrderNbr.Value, Convert.ToInt64(objAjaxRequest.qs_value), Announcements.AnnouncementID, Convert.ToInt32(Announcements.TypeMasterID)))
                                {
                                    TempData["AlertMessage"] = "Display Order has been changed successfully.";
                                }
                            }
                            else
                            {
                                objContext.SaveChanges();
                                #region System Change Log
                                SystemChangeLog objSCL = new SystemChangeLog();
                                long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                                User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                                objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                                objSCL.UsernameTxt = objuser.UserNameTxt;
                                objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                                objSCL.ModuleTxt = _service.GetAnnouncementType(Announcements.TypeMasterID.Value);
                                objSCL.LogTypeTxt = Announcements.AnnouncementID > 0 ? "Update" : "Add";
                                objSCL.NotesTxt = _service.GetAnnouncementType(Announcements.TypeMasterID.Value) + " Details updated status for " + Announcements.TitleTxt;
                                objSCL.LogDateTime = DateTime.Now;
                                objContext.SystemChangeLogs.Add(objSCL);
                                objContext.SaveChanges();

                                objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                                var newResult = (from x in objContext.Announcements
                                                 where x.AnnouncementID == Announcements.AnnouncementID
                                                 select x);
                                DataTable dtNew = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(newResult);
                                foreach (DataColumn col in dtNew.Columns)
                                {
                                    // if(objSCL)
                                    if (dtOld.Rows[0][col.ColumnName].ToString() != dtNew.Rows[0][col.ColumnName].ToString())
                                    {
                                        SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                                        objSCLD.ChangeLogID = objSCL.ChangeLogID;
                                        objSCLD.FieldNameTxt = col.ColumnName.ToString();
                                        objSCLD.OldValueTxt = dtOld.Rows[0][col.ColumnName].ToString();
                                        objSCLD.NewValueTxt = dtNew.Rows[0][col.ColumnName].ToString();
                                        objContext.SystemChangeLogDetails.Add(objSCLD);
                                        objContext.SaveChanges();
                                    }
                                }
                                #endregion
                                TempData["AlertMessage"] = "Status updated successfully.";
                            }
                        }


                        objAjaxRequest.hfid = null;//remove parameter value
                        objAjaxRequest.hfvalue = null;//remove parameter value
                        objAjaxRequest.qs_Type = null;
                        PageSize = ((Request.QueryString["pagesize"] != null && Request.QueryString["pagesize"].ToString() != "All") ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : Models.Common._pageSize);
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Models.Common._currentPage);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                }

                else
                {
                    TempData["Message"] = string.Empty;
                }
                ObjResult = string.Empty;
            }
            #endregion Ajax Call

            ViewBag.Title = ViewBag.PageTitle = _service.GetAnnouncementType(AnnouncementType);//+ "s"

            //This section is used to retain the values of page , pagesize and gridsortoption on complete page post back(Edit, Dlete)
            if (!Request.IsAjaxRequest() && Session["Edit/Delete"] != null && !string.IsNullOrEmpty(Session["Edit/Delete"].ToString()))
            {
                PageSize = (Session["PageSize"] != null ? Convert.ToInt32(Session["PageSize"]) : Models.Common._pageSize);
                Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"]) : Models.Common._currentPage);
                gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                Session["Edit/Delete"] = null;
            }
            else if (!Request.IsAjaxRequest() && Session["Edit/Delete"] == null)
            {
                gridSortOptions.Column = "AnnouncementCreateDate";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            if (gridSortOptions.Column == "TitleTxt" || gridSortOptions.Column == "AnnouncementCreateDate"
                || gridSortOptions.Column == "DisplayOrderNbr")
            {

            }
            else
            {
                gridSortOptions.Column = "AnnouncementCreateDate";
            }
            //.. Code for get records as page view model
            var pagesize = PageSize.HasValue ? PageSize.Value : Models.Common._pageSize;
            var page = Page.HasValue ? Page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;
            long announcementType = Convert.ToInt64(it);
            var pagedViewModel = new PagedViewModel<AnnouncementModel>
            {
                ViewData = ViewData,
                Query = _service.GetAnnouncements(announcementType).AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "AnnouncementCreateDate",
                Page = page,
                PageSize = pagesize,
            }.Setup();
            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("AnnouncementPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }
        [SessionExpire]
        [Authorize]
        /// <summary>
        /// 
        /// </summary>
        /// <param name="it">Announcement type id</param>
        /// <param name="iid"> Announcement id</param>
        /// <returns></returns>
        public ActionResult Create(string it, string iid)
        {
            var objAnnouncementModel = new AnnouncementModel();
            //Check for valid TypeMasterID

            ViewBag.TypeMasterID = it;
            ViewBag.Title = "";

            #region Check Tab is Accessible or Not
            db_KISDEntities objContext = new db_KISDEntities();
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Masters), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            if ((Request.QueryString["it"] == null) && (Request.QueryString["iid"] == null)
                )
            {
                return RedirectToAction("Index", "Home");
            }
            //decrypt Announcement type id(it)
            it = !string.IsNullOrEmpty(Convert.ToString(it)) ? EncryptDecrypt.Decrypt(it) : "0";

            //decrypt Announcement id(iid)
            iid = !string.IsNullOrEmpty(Convert.ToString(iid)) ? EncryptDecrypt.Decrypt(iid) : "0";
            int AnnouncementID = Convert.ToInt32(iid);
            if (AnnouncementID > 0 && objContext.Announcements.Where(x => x.AnnouncementID == AnnouncementID && x.IsDeletedInd == true).Any())
            {
                return RedirectToAction("Index", "Home");
            }
            Session["Edit/Delete"] = "Edit";
            ViewBag.Title = ViewBag.PageTitle = (iid == "0" ? "Add " : "Edit ") + (_service.GetAnnouncementType(Convert.ToInt32(it)));
            ViewBag.Submit = (iid == "0" ? "Save" : "Update");
            ViewBag.TypeMasterID = it;
            ViewBag.AnnouncementTypeTitle = _service.GetAnnouncementType(Convert.ToInt32(it)) ;// +"s"
            objAnnouncementModel.AnnouncementCreateDate = DateTime.Now;
            ViewBag.AnnouncementCreateDate = DateTime.Now.ToShortDateString();
            ViewBag.StartDateStr = "";
            ViewBag.EndDateStr = "";
            ViewBag.ScheduleDateStr = DateTime.Now.ToShortDateString();
            objAnnouncementModel.TypeMasterID = Convert.ToInt32(it);
            if (Convert.ToInt32(iid) > 0)
            {
                var Announcement = (from u in objContext.Announcements
                                    where u.AnnouncementID == AnnouncementID
                                    select u).FirstOrDefault();
                if (Announcement != null)
                {
                    objAnnouncementModel.AnnouncementID = Announcement.AnnouncementID;
                    objAnnouncementModel.TitleTxt = Announcement.TitleTxt;
                    objAnnouncementModel.DisplayStartDate = Announcement.DisplayStartDate;
                    objAnnouncementModel.DisplayEndDate = Announcement.DisplayEndDate;
                    objAnnouncementModel.AltImageTxt = Announcement.AltImageTxt;
                    objAnnouncementModel.DescriptionTxt = Announcement.DescriptionTxt;
                    objAnnouncementModel.ScheduleDateTime = Announcement.ScheduleDateTime;
                    objAnnouncementModel.ImageURLTxt = Announcement.ImageURLTxt;
                    objAnnouncementModel.AnnouncementCreateDate = Convert.ToDateTime(Announcement.AnnouncementCreateDate.Value.ToShortDateString());
                    objAnnouncementModel.TypeMasterID = Announcement.TypeMasterID;
                    objAnnouncementModel.StatusInd = Announcement.StatusInd.Value;
                    objAnnouncementModel.CreateByID = Announcement.CreateByID;
                    objAnnouncementModel.CreateDate = Announcement.CreateDate;
                    objAnnouncementModel.LastModifyByID = Announcement.LastModifyByID;
                    objAnnouncementModel.LastModifyDate = Announcement.LastModifyDate;
                    objAnnouncementModel.ScheduleTimeTxt = Announcement.ScheduleTimeTxt;
                    ViewBag.StatusInd = GetStatusData(objAnnouncementModel.StatusInd ? "1" : "0");
                    ViewBag.StartDateStr = objAnnouncementModel.DisplayStartDate.HasValue ? objAnnouncementModel.DisplayStartDate.Value.ToString("MM/dd/yyyy hh:mm tt") : null;
                    ViewBag.EndDateStr = objAnnouncementModel.DisplayEndDate.HasValue ? objAnnouncementModel.DisplayEndDate.Value.ToString("MM/dd/yyyy hh:mm tt") : null;
                    ViewBag.ScheduleDateStr = objAnnouncementModel.ScheduleDateTime.HasValue ? objAnnouncementModel.ScheduleDateTime.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                    ViewBag.AnnouncementCreateDate = objAnnouncementModel.AnnouncementCreateDate.HasValue ? objAnnouncementModel.AnnouncementCreateDate.Value.ToShortDateString(): DateTime.Now.ToShortDateString();
                }
            }
            else
            {
                if (Convert.ToInt32(it) == Convert.ToInt32(AnnouncementTypeAlias.OnscreenAlert))
                {
                    var onscreenalerttypeid = Convert.ToInt32(AnnouncementTypeAlias.OnscreenAlert);
                    var alertcnt = objContext.Announcements.Where(x => x.TypeMasterID == onscreenalerttypeid && x.StatusInd == true && x.IsDeletedInd == false).Count();
                    if (alertcnt == 3)
                    {
                        ViewBag.StatusInd = GetStatusData("0");
                    }
                    else
                    {
                        ViewBag.StatusInd = GetStatusData(string.Empty);
                    }
                }
                else { ViewBag.StatusInd = GetStatusData(string.Empty); }
                
            }

            return View(objAnnouncementModel);
        }

        /// <summary>
        /// this method wil post the details of Announcement filled by the admin.
        /// </summary>
        /// <param name="command">command name whether Save or Cancel.</param>
        /// <param name="fm">controls collection on the page.</param>
        /// <param name="model">object of Announcement model</param>
        /// <returns>view with status message.</returns>
        [HttpPost]
        [Authorize]
        [SessionExpire]
        [ValidateInput(false)]
        public ActionResult Create(string command, FormCollection fm, AnnouncementModel model)
        {
            try
            {
                HttpPostedFileBase file = Request.Files.Count > 0 ? Request.Files[0] : null;
                ViewBag.Title = (model.AnnouncementID == 0 ? "Add " : "Edit ") + _service.GetAnnouncementType(model.TypeMasterID.Value);
                ViewBag.StatusInd = GetStatusData(string.Empty);
                ViewBag.Submit = (model.AnnouncementID == 0 ? "Save" : "Update");
                ViewBag.TypeMasterID = model.TypeMasterID;
                ViewBag.AnnouncementCreateDate = model.AnnouncementCreateDate.Value.ToShortDateString();
                var rvd = new RouteValueDictionary();
                rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
                rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
                rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "AnnouncementCreateDate");
                rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
                rvd.Add("it", EncryptDecrypt.Encrypt(Convert.ToString(model.TypeMasterID)));
                ViewBag.PageTitle = (model.AnnouncementID == 0 ? "Add " : "Edit ") + _service.GetAnnouncementType(model.TypeMasterID.Value) + " Details";
                ViewBag.AnnouncementTypeTitle = _service.GetAnnouncementType(model.TypeMasterID.Value);// +"s"
                ViewBag.StartDateStr = model.DisplayStartDate != null ? Convert.ToString(model.DisplayStartDate) : "";
                ViewBag.EndDateStr = model.DisplayEndDate != null ? Convert.ToString(model.DisplayEndDate) : "";
                ViewBag.ScheduleDateStr = model.ScheduleDateTime != null? model.ScheduleDateTime.Value.ToShortDateString():"";
                #region System Change Log
                DataTable dtOld;
                var oldresult = (from a in objContext.Announcements
                                 where a.AnnouncementID == model.AnnouncementID
                                 select a).ToList();
                dtOld = Models.Common.LINQResultToDataTable(oldresult);
                #endregion
                if (string.IsNullOrEmpty(command))
                {
                    Announcement objAnnouncement;
                    if (model.AnnouncementID > 0)
                    {
                        objAnnouncement = objContext.Announcements.Find(model.AnnouncementID);
                    }
                    else
                    {
                        objAnnouncement = new Announcement();
                        objAnnouncement.IsDeletedInd = false;
                    }
                    ViewBag.StatusInd = GetStatusData(model.StatusInd ? "1" : "0");
                    objAnnouncement.TypeMasterID = model.TypeMasterID;
                    objAnnouncement.StatusInd = fm["StatusInd"].ToString() == "1";
                    if (model.TypeMasterID == Convert.ToInt32(AnnouncementTypeAlias.OnscreenAlert))
                    {
                        var onscreenalerttypeid = Convert.ToInt32(AnnouncementTypeAlias.OnscreenAlert);
                        var alertcnt = objContext.Announcements.Where(x => x.TypeMasterID == onscreenalerttypeid && x.StatusInd == true && x.IsDeletedInd == false && x.AnnouncementID != model.AnnouncementID).Count();
                        if (alertcnt == 3 && fm["StatusInd"].ToString() == "1")
                        {
                            TempData["Message"] = "Maximum three onscreen alerts can be set as active to show on website.";
                            return View(model);
                        }
                    }
                    if (!string.IsNullOrEmpty(model.TitleTxt))
                    {
                        var chkAnnouncement = objContext.Announcements.Where(x => x.TitleTxt == model.TitleTxt.Trim()
                                                                    && x.TypeMasterID == model.TypeMasterID
                                                                    && x.AnnouncementID != model.AnnouncementID && (x.IsDeletedInd == false || x.IsDeletedInd == null)).Any();

                        if (chkAnnouncement)//check Announcement title on adding new Announcement details or updating existing 1
                        {
                            TempData["CroppedImage"] = null;
                            ModelState.AddModelError("TitleTxt", model.TitleTxt + " title already exists.");
                            return View(model);
                        }

                        //if (!objAnnouncement.StatusInd.Value)
                        //{
                        //    TempData["CroppedImage"] = null;
                        //    ViewBag.AnnouncementCreateDate = model.AnnouncementCreateDate.Value.ToShortDateString().Trim();
                        //    TempData["Message"] = "Announcement is in use, cannot be set as Inactive.";
                        //    return View(model);
                        //}                     
                    }
                    objAnnouncement.TitleTxt = model.TitleTxt;
                    objAnnouncement.AltImageTxt = !string.IsNullOrEmpty(model.AltImageTxt) ? model.AltImageTxt.Replace(">", "").Replace("<", "") : string.Empty;
                    objAnnouncement.AnnouncementCreateDate = model.AnnouncementCreateDate;
                    objAnnouncement.DisplayStartDate = model.DisplayStartDate != null ? model.DisplayStartDate : null;
                    objAnnouncement.DisplayEndDate = model.DisplayEndDate != null ? model.DisplayEndDate : null;
                    objAnnouncement.DescriptionTxt = model.DescriptionTxt;
                    objAnnouncement.CreateDate = model.AnnouncementID > 0 ? objAnnouncement.CreateDate : DateTime.Now; ;
                    objAnnouncement.CreateByID = model.AnnouncementID > 0 ? objAnnouncement.CreateByID : Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    objAnnouncement.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    objAnnouncement.LastModifyDate = DateTime.Now;
                    objAnnouncement.ScheduleDateTime = model.ScheduleDateTime;
                    objAnnouncement.ScheduleTimeTxt = model.ScheduleTimeTxt;
                    
                    if (TempData["CroppedImage"] != null)
                    {
                        #region Cropped Image
                        var croppedfile = new System.IO.FileInfo(Server.MapPath(TempData["CroppedImage"].ToString()));
                        var fileName = croppedfile.Name;
                        croppedfile = null;
                        var sourcePath = Server.MapPath(TempData["CroppedImage"].ToString());
                        var targetPath = Request.PhysicalApplicationPath + "WebData\\";
                        System.IO.File.Copy(System.IO.Path.Combine(sourcePath.Replace(fileName, ""), fileName), System.IO.Path.Combine(targetPath + "images\\", fileName), true);
                        try
                        {
                            Models.Common.DeleteImage(Server.MapPath(TempData["CroppedImage"].ToString()));
                        }
                        catch
                        {
                        }
                        objAnnouncement.ImageURLTxt = "~/WebData/images/" + fileName;
                        var width = 100;
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
                        TempData["CroppedImage"] = null;
                        #endregion
                    }
                    else
                    {
                        objAnnouncement.ImageURLTxt = objAnnouncement.ImageURLTxt;
                    }

                    if (model.AnnouncementID > 0)
                    {
                        TempData["AlertMessage"] = _service.GetAnnouncementType(model.TypeMasterID.Value) + " details updated successfully.";
                        objContext.SaveChanges();
                    }
                    else
                    {
                        objContext.Announcements.Add(objAnnouncement);
                        objContext.SaveChanges();
                        TempData["AlertMessage"] = _service.GetAnnouncementType(model.TypeMasterID.Value) + " details saved successfully.";
                    }

                    #region System Change Log
                    SystemChangeLog objSCL = new SystemChangeLog();
                    long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                    objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                    objSCL.UsernameTxt = objuser.UserNameTxt;
                    objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                    objSCL.ModuleTxt = _service.GetAnnouncementType(objAnnouncement.TypeMasterID.Value);
                    objSCL.LogTypeTxt = model.AnnouncementID > 0 ? "Update" : "Add";
                    objSCL.NotesTxt = (_service.GetAnnouncementType(model.TypeMasterID.Value)) + " Details" + (model.AnnouncementID > 0 ? " updated for " : "  added for ") + model.TitleTxt;
                    objSCL.LogDateTime = DateTime.Now;
                    objContext.SystemChangeLogs.Add(objSCL);
                    objContext.SaveChanges();
                    objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                    var newResult = (from x in objContext.Announcements
                                     where x.AnnouncementID == objAnnouncement.AnnouncementID
                                     select x);
                    DataTable dtNew = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(newResult);
                    foreach (DataColumn col in dtNew.Columns)
                    {
                        if (model.AnnouncementID > 0)
                        {
                            // if(objSCL)
                            if (dtOld.Rows[0][col.ColumnName].ToString() != dtNew.Rows[0][col.ColumnName].ToString())
                            {
                                SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                                objSCLD.ChangeLogID = objSCL.ChangeLogID;
                                objSCLD.FieldNameTxt = col.ColumnName.ToString();
                                objSCLD.OldValueTxt = dtOld.Rows[0][col.ColumnName].ToString();
                                objSCLD.NewValueTxt = dtNew.Rows[0][col.ColumnName].ToString();
                                objContext.SystemChangeLogDetails.Add(objSCLD);
                                objContext.SaveChanges();
                            }

                        }
                        else
                        {
                            SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                            objSCLD.ChangeLogID = objSCL.ChangeLogID;
                            objSCLD.FieldNameTxt = col.ColumnName.ToString();
                            objSCLD.OldValueTxt = "";
                            objSCLD.NewValueTxt = dtNew.Rows[0][col.ColumnName].ToString();
                            objContext.SystemChangeLogDetails.Add(objSCLD);
                            objContext.SaveChanges();

                        }
                    }
                    #endregion
                    return RedirectToAction("Index", "Announcement", rvd);
                }
                else
                {
                    return RedirectToAction("Index", "Announcement", rvd);
                }
            }
            catch
            {
                return View(model);
            }
        }
        /// <summary>
        /// This method is used to delete the Announcement from database.It also chacks wheather Announcement is in use or not.
        /// If Announcement is in use then it return to view and show message "Announcement is in use, cannot be deleted." else it delte the Announcement and return to thye view.
        /// </summary>
        /// <param name="TypeMasterID">This parameter is used to get the TypeMasterID</param>
        /// <param name="AnnouncementID">This parameter is used to get the Announcementid</param>
        /// <param name="fm">this parameter is used to get the form control values from view </param>
        /// <returns>This method return the Json result (url) that will be passed to the Ajax post method on client side.</returns>
        [HttpPost]
        [Authorize]
        public JsonResult Delete(string it, string iid, FormCollection fm)
        {
            //decrypt Announcement type id(it)
            it = !string.IsNullOrEmpty(Convert.ToString(it)) ? EncryptDecrypt.Decrypt(it) : "0";
            //decrypt Announcement id(iid)
            iid = !string.IsNullOrEmpty(Convert.ToString(iid)) ? EncryptDecrypt.Decrypt(iid) : "0";

            //.. Code for get the route value directory
            RouteValueDictionary rvd = new RouteValueDictionary();
            ViewBag.TypeMasterID = it;
            ViewBag.Title = _service.GetAnnouncementType(Convert.ToInt32(it));
            var page = Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString();
            var pagesize = Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString();
            rvd.Add("pagesize", pagesize);
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "AnnouncementCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
            rvd.Add("it", EncryptDecrypt.Encrypt(it));
            TempData["pager"] = pagesize;
            Session["Edit/Delete"] = "Delete";
            try
            {
                // TODO: Add delete logic here
                //.. Check for Announcement in use
                Announcement objAnnouncement = objContext.Announcements.Find(Convert.ToInt32(iid));
                int AnnounceID = Convert.ToInt32(iid);
                #region System Change Log
                var oldresult = (from a in objContext.Announcements
                                 where a.AnnouncementID == AnnounceID
                                 select a).ToList();
                DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                #endregion
                if (objAnnouncement != null)
                {
                    // objAnnouncement.IsDeletedInd = true;

                    //****************Display Order ************************
                    //var objAnnouncements = objContext.Announcements.Where(x => x.TypeMasterID == objAnnouncement.TypeMasterID).FirstOrDefault();
                    if (objAnnouncement != null)
                    {
                        try
                        {
                            var objAnnouncementService = new AnnouncementService();
                            objAnnouncementService.ChangeDeletedDisplayOrder(objAnnouncement.DisplayOrderNbr.Value, objAnnouncement.AnnouncementID, objAnnouncement.TypeMasterID.Value);
                        }
                        catch { }
                    }

                    #region Delete Selected Right Section for the Announcement
                    var MasterID = Convert.ToInt64(it);
                    var rightSections = objContext.RightSections.Where(x => x.ListingID == AnnounceID && x.TypeMasterID == MasterID).ToList();
                    if (rightSections != null && rightSections.Count() > 0)
                    {
                        foreach (var section in rightSections)
                        {
                            section.IsDeletedInd = true;
                        }
                        objContext.SaveChanges();
                    }
                    #endregion

                    //***************************************************
                    // objContext.SaveChanges();
                    #region System Change Log
                    SystemChangeLog objSCL = new SystemChangeLog();
                    long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                    objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                    objSCL.UsernameTxt = objuser.UserNameTxt;
                    objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                    objSCL.ModuleTxt = _service.GetAnnouncementType(objAnnouncement.TypeMasterID.Value);
                    objSCL.LogTypeTxt = "Delete";
                    objSCL.NotesTxt = (_service.GetAnnouncementType(objAnnouncement.TypeMasterID.Value)) + " Details deleted for " + objAnnouncement.TitleTxt;
                    objSCL.LogDateTime = DateTime.Now;
                    objContext.SystemChangeLogs.Add(objSCL);
                    objContext.SaveChanges();
                    objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                    var objContextnew = new db_KISDEntities();
                    var newResult = (from x in objContextnew.Announcements
                                     where x.AnnouncementID == AnnounceID
                                     select x);
                    DataTable dtNew = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(newResult);
                    foreach (DataColumn col in dtNew.Columns)
                    {
                        SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                        objSCLD.ChangeLogID = objSCL.ChangeLogID;
                        objSCLD.FieldNameTxt = col.ColumnName.ToString();
                        objSCLD.OldValueTxt = dtOld.Rows[0][col.ColumnName].ToString();
                        objSCLD.NewValueTxt = col.ColumnName == "IsDeletedInd" ? dtNew.Rows[0][col.ColumnName].ToString() : "";
                        objContext.SystemChangeLogDetails.Add(objSCLD);
                        objContext.SaveChanges();
                    }
                    #endregion
                    try
                    {
                        Models.Common.DeleteImage(Server.MapPath(objAnnouncement.ImageURLTxt));
                    }
                    catch
                    {
                    }
                    TempData["AlertMessage"] = _service.GetAnnouncementType(Convert.ToInt32(it)) + " details deleted successfully.";
                }
                //.. Checks for no of records in current page if exists records then return same page number else decrease the page number
                int? CheckPage = 1;
                int TypeMasterID = Convert.ToInt32(it);
                var count = objContext.Announcements.Where(x => x.TypeMasterID == TypeMasterID && x.IsDeletedInd == false).Count();
                if (Convert.ToInt32(page) > 1)
                    CheckPage = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
                rvd.Add("page", CheckPage);
                return Json(Url.Action("Index", "Announcement", rvd));
            }
            catch
            {
                rvd.Add("page", page);
                return Json(Url.Action("Index", "Announcement", rvd));
            }
        }
        /// <summary>
        /// Get the value of Status with selected value(active or inactive)
        /// </summary>
        /// <param name="value">who's status to be found.</param>
        /// <returns>list with status values.</returns>
        private List<SelectListItem> GetStatusData(string value)
        {
            var items = new List<SelectListItem>();
            var data = new SelectListItem();
            data.Text = "Active";
            data.Value = "1";
            items.Add(data);
            data = new SelectListItem();
            data.Text = "InActive";
            data.Value = "0";
            items.Add(data);
            if (!string.IsNullOrEmpty(value))
            {
                items.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }
            return items;
        }
    }
}