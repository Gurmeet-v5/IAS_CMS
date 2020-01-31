using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KISD.Areas.Admin.Models;
using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using static KISD.Areas.Admin.Models.Common;

namespace KISD.Areas.Admin.Controllers
{
    public class ReportsController : Controller
    {
        [Authorize]
        public ActionResult Index(string rt, int? page, int? pagesize, GridSortOptions gridSortOptions, string objresult, string command, PagedViewModel<ReportsModel> model, FormCollection fm)
        {
            #region Check Tab is Accessible or Not
            db_KISDEntities objContext = new db_KISDEntities();
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.Reports), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            int ReportType = !string.IsNullOrEmpty(rt) ? Convert.ToInt32(EncryptDecrypt.Decrypt(rt)) : 0;
            ViewBag.Title = ViewBag.PageTitle = ReportType == Convert.ToInt32(ReportsModel.ReportType.SystemAccessLogReport) ? "System Access Log Report" : "System Change Log Report";
            ViewBag.UserType = GetUserType(!string.IsNullOrEmpty(fm["UserType"]) ? Convert.ToString(fm["UserType"]) : "0");
            if (ReportType == Convert.ToInt32(ReportsModel.ReportType.SystemChangeLogReport))
            {
                ViewBag.UserType = GetUserType(!string.IsNullOrEmpty(fm["UserType"]) ? Convert.ToString(fm["UserType"]) : "0");
            }
            #region Ajax Call

            // Ajax call type 
            if (objresult != null)
            {
                AjaxRequest objAjaxRequest = JsonConvert.DeserializeObject<AjaxRequest>(objresult);//Convert json String to object Model
                if (objAjaxRequest.ajaxcall != null && !string.IsNullOrEmpty(objAjaxRequest.ajaxcall) && objresult != null && !string.IsNullOrEmpty(objresult))
                {
                    if (objAjaxRequest.ajaxcall == "paging")//Ajax Call type = paging i.e. Next|Previous|Back|Last
                    {
                        Session["pageNo"] = page;// stores the page no for status
                    }
                    else if (objAjaxRequest.ajaxcall == "sorting")//Ajax Call type = sorting i.e. column sorting Asc or Desc
                    {
                        page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : page);
                        Session["GridSortOption"] = gridSortOptions;
                        pagesize = (Session["PageSize"] != null ? Convert.ToInt32(Session["PageSize"].ToString()) : pagesize);
                    }
                    else if (objAjaxRequest.ajaxcall == "ddlPaging")//Ajax Call type = drop down paging i.e. drop down value 10, 25, 50, 100, ALL
                    {
                        Session["PageSize"] = (Request.QueryString["pagesize"] != null ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : pagesize);
                        Session["GridSortOption"] = gridSortOptions;
                        Session["pageNo"] = page;
                    }
                    else if (objAjaxRequest.ajaxcall == "status")//Ajax Call type = status i.e. Active/Inactive
                    {
                        page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                    objAjaxRequest.ajaxcall = null;//remove parameter value

                    //here we will check filter values
                    model.strFromDate = objAjaxRequest.qs_FilterFromDate;
                    model.strToDate = objAjaxRequest.qs_FilterToDate;
                    model.UserType = objAjaxRequest.qs_FilterUserType;
                    model.Title = objAjaxRequest.qs_FilterUserName;
                    model.ModuleType = objAjaxRequest.qs_FilterModuleType;
                }
                else
                {
                    TempData["AlertMessage"] = string.Empty;
                }
                objresult = string.Empty;
            }
            #endregion Ajax Call

            var pageSize = pagesize.HasValue ? pagesize.Value : Models.Common._pageSize;
            var Page = page.HasValue ? page.Value : Models.Common._currentPage;
            TempData["pager"] = pageSize;

            if (gridSortOptions.Column != null)
            {
                if (gridSortOptions.Column == "LoginDateTime"
                    || gridSortOptions.Column == "LogoutDateTime" || gridSortOptions.Column == "RoleNameTxt"
                    || gridSortOptions.Column == "UserNameTxt" || gridSortOptions.Column == "RoleNameTxt"
                    || gridSortOptions.Column == "ModuleNameTxt" || gridSortOptions.Column == "LogTypeTxt"
                    || gridSortOptions.Column == "LogDateTime"
                    )
                {

                }
                else
                {
                    gridSortOptions.Column = "NameTxt";
                }
            }

            var objService = new ReportService();
            var pagedViewModel = new PagedViewModel<ReportsModel>
            {
                ViewData = ViewData,
                Query = objService.GetReportData(ReportType).AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "NameTxt",
                Page = Page,
                PageSize = pageSize,
            };

            if (!string.IsNullOrEmpty(model.strFromDate) && (string.IsNullOrEmpty(command) || command != "Cancel"))
            {
                model.FromDate = Convert.ToDateTime(model.strFromDate);
                pagedViewModel.AddFilter("LoginDateTime", model.FromDate, a => a.LoginDateTime >= model.FromDate);
            }

            if (!string.IsNullOrEmpty(model.strToDate))
            {
                model.FromDate = Convert.ToDateTime(model.strToDate);
                pagedViewModel.AddFilter("LogoutDateTime", model.FromDate, a => a.LogoutDateTime >= model.ToDate);
            }

            if (!string.IsNullOrEmpty(model.Title) && (string.IsNullOrEmpty(command) || command != "Cancel"))
            {
                pagedViewModel.AddFilter("NameTxt", model.Title, a => a.NameTxt.ToLower().Contains(model.Title.ToLower()));
            }

            if (!string.IsNullOrEmpty(model.UserType))
            {
                if (model.UserType != "0")
                    pagedViewModel.AddFilter("UserType", model.UserType, a => a.UserRoleID.ToString() == model.UserType);
            }

            if (!string.IsNullOrEmpty(model.ModuleType))
            {
                if (model.ModuleType != "0")
                {
                    Models.Common.ModuleType ModuleName = (Models.Common.ModuleType)Enum.Parse(typeof(Models.Common.ModuleType), model.ModuleType);
                    pagedViewModel.AddFilter("ModuleType", model.ModuleType, a => a.ModuleNameTxt.ToString() == ModuleName.ToString());
                }
            }

            pagedViewModel.CommonSelectItemList = GetUserType(!string.IsNullOrEmpty(fm["UserType"]) ? Convert.ToString(fm["UserType"]) : "0");
            pagedViewModel.CommonSelectItemList2 = GetAllModules(!string.IsNullOrEmpty(fm["ModuleType"]) ? Convert.ToString(fm["ModuleType"]) : "0");
            if (!string.IsNullOrEmpty(model.strFromDate))
            {
                pagedViewModel.FromDate = Convert.ToDateTime(model.strFromDate);
            }
            if (!string.IsNullOrEmpty(model.strToDate))
            {
                pagedViewModel.ToDate = Convert.ToDateTime(model.strToDate);
            }
            pagedViewModel.UserType = model.UserType;
            pagedViewModel.Title = model.Title;
            pagedViewModel.ModuleType = model.ModuleType;
            pagedViewModel.Setup();

            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View((ReportType == Convert.ToInt32(ReportsModel.ReportType.SystemAccessLogReport) ? "SystemAccessLogPartial" : "SystemChangeLogPartial"), pagedViewModel);
            }
            else
            {
                return View(pagedViewModel);
            }

        }

        public static List<SelectListItem> GetUserType(string value)
        {
            db_KISDEntities _context = new db_KISDEntities();
            List<SelectListItem> items = new List<SelectListItem>();
            SelectListItem data = new SelectListItem();
            data.Text = "--- Select Type ---";
            data.Value = "0";
            items.Add(data);

            var UserTypes = _context.Roles.ToList();
            if (UserTypes != null)
            {
                foreach (var type in UserTypes)
                {
                    data = new SelectListItem();
                    data.Text = type.RoleNameTxt;
                    data.Value = type.RoleID.ToString();
                    items.Add(data);
                }

                if (!string.IsNullOrEmpty(value))
                {
                    items.Where(x => x.Value == value).FirstOrDefault().Selected = true;
                }
            }

            return items;
        }

        public ActionResult ViewDetails(long ChangeLogID)
        {
            db_KISDEntities _context = new db_KISDEntities();
            var changeLog = _context.SystemChangeLogs.Where(x => x.ChangeLogID == ChangeLogID).FirstOrDefault();

            if (changeLog != null)
            {
                ViewBag.LogType = changeLog.LogTypeTxt;
                ViewBag.ModuleName = changeLog.ModuleTxt;

                var changeLogDetails = _context.SystemChangeLogDetails.Where(x => x.ChangeLogID == changeLog.ChangeLogID).ToList();

                //update
                #region UPDATE
                if (changeLog.LogTypeTxt.ToLower() == "update")
                {
                    if (changeLogDetails != null)
                    {
                        string data = "<table class='tblDisplayFull'><tr><th class='tblDisplayth tblDisplaythleft'>Field</th><th class='tblDisplayth'>Old Value</th><th class='tblDisplayth'>New Value</th></tr>";

                        foreach (var s in changeLogDetails)
                        {
                            if (s.FieldNameTxt != "TypeMaster" && s.FieldNameTxt != "TypeMaster1"
                                && s.FieldNameTxt != "User" && s.FieldNameTxt != "User1")
                                data += "<tr><td class='common-center tblDisplaytd'>" + s.FieldNameTxt.Replace("Txt", "").Replace("Ind", "") + "</td><td class='common-center tblDisplaytd'>" + s.OldValueTxt + "</td><td class='common-center tblDisplaytd'>" + s.NewValueTxt + "</td></tr>";
                        }

                        data += "</table>";
                        data += "<div class='common-center updated-by'>Updated By " + changeLog.NameTxt + " on " + changeLog.LogDateTime + "</div>";
                        ViewBag.Data = data;
                    }
                }
                #endregion

                //add
                #region ADD
                if (changeLog.LogTypeTxt.ToLower() == "add")
                {
                    if (changeLogDetails != null)
                    {
                        string data = "<table class='tblDisplay'><tr><th class='tblDisplayth tblDisplaythleft'>Field</th><th class='tblDisplayth'>Value</th></tr>";

                        foreach (var s in changeLogDetails)
                        {
                            if (s.FieldNameTxt != "TypeMaster" && s.FieldNameTxt != "TypeMaster1"
                                && s.FieldNameTxt != "User" && s.FieldNameTxt != "User1"
                                && s.FieldNameTxt != "SchoolID" && s.FieldNameTxt != "IsDeletedInd"
                                && s.FieldNameTxt != "TypeMasterID" && s.FieldNameTxt != "SchoolCategoryID"
                                && s.FieldNameTxt != "CreateByID" && s.FieldNameTxt != "LastModifyByID"
                                )
                            {
                                data += "<tr><td class='common-center tblDisplaytd'>" + s.FieldNameTxt.Replace("Txt", "").Replace("Ind", "") + "</td><td class='common-center tblDisplaytd'>" + s.NewValueTxt + "</td></tr>";
                            }
                            else if (s.FieldNameTxt == "SchoolCategoryID")
                            {
                                var catName = GetCategoryName(Convert.ToInt64(s.NewValueTxt));
                                data += "<tr><td class='common-center tblDisplaytd'>" + s.FieldNameTxt.Replace("ID", "") + "</td><td class='common-center tblDisplaytd'>" + (!string.IsNullOrEmpty(catName) ? catName : "-") + "</td></tr>";
                            }
                            else if (s.FieldNameTxt == "CreateByID" || s.FieldNameTxt == "LastModifyByID")
                            {
                                var userName = GetUserName(Convert.ToInt64(s.NewValueTxt));
                                data += "<tr><td class='common-center tblDisplaytd'>" + s.FieldNameTxt.Replace("ID", "") + "</td><td class='common-center tblDisplaytd'>" + (!string.IsNullOrEmpty(userName) ? userName : "-") + "</td></tr>";
                            }
                        }

                        data += "</table>";
                        ViewBag.Data = data;
                    }
                }
                #endregion

                //delete
                #region DELETE
                if (changeLog.LogTypeTxt.ToLower() == "delete")
                {
                    string data = "<div class='common-center'>" + changeLog.NotesTxt + " by " + changeLog.NameTxt + " on " + changeLog.LogDateTime + "</div>";
                    ViewBag.Data = data;
                }
                #endregion
            }
            return View();
        }

        private List<SelectListItem> GetAllModules(string value)
        {
            List<SelectListItem> lst = new List<SelectListItem>();
            var values = Enum.GetValues(typeof(Models.Common.ModuleType));
            foreach (var item in values)
            {
                SelectListItem sli = new SelectListItem();
                sli.Value = Convert.ToInt32(item).ToString();
                sli.Text = item.ToString();
                lst.Add(sli);
            }
            SelectListItem objsli = new SelectListItem();
            objsli.Text = "--- Select Module ---";
            objsli.Value = "0";
            objsli.Selected = true;
            lst.Insert(0, objsli);

            if (!string.IsNullOrEmpty(value))
            {
                lst.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }

            return lst;
        }

        private string GetCategoryName(long ID)
        {
            var _context = new db_KISDEntities();
            var TypeMasterID = Convert.ToInt32(GalleryListingService.TypeMaster.SchoolCategory);
            return _context.Schools.Where(x => x.SchoolID == ID && x.TypeMasterID == TypeMasterID).Select(x => x.NameTxt).FirstOrDefault();
        }

        private string GetUserName(long ID)
        {
            var _context = new db_KISDEntities();
            return _context.Users.Where(x => x.UserID == ID).Select(x => x.FirstNameTxt + " " + x.LastNameTxt).FirstOrDefault();
        }
    }
}