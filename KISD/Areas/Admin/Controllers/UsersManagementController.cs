using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KISD.Areas.Admin.Models;
using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System.Data;
using System.Web.Security;
using KISD.Common.Infrastructure;
using System.Web.Routing;

namespace KISD.Areas.Admin.Controllers
{
    public class UsersManagementController : Controller
    {
        private UsersManagementListingService _service;
        db_KISDEntities objContext = new db_KISDEntities();
        /// <summary>
        /// Code to create instance Image Service class in constructor
        /// </summary>
        public UsersManagementController()
        {
            _service = new UsersManagementListingService();
        }

        [Authorize]
        [SessionExpire]
        /// <summary>
        /// this method will show the Gallery listing with all type master.
        /// </summary>
        /// <param name="Page">this parameter is used to get page number to be shown.</param>
        /// <param name="PageSize">this parameter is used to get no of recorde to be shown.</param>
        /// <param name="gridSortOptions">this parameter is used to get grid sorting option.</param>
        /// <param name="glt">this parameter is used to get type id of the gallery listing i.e. 1,2 or 3</param>
        /// <param name="CategoryId">show the category id in case of image type=3 i.e Photo Gallery</param>
        /// <param name="formCollection">this parameter is used to get controls collection on the page.</param>
        /// <param name="ObjResult"></param>
        /// <returns>view to enter image details.</returns>
        public ActionResult Index(int? Page, int? PageSize, GridSortOptions gridSortOptions, FormCollection formCollection, string ObjResult)
        {
            var db_obj = new db_KISDEntities();

            var currentLoggedUserId = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
            var CurrentUserRoleID = objContext.UserRoles.Where(x => x.UserID == currentLoggedUserId).FirstOrDefault().RoleID;
            if (CurrentUserRoleID > 2)//if not super admin and sub admin then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Rolllist = GetAllUserType(CurrentUserRoleID);
            var searchusername = "";
            var searchusertype = "";
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
                    else if (objAjaxRequest.ajaxcall == "search")//Ajax Call type = search 
                    {
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                    if (!string.IsNullOrEmpty(objAjaxRequest.qs_FilterUserType))
                    {
                        searchusertype = objAjaxRequest.qs_FilterUserType.Trim();
                    }
                    if (!string.IsNullOrEmpty(objAjaxRequest.qs_FilterUserName))
                    {
                        searchusername = objAjaxRequest.qs_FilterUserName.Trim();
                    }
                    objAjaxRequest.ajaxcall = null; ;//remove parameter value
                }

                //Ajax Call for update status for images
                if (objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && ObjResult != null && !string.IsNullOrEmpty(ObjResult))
                {
                    var ListingID = System.Convert.ToInt64(objAjaxRequest.hfid);
                    var userListing = objContext.Users.Find(ListingID);
                    if (userListing != null)
                    {
                        #region System Change Log
                        var oldresult = (from a in objContext.Users
                                         where a.UserID == ListingID
                                         select a).ToList();

                        DataTable dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
                        #endregion

                        userListing.StatusInd = objAjaxRequest.hfvalue == "1";
                        objContext.SaveChanges();

                        #region System Change Log
                        SystemChangeLog objSCL = new SystemChangeLog();
                        User objuser = objContext.Users.Where(x => x.UserID == currentLoggedUserId).FirstOrDefault();
                        objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                        objSCL.UsernameTxt = objuser.UserNameTxt;
                        objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                        objSCL.ModuleTxt = "Users Listing";
                        objSCL.LogTypeTxt = "Update";
                        objSCL.NotesTxt = "Status updated for " + userListing.FirstNameTxt + " " + userListing.LastNameTxt;
                        objSCL.LogDateTime = DateTime.Now;
                        objContext.SystemChangeLogs.Add(objSCL);
                        objContext.SaveChanges();

                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                        var newResult = (from x in objContext.Users
                                         where x.UserID == userListing.UserID
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
                        objAjaxRequest.hfid = null;//remove parameter value
                        objAjaxRequest.hfvalue = null;//remove parameter value

                        PageSize = ((Request.QueryString["pagesize"] != null && Request.QueryString["pagesize"].ToString() != "All") ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : PageSize);
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
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

            ViewBag.Title = ViewBag.PageTitle = "User Listing";

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
                gridSortOptions.Column = "UserCreateDate";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            if (gridSortOptions.Column == "Name" || gridSortOptions.Column == "UserCreateDate"
                || gridSortOptions.Column == "UserNameTxt")
            {

            }
            else
            {
                gridSortOptions.Column = "UserCreateDate";
            }
            //.. Code for get records as page view model
            var pagesize = PageSize.HasValue ? PageSize.Value : Models.Common._pageSize;
            var page = Page.HasValue ? Page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;

            var pagedViewModel = new PagedViewModel<UsersModel>
            {
                ViewData = ViewData,
                Query = _service.GetUserListingView(CurrentUserRoleID,searchusername,searchusertype),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "UserCreateDate",
                Page = page,
                PageSize = pagesize,
            }.Setup();
            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("UserListingPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }

        [Authorize]
        [SessionExpire]
        public ActionResult Create(string UID)
        {
            // Get role type and work acdngly

            var currentLoggedUserId = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
            var CurrentUserRoleID = objContext.UserRoles.Where(x => x.UserID == currentLoggedUserId).FirstOrDefault().RoleID;
            if (CurrentUserRoleID > 2)//if not super admin and sub admin then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }

            UID = !string.IsNullOrEmpty(Convert.ToString(UID)) ? EncryptDecrypt.Decrypt(UID) : "0";
            var UserId = Convert.ToInt64(UID);
            var objUser = new UsersModel();
            ViewBag.Title = ViewBag.PageTitle = (UserId > 0 ? "Edit " : "Add ") + " User Details ";
            ViewBag.Submit = UserId > 0 ? "Update" : "Save";
            ViewBag.UserCreateDate = DateTime.Now.ToShortDateString();
            ViewBag.UserID = UID;
            string isActive = "True";
            Session["Edit/Delete"] = "Edit";
            ViewBag.UserTypeID = 0;
            objUser.DepartmentUsersList = GetDepartments();
          

            if (UserId > 0)
            {
                var objUserContext = objContext.Users.Find(UserId);
                if (objUserContext != null)
                {
                    objUser.UserID = objUserContext.UserID;
                    objUser.Status = objUserContext.StatusInd;
                    objUser.FirstName = objUserContext.FirstNameTxt;
                    objUser.LastName = objUserContext.LastNameTxt;
                    var selectedDepts = objContext.UserDepartments.Where(m => m.UserID == objUser.UserID).Select(m => m.DepartmentID).ToArray();
                    objUser.SelectedDepartment = Array.ConvertAll<long, string>(selectedDepts,
                                                                    delegate (long i)
                                                                    {
                                                                        return i.ToString();
                                                                    });
                    objUser.UserNameTxt = objUserContext.UserNameTxt;
                    objUser.Password = objUserContext.PasswordTxt;
                    objUser.UserCreateDate = objUserContext.UserCreateDate;
                    objUser.RolesList = GetAllUserType(CurrentUserRoleID);
                    ViewBag.UserCreateDate = objUserContext.UserCreateDate.Value.ToShortDateString();
                    objUser.Email = objUserContext.EmailTxt;
                    isActive = objUserContext.StatusInd.ToString();
                    ViewBag.UserTypeID = objUser.UserRoleID = objContext.UserRoles.Where(m => m.UserID == UserId).Select(m => m.RoleID).FirstOrDefault();
                }
            }
            else
            {
                objUser.Status = true;
                objUser.UserCreateDate = DateTime.Now;
            }
            ViewBag.IsActiveInd = Models.Common.GetStatusListBoolean(isActive);
            ViewBag.RolesList = GetAllUserType(CurrentUserRoleID);

            return View(objUser);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(UsersModel model, string command, FormCollection fm)
        {
            var currentLoggedUserId = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
            var CurrentUserRoleID = objContext.UserRoles.Where(x => x.UserID == currentLoggedUserId).FirstOrDefault().RoleID;
            if (CurrentUserRoleID > 2)//if not super admin and sub admin then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }

            var EncryptUserID = EncryptDecrypt.Encrypt(model.UserID.ToString());

            var rvd = new RouteValueDictionary();
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "UserCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
            rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
            rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
            ViewBag.Title = ViewBag.PageTitle = (model.UserID > 0 ? "Edit " : "Add ") + " User Details ";

            ViewBag.Submit = model.UserID > 0 ? "Update" : "Save";
            ViewBag.CreateDate = DateTime.Now.ToShortDateString();
            ViewBag.UserCreateDate = DateTime.Now.ToShortDateString();

            #region System Change Log
            DataTable dtOld;
            var oldresult = (from a in objContext.Users
                             where a.UserID == model.UserID
                             select a).ToList();
            dtOld = KISD.Areas.Admin.Models.Common.LINQResultToDataTable(oldresult);
            #endregion

            var objDBContent = new db_KISDEntities();
            var objctUser = new User();
            ViewBag.IsActiveInd = Models.Common.GetStatusListBoolean(model.Status.ToString());
            ViewBag.UserID = model.UserID;
            ViewBag.isValid = "1";
            ViewBag.RolesList = GetAllUserType(currentLoggedUserId);
            ViewBag.UserTypeID = model.UserRoleID;
            if (string.IsNullOrEmpty(command))
            {
                if (model.UserID > 0)
                {
                    objctUser = objDBContent.Users.Where(x => x.UserID == model.UserID).FirstOrDefault();
                }

                model.DepartmentUsersList = GetDepartments();


                var UserNameCount = objDBContent.Users.Where(x => x.UserNameTxt.ToLower().Trim() == model.UserNameTxt.ToLower().Trim() && x.UserID != model.UserID).Count();
                if (UserNameCount > 0)
                {
                    var selectedDepts = objContext.UserDepartments.Where(m => m.UserID == model.UserID).Select(m => m.DepartmentID).ToArray();
                    model.SelectedDepartment = Array.ConvertAll<long, string>(selectedDepts,
                                                                    delegate (long i)
                                                                    {
                                                                        return i.ToString();
                                                                    });

                    ModelState.AddModelError("UserNameTxt", "Username already exists.");
                    ViewBag.isValid = "0";
                    return View(model);
                }

                objctUser.FirstNameTxt = model.FirstName;
                objctUser.LastNameTxt = model.LastName;
                objctUser.UserNameTxt = model.UserNameTxt;
                objctUser.EmailTxt = model.Email;
                //  objctUser.PasswordTxt = !string.IsNullOrEmpty(model.Password)? CustomMembershipProvider.GetMd5Hash(model.Password): objctUser.PasswordTxt;
                objctUser.PasswordTxt = (!string.IsNullOrEmpty(model.ChangedPassword) ? CustomMembershipProvider.GetMd5Hash(model.ChangedPassword) :
                     (!string.IsNullOrEmpty(model.Password) ? CustomMembershipProvider.GetMd5Hash(model.Password) : objctUser.PasswordTxt)
                     );

                objctUser.StatusInd = Convert.ToBoolean(fm["IsActiveInd"]);
                objctUser.IsDeletedInd = false;
                objctUser.CreateDate = DateTime.Now;
                objctUser.UserCreateDate = model.UserID > 0 ? objctUser.CreateDate : model.UserCreateDate;
                objctUser.CreateByID = model.UserID > 0 ? objctUser.CreateByID : Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                objctUser.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                objctUser.LastModifyDate = DateTime.Now;
                if (model.UserID == 0)
                {
                    objDBContent.Users.Add(objctUser);
                    var UserID = model.UserID != 0 ? model.UserID : objctUser.UserID;
                    var obj = new UserRole();
                    obj.UserID = Convert.ToInt32(UserID);
                    obj.RoleID = Convert.ToInt16(model.UserRoleID);
                    objDBContent.UserRoles.Add(obj);
                    objDBContent.SaveChanges();
                }
                else
                {
                    var UserID = model.UserID != 0 ? model.UserID : objctUser.UserID;
                    var obj = new UserRole();
                    obj = objDBContent.UserRoles.Where(x => x.UserID == model.UserID).FirstOrDefault();
                    obj.RoleID = Convert.ToInt16(model.UserRoleID);
                }
                try
                {
                    objDBContent.SaveChanges();
                    var newUserID = objctUser.UserID;

                    #region System Change Log
                    SystemChangeLog objSCL = new SystemChangeLog();
                    long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                    User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                    objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                    objSCL.UsernameTxt = objuser.UserNameTxt;
                    objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                    objSCL.ModuleTxt = "Users";
                    objSCL.LogTypeTxt = model.UserID > 0 ? "Update" : "Add";
                    objSCL.NotesTxt = "User Details" + (objctUser.UserID > 0 ? " updated for " : "  added for ") + objctUser.FirstNameTxt + " " + objctUser.LastNameTxt;
                    objSCL.LogDateTime = DateTime.Now;
                    objContext.SystemChangeLogs.Add(objSCL);
                    objContext.SaveChanges();

                    objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                    var newResult = (from x in objContext.Users
                                     where x.UserID == newUserID
                                     select x);
                    DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                    foreach (DataColumn col in dtNew.Columns)
                    {
                        if (dtOld.Rows.Count > 0)
                        {
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
                    }
                    #endregion

                    #region Save Department Users
                    if (model.UserRoleID == 3)
                    {
                        var objUserDept = objContext.UserDepartments.Where(x => x.UserID == objctUser.UserID).ToList();
                        if (objUserDept.Count > 0)
                        {
                            foreach (var s in objUserDept)
                            {
                                objContext.UserDepartments.Remove(s);
                                objContext.SaveChanges();
                            }
                        }

                        if (model.SelectedDepartment != null)
                        {
                            foreach (var list in model.SelectedDepartment)
                            {
                                UserDepartment objUserDepartment = new UserDepartment();
                                objUserDepartment.DepartmentID = Convert.ToInt64(list);
                                objUserDepartment.UserID = model.UserID;
                                objUserDepartment.CreateDate = DateTime.Now;
                                objContext.UserDepartments.Add(objUserDepartment);
                                objContext.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        var objUserDept = objContext.UserDepartments.Where(x => x.UserID == objctUser.UserID).ToList();
                        if (objUserDept.Count > 0)
                        {
                            foreach (var s in objUserDept)
                            {
                                objContext.UserDepartments.Remove(s);
                                objContext.SaveChanges();
                            }
                        }

                        var objUserPermissions = objContext.UserPermissions.Where(x => x.UserID == objctUser.UserID).ToList();
                        if (objUserPermissions != null && objUserPermissions.Count > 0)
                        {
                            foreach (var up in objUserPermissions)
                            {
                                objContext.UserPermissions.Remove(up);
                            }
                            objContext.SaveChanges();
                        }
                    }
                    #endregion

                    TempData["AlertMessage"] = "User details " + ((objctUser.UserID == 0) ? "saved" : "updated") + " successfully.";
                }
                catch (Exception ex)
                {
                    TempData["AlertMessage"] = "Some error occured. Please try after some time.";
                }
            }
            return RedirectToAction("Index", "UsersManagement", rvd);
        }

        /// <summary>
        /// Set the status of the User as deleted. Donot delete from the table.
        /// </summary>
        /// <param name="UID"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(string UID)
        {
            UID = !string.IsNullOrEmpty(Convert.ToString(UID)) ? EncryptDecrypt.Decrypt(UID) : "0";
            long UserID = Convert.ToInt32(UID);
            var rvd = new RouteValueDictionary();
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "UserCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            Session["Edit/Delete"] = "Delete";
            if (UserID > 0)
            {
                try
                {
                    using (var objContext = new db_KISDEntities())
                    {
                        #region System Change Log
                        var oldresult = (from a in objContext.Users
                                         where a.UserID == UserID
                                         select a).ToList();
                        DataTable dtOld = Models.Common.LINQResultToDataTable(oldresult);
                        #endregion

                        var obj = objContext.Users.Where(x => x.UserID == UserID).ToList();
                        foreach (var value in obj)
                        {
                            value.IsDeletedInd = true;
                            objContext.SaveChanges();

                            #region Delete Selected Departments for the User
                            var objUserDept = objContext.UserDepartments.Where(x => x.UserID == UserID).ToList();
                            if (objUserDept.Count() > 0)
                            {
                                foreach (var s in objUserDept)
                                {
                                    objContext.UserDepartments.Remove(s);
                                    objContext.SaveChanges();
                                }
                            }
                            #endregion

                            #region System Change Log
                            SystemChangeLog objSCL = new SystemChangeLog();
                            long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                            User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                            objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.UsernameTxt = objuser.UserNameTxt;
                            objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                            objSCL.ModuleTxt = "Users";
                            objSCL.LogTypeTxt = "Delete";
                            objSCL.NotesTxt = "Users Details deleted for " + value.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.LogDateTime = DateTime.Now;
                            objContext.SystemChangeLogs.Add(objSCL);
                            objContext.SaveChanges();
                            objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                            var newResult = (from x in objContext.RightSections
                                             where x.RightSectionID == UserID
                                             select x);
                            DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                            if (dtOld.Rows.Count > 0)
                            {
                                foreach (DataColumn col in dtNew.Columns)
                                {
                                    SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                                    objSCLD.ChangeLogID = objSCL.ChangeLogID;
                                    objSCLD.FieldNameTxt = col.ColumnName.ToString();
                                    objSCLD.OldValueTxt = dtOld.Rows[0][col.ColumnName].ToString();
                                    objSCLD.NewValueTxt = "";// dtNew.Rows[0][col.ColumnName].ToString();
                                    objContext.SystemChangeLogDetails.Add(objSCLD);
                                    objContext.SaveChanges();
                                }
                            }
                            #endregion
                        }
                        TempData["AlertMessage"] = " User details deleted successfully.";
                    }
                }
                catch (Exception e)
                {
                    TempData["AlertMessage"] = "Some error occured while deleting the User, Please try again later.";
                }
            }
            int? Page = 1;
            var count = 1;
            using (var objContext = new db_KISDEntities())
            {
                count = objContext.Users.Where(x => x.IsDeletedInd == false).Count();
            }
            var page = Request.QueryString["page"] ?? "1";
            var pagesize = Request.QueryString["pagesize"] ?? "10";
            if (Convert.ToInt32(page) > 1)
                Page = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
            rvd.Add("page", Page);
            return Json(Url.Action("Index", "UsersManagement", rvd));
        }

        #region Manage User Permission
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ContentID"></param>
        /// <param name="TypeID"></param>
        /// <param name="SubMenuTypeID"></param>
        /// <param name="IsFromMenu"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResult ManageUserPermission(string UID)
        {
            UID = !string.IsNullOrEmpty(Convert.ToString(UID)) ? EncryptDecrypt.Decrypt(UID) : "0";
            long UserID = Convert.ToInt32(UID);
            using (var objContext = new db_KISDEntities())
            {
                var objModel = new UserPermissionsModel();
                objModel.PermissionList = GetUserPermissions();
                var selectedSections = objContext.UserPermissions.Where(m => m.UserID == UserID).Select(m => m.PageID.Value).ToArray();

                if (selectedSections.Count() == 0)
                {
                    string[] s = { "0" };
                    objModel.SelectedUserPermissions = s;
                }
                else
                {
                    objModel.SelectedUserPermissions = Array.ConvertAll<long, string>(selectedSections,
                                                                  delegate (long i)
                                                                  {
                                                                      return i.ToString();
                                                                  });
                }
                objModel.User = objContext.Users.Where(x => x.UserID == UserID).FirstOrDefault();
                objModel.UserRoleID = (short?)objContext.UserRoles.Where(x => x.UserID == UserID).FirstOrDefault().RoleID;
                objModel.Role = objContext.Roles.Where(x => x.RoleID == objModel.UserRoleID).FirstOrDefault();

                ViewBag.Title = ViewBag.PageTitle = "Manage User Permissions ";
                return View("ManageUserPermission", objModel);
            }
        }


        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ManageUserPermission(UserPermissionsModel model, string command, FormCollection fm)
        {
            #region Route value directory
            var rvd = new RouteValueDictionary();
            rvd.Add("page", Request.QueryString["page"] ?? "1");
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", "UserCreateDate");
            rvd.Add("Direction", "Descending");
            rvd.Add("UID", model.UserID);
            #endregion

            using (var objContext = new db_KISDEntities())
            {
                if (string.IsNullOrEmpty(command))
                {
                    if (model.SelectedUserPermissions != null)
                    {
                        #region Save User Permissions 
                        long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                        User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();

                        var objUserPermissions = objContext.UserPermissions.Where(x => x.UserID == model.User.UserID).ToList();
                        if (objUserPermissions != null && objUserPermissions.Count > 0)
                        {
                            foreach (var up in objUserPermissions)
                            {
                                objContext.UserPermissions.Remove(up);
                            }
                            objContext.SaveChanges();
                        }
                        foreach (var item in model.SelectedUserPermissions)
                        {
                            if (item != "0")
                            {
                                var obj = new UserPermission();
                                obj.UserID = model.User.UserID;
                                obj.PageID = Convert.ToInt32(item);
                                obj.CreateDate = DateTime.Now;
                                obj.IsAccessInd = true;
                                obj.CreateByID = userid;
                                obj.LastModifyByID = userid;
                                obj.LastModifyDate = DateTime.Now;
                                obj.UserRoleID = model.UserRoleID;
                                objContext.UserPermissions.Add(obj);
                                objContext.SaveChanges();
                            }
                        }
                        #endregion
                    }
                    TempData["AlertMessage"] = "Manage User Permissions updated successfully.";
                }
                return RedirectToAction("Index", "UsersManagement", rvd);
            }
        }

        #endregion

        #region Private Methods
        private SelectList GetAllUserType(Int64 currentRoleID)
        {
            var objContext = new db_KISDEntities();
            var list = objContext.Roles.OrderBy(x => x.RoleID).Where(x => x.RoleID > currentRoleID).Select(x => new RoleModel()
            {
                RoleId = x.RoleID,
                RoleName = x.RoleNameTxt
            }).ToList();
            var objRole = new RoleModel();
            objRole.RoleName = "--- Select User Type ---";
            objRole.RoleId = 0;
            list.Insert(0, objRole);
            var objselectlist = new SelectList(list, "RoleID", "RoleName");
            return objselectlist;
        }

        private List<SelectListItem> GetUserPermissions()
        {
            List<SelectListItem> lst = new List<SelectListItem>();
            var values = Enum.GetValues(typeof(Models.Common.ModuleType));
            foreach (var item in values)
            {
                if (item.ToString() != "Users" && item.ToString() != "News" && item.ToString() != "Events")
                {
                    SelectListItem sli = new SelectListItem();
                    sli.Value = Convert.ToInt32(item).ToString();
                    sli.Text = item.ToString();
                    lst.Add(sli);
                }
            }

            SelectListItem objsli = new SelectListItem();
            objsli.Text = "--- Select Module ---";
            objsli.Value = "0";
            objsli.Selected = true;
            lst.Insert(0, objsli);
            return lst;
        }

        private List<SelectListItem> GetDepartments()
        {
            List<SelectListItem> lst = new List<SelectListItem>();
            var values = objContext.Departments.Where(x =>x.ParentID==null && x.IsDeletedInd == false && x.StatusInd == true).ToList();
            foreach (var item in values)
            {
                SelectListItem sli = new SelectListItem();
                sli.Value = item.DepartmentID.ToString();
                sli.Text = item.NameTxt;
                lst.Add(sli);
            }

            SelectListItem objsli = new SelectListItem();
            objsli.Text = "--- Select Departments ---";
            objsli.Value = "0";
            objsli.Selected = true;
            lst.Insert(0, objsli);
            return lst;
        }
        #endregion
    }
}