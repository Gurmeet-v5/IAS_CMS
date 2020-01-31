using KISD.Areas.Admin.Models;
using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using static KISD.Areas.Admin.Models.Common;

namespace KISD.Areas.Admin.Controllers
{
    public class StaffController : Controller
    {
        private StaffModelService _service;
        db_KISDEntities objContext = new db_KISDEntities();
        /// <summary>
        /// Code to create instance Image Service class in constructor
        /// </summary>
        public StaffController()
        {
            _service = new StaffModelService();
        }

        [Authorize]
        [SessionExpire]
        public ActionResult Index(int? Page, int? PageSize, GridSortOptions gridSortOptions, FormCollection formCollection, string ObjResult)
        {
            var objContext = new db_KISDEntities();
            var currentLoggedUserId = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
            var CurrentUserRoleID = objContext.UserRoles.Where(x => x.UserID == currentLoggedUserId).FirstOrDefault().RoleID;

            #region Check Tab is Accessible or Not
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.ParentStudents), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            ViewBag.Title = ViewBag.PageTitle = "Staff Listing";

            //*******************Fill Values if Display order contains null values***************************
            var displayOrderList = objContext.Staffs.Where(x => x.IsDeletedInd == false).ToList();
            foreach (var item in displayOrderList)
            {
                if (string.IsNullOrEmpty(item.DisplayOrderNbr.ToString()))
                {
                    var objContentData = objContext.Staffs.Where(x => x.StaffID == item.StaffID && x.IsDeletedInd == false).FirstOrDefault();
                    var displayOrder1 = (displayOrderList.Max(x => x.DisplayOrderNbr)) == null ? 1 : displayOrderList.Max(x => x.DisplayOrderNbr).Value + 1;
                    objContentData.DisplayOrderNbr = displayOrder1;
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

                    objAjaxRequest.ajaxcall = null; ;//remove parameter value
                }

                //Ajax Call for update status for images
                if (objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && ObjResult != null && !string.IsNullOrEmpty(ObjResult))
                {
                    var ListingID = Convert.ToInt64(objAjaxRequest.hfid);
                    var Listing = objContext.Staffs.Find(ListingID);
                    if (Listing != null)
                    {
                        #region System Change Log
                        var oldresult = (from a in objContext.Staffs
                                         where a.StaffID == ListingID
                                         select a).ToList();

                        DataTable dtOld = Models.Common.LINQResultToDataTable(oldresult);
                        #endregion

                        if (objAjaxRequest.qs_Type == "status")
                        {
                            Listing.StatusInd = objAjaxRequest.hfvalue == "1";
                            TempData["Message"] = "Status updated successfully.";
                        }
                        else if (objAjaxRequest.qs_Type == "displayorder")
                        {
                            try
                            {
                                if (StaffModelService.ChangeDisplayOrder(Convert.ToInt64(Listing.DisplayOrderNbr), Convert.ToInt64(objAjaxRequest.qs_value)))
                                {
                                    TempData["Message"] = "Display Order has been changed successfully.";
                                }
                            }
                            catch
                            {
                                TempData["Message"] = "Some Error Occured while changing Display Order, Please try again later.";
                            }
                        }
                        objContext.SaveChanges();

                        #region System Change Log
                        SystemChangeLog objSCL = new SystemChangeLog();
                        User objuser = objContext.Users.Where(x => x.UserID == currentLoggedUserId).FirstOrDefault();
                        objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                        objSCL.UsernameTxt = objuser.UserNameTxt;
                        objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                        objSCL.ModuleTxt = "Staff Listing";
                        objSCL.LogTypeTxt = "Update";
                        objSCL.NotesTxt = (objAjaxRequest.qs_Type == "status" ? "Status " : "Display order ") + "updated for " + Listing.FirstNameTxt +" "+ Listing.LastNameTxt;
                        objSCL.LogDateTime = DateTime.Now;
                        objContext.SystemChangeLogs.Add(objSCL);
                        objContext.SaveChanges();

                        objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                        var newResult = (from x in objContext.Staffs
                                         where x.StaffID == Listing.StaffID
                                         select x);

                        DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
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
                gridSortOptions.Column = "StaffCreateDate";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            if (gridSortOptions.Column == "NameTxt" || gridSortOptions.Column == "TitleTxt"
                || gridSortOptions.Column == "DisplayOrderNbr" || gridSortOptions.Column == "DesignationTxt"
                )
            {

            }
            else
            {
                gridSortOptions.Column = "StaffCreateDate";
            }
            //.. Code for get records as page view model
            var pagesize = PageSize.HasValue ? PageSize.Value : Models.Common._pageSize;
            var page = Page.HasValue ? Page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;

            var pagedViewModel = new PagedViewModel<StaffModel>
            {
                ViewData = ViewData,
                Query = _service.GetStaff().AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "StaffCreateDate",
                Page = page,
                PageSize = pagesize,
            }.Setup();
            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("StaffListingPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }

        [Authorize]
        [SessionExpire]
        public ActionResult Create(string stid)
        {
            var currentLoggedUserId = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
            var CurrentUserRoleID = objContext.UserRoles.Where(x => x.UserID == currentLoggedUserId).FirstOrDefault().RoleID;
            ViewBag.LiTitle = "Staff Listing";
            stid = !string.IsNullOrEmpty(Convert.ToString(stid)) ? EncryptDecrypt.Decrypt(stid) : "0";
            var StaffId = Convert.ToInt64(stid);
            var objModel = new StaffModel();

            #region Check Tab is Accessible or Not
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.ParentStudents), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

            ViewBag.Title = ViewBag.PageTitle = (StaffId > 0 ? "Edit " : "Add ") + " Staff Details ";
            ViewBag.Submit = StaffId > 0 ? "Update" : "Save";
            ViewBag.UserCreateDate = DateTime.Now.ToShortDateString();
            ViewBag.UserID = stid;
            string isActive = "True";
            Session["Edit/Delete"] = "Edit";
            ViewBag.UserTypeID = 0;
            objModel.DepartmentsList = GetDepartments();
            objModel.SubDepartmentsList = GetSubDepartments(0);
            ViewBag.SchoolCategoryList = GetSchoolCategories(0);
            ViewBag.SchoolList = GetSchools(0);

            var selectedDepts = objContext.DepartmentStaffs.Where(m => m.StaffID == StaffId).Select(m => m.DepartmentID.Value).ToArray();
            objModel.SelectedDepartment = Array.ConvertAll<long, string>(selectedDepts,
                                                            delegate (long i)
                                                            {
                                                                return i.ToString();
                                                            });

            var selectedSubDepts = objContext.DepartmentStaffs.Where(m => m.StaffID == StaffId).Select(m => m.DepartmentID.Value).ToArray();
            objModel.SelectedSubDepartment = Array.ConvertAll<long, string>(selectedSubDepts,
                                                            delegate (long i)
                                                            {
                                                                return i.ToString();
                                                            });

            objModel.strStaffCreateDate = DateTime.Now.ToShortDateString();
            if (StaffId > 0)
            {
                var objBOMContext = objContext.Staffs.Find(StaffId);
                if (objBOMContext != null)
                {
                    objModel.StaffID = objBOMContext.StaffID;
                    objModel.StatusInd = objBOMContext.StatusInd.Value;
                    objModel.StaffCreateDate = objBOMContext.StaffCreateDate.HasValue ? objBOMContext.StaffCreateDate.Value : DateTime.Now;
                    objModel.PhoneTxt = objBOMContext.PhoneTxt;
                    objModel.CreateByID = objBOMContext.CreateByID;
                    objModel.CreateDate = objBOMContext.CreateDate.HasValue ? objBOMContext.CreateDate.Value : DateTime.Now;
                    objModel.EmailTxt = objBOMContext.EmailTxt;
                    objModel.DisplayOrderNbr = objBOMContext.DisplayOrderNbr;
                    objModel.DesignationTxt = objBOMContext.DesignationTxt;
                    objModel.IsDeletedInd = objBOMContext.IsDeletedInd;
                    objModel.LastModifyByID = objBOMContext.LastModifyByID;
                    objModel.LastModifyDate = objBOMContext.LastModifyDate.HasValue ? objBOMContext.LastModifyDate.Value : DateTime.Now;
                    objModel.FirstNameTxt = objBOMContext.FirstNameTxt;
                    objModel.LastNameTxt = objBOMContext.LastNameTxt;
                    objModel.strStaffCreateDate = objBOMContext.StaffCreateDate.HasValue ? objBOMContext.StaffCreateDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                    isActive = objBOMContext.StatusInd.ToString();
                    ViewBag.SchoolCategoryList = GetSchoolCategories(objBOMContext.SchoolCategoryID.HasValue ? objBOMContext.SchoolCategoryID.Value : 0);
                    ViewBag.SchoolList = GetSchools(objBOMContext.SchoolID.HasValue ? objBOMContext.SchoolID.Value : 0);

                    //Select Sub Departments
                    if (objModel.SelectedSubDepartment.Count() > 0)
                    {
                        List<SelectListItem> lst = new List<SelectListItem>();
                        foreach (var dept in objModel.SelectedSubDepartment)
                        {
                            long SubDeptID = Convert.ToInt64(dept);
                            var values = objContext.Departments.Where(x => x.IsDeletedInd == false && x.StatusInd == true && x.ParentID == SubDeptID).ToList();
                            if (values != null)
                            {
                                foreach (var item in values)
                                {
                                    SelectListItem sli = new SelectListItem();
                                    sli.Value = item.DepartmentID.ToString();
                                    sli.Text = item.NameTxt;
                                    lst.Add(sli);
                                }
                            }
                        }

                        SelectListItem objsli = new SelectListItem();
                        objsli.Text = "--- Select Sub Departments ---";
                        objsli.Value = "0";
                        objsli.Selected = true;
                        lst.Insert(0, objsli);

                        objModel.SubDepartmentsList = lst;
                    }

                }
            }
            else
            {
                objModel.StatusInd = true;
                objModel.StaffCreateDate = DateTime.Now;
            }
            ViewBag.IsActiveInd = Models.Common.GetStatusListBoolean(isActive);

            return View(objModel);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(StaffModel model, string command, FormCollection fm)
        {
            var currentLoggedUserId = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
            var CurrentUserRoleID = objContext.UserRoles.Where(x => x.UserID == currentLoggedUserId).FirstOrDefault().RoleID;
            ViewBag.LiTitle = "Staff Listing";
            var EncryptStaffID = EncryptDecrypt.Encrypt(model.StaffID.ToString());

            var rvd = new RouteValueDictionary();
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "CreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
            rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
            rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
            ViewBag.Title = ViewBag.PageTitle = (model.StaffID > 0 ? "Edit " : "Add ") + " Board of Member Details ";

            ViewBag.Submit = model.StaffID > 0 ? "Update" : "Save";
            ViewBag.CreateDate = DateTime.Now.ToShortDateString();
            ViewBag.UserCreateDate = DateTime.Now.ToShortDateString();
            //model.RightSections = GetAllRightSections();

            #region System Change Log
            DataTable dtOld;
            var oldresult = (from a in objContext.Staffs
                             where a.StaffID == model.StaffID
                             select a).ToList();
            dtOld = Models.Common.LINQResultToDataTable(oldresult);
            #endregion

            var objDBContent = new db_KISDEntities();
            var objctST = new Staff();
            ViewBag.IsActiveInd = Models.Common.GetStatusListBoolean(!string.IsNullOrEmpty(fm["IsActiveInd"]) ? Convert.ToString(fm["IsActiveInd"]) : "0");
            ViewBag.StaffID = model.StaffID;
            ViewBag.isValid = "1";
            //ViewBag.RolesList = GetAllUserType(currentLoggedUserId);

            var file = Request.Files.Count > 0 ? Request.Files[0] : null;

            if (string.IsNullOrEmpty(command))
            {
                if (model.StaffID > 0)
                {
                    objctST = objDBContent.Staffs.Where(x => x.StaffID == model.StaffID).FirstOrDefault();
                }
                else
                {
                    objctST = new Staff();
                }

                //var NameCount = objDBContent.BoardOfMembers.Where(x => x.URLTxt.ToLower().Trim() == model.URLTxt.ToLower().Trim() && x.BoardMemberID != model.BoardMemberID && x.IsDeletedInd == false).Count();
                //if (NameCount > 0)
                //{
                //    //var TypeMasterID = Convert.ToInt64(GalleryListingService.TypeMaster.BoardOfMembers);
                //    //var selectedSections = objContext.ListingRightSections.Where(m => m.ListingID == model.BoardMemberID && m.TypeMasterID == TypeMasterID).Select(m => m.RightSectionID).ToArray();

                //    //model.SelectedRightSections = Array.ConvertAll<long, string>(selectedSections,
                //    //                                                delegate (long i)
                //    //                                                {
                //    //                                                    return (string)i.ToString();
                //    //                                                });

                //    ModelState.AddModelError("URLTxt", "Page URL already exists.");
                //    ViewBag.isValid = "0";
                //    return View(model);
                //}

                objctST.StaffCreateDate = Convert.ToDateTime(model.strStaffCreateDate + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
                objctST.PhoneTxt = model.PhoneTxt;
                objctST.CreateDate = DateTime.Now;
                objctST.EmailTxt = model.EmailTxt;
                objctST.DisplayOrderNbr = model.DisplayOrderNbr;
                objctST.FirstNameTxt = model.FirstNameTxt;
                objctST.LastNameTxt = model.LastNameTxt;
                objctST.DesignationTxt = model.DesignationTxt;
                objctST.SchoolCategoryID = fm["SchoolCategoryList"] != null ? Convert.ToInt64(fm["SchoolCategoryList"]) : 0;
                objctST.SchoolID = fm["SchoolList"] != null ? Convert.ToInt64(fm["SchoolList"]) : 0;
                objctST.StatusInd = Convert.ToBoolean(fm["IsActiveInd"]);
                objctST.IsDeletedInd = false;
                objctST.CreateDate = model.StaffID > 0 ? objctST.CreateDate : Convert.ToDateTime(model.strStaffCreateDate);
                objctST.CreateByID = model.StaffID > 0 ? objctST.CreateByID : Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                objctST.LastModifyByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                objctST.LastModifyDate = DateTime.Now;

                try
                {
                    if (model.StaffID == 0)
                    {
                        objDBContent.Staffs.Add(objctST);
                    }
                    objDBContent.SaveChanges();
                    var newID = objctST.StaffID;

                    #region Save Department Staff

                    var objDept = objContext.DepartmentStaffs.Where(x => x.StaffID == objctST.StaffID).ToList();
                    if (objDept != null && objDept.Count > 0)
                    {
                        foreach (var up in objDept)
                        {
                            objContext.DepartmentStaffs.Remove(up);
                        }
                        objContext.SaveChanges();
                    }

                    //Departments
                    if (model.SelectedDepartment != null)
                    {
                        foreach (var s in model.SelectedDepartment)
                        {
                            long DeptId = Convert.ToInt64(s);
                            ManageStaffDetails(objctST.StaffID, DeptId, true);
                        }
                    }

                    //Sub Departments
                    if (model.SelectedSubDepartment != null)
                    {
                        foreach (var s in model.SelectedSubDepartment)
                        {
                            long SubDeptId = Convert.ToInt64(s);
                            ManageStaffDetails(objctST.StaffID, SubDeptId, false);
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
                    objSCL.ModuleTxt = "Staff";
                    objSCL.LogTypeTxt = model.StaffID > 0 ? "Update" : "Add";
                    objSCL.NotesTxt = "Staff Details" + (objctST.StaffID > 0 ? " updated for " : "  added for ") + objctST.FirstNameTxt+" "+ objctST.LastNameTxt;
                    objSCL.LogDateTime = DateTime.Now;
                    objContext.SystemChangeLogs.Add(objSCL);
                    objContext.SaveChanges();

                    objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                    var newResult = (from x in objContext.BoardOfMembers
                                     where x.BoardMemberID == newID
                                     select x);
                    DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                    foreach (DataColumn col in dtNew.Columns)
                    {
                        if (model.StaffID > 0)
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

                    TempData["Message"] = "Staff details " + ((objctST.StaffID == 0) ? "saved" : "updated") + " successfully.";
                }
                catch (Exception ex)
                {
                    TempData["Message"] = "Some error occured. Please try after some time.";
                }
            }
            return RedirectToAction("Index", "Staff", rvd);
        }

        /// <summary>
        /// Set the status of the User as deleted. Donot delete from the table.
        /// </summary>
        /// <param name="stid"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Delete(string stid)
        {
            stid = !string.IsNullOrEmpty(Convert.ToString(stid)) ? EncryptDecrypt.Decrypt(stid) : "0";
            long STID = Convert.ToInt32(stid);
            var rvd = new RouteValueDictionary();
            rvd.Add("pagesize", Request.QueryString["pagesize"] ?? "10");
            rvd.Add("Column", Request.QueryString["Column"] ?? "StaffCreateDate");
            rvd.Add("Direction", Request.QueryString["Direction"] ?? "Descending");
            Session["Edit/Delete"] = "Delete";
            if (STID > 0)
            {
                try
                {
                    using (var objContext = new db_KISDEntities())
                    {
                        #region System Change Log
                        var oldresult = (from a in objContext.BoardOfMembers
                                         where a.BoardMemberID == STID
                                         select a).ToList();
                        DataTable dtOld = Models.Common.LINQResultToDataTable(oldresult);
                        #endregion

                        var obj = objContext.Staffs.Where(x => x.StaffID == STID).ToList();
                        foreach (var value in obj)
                        {                      

                            //****************Display Order ************************
                            var objData = objContext.Staffs.Where(x => x.StaffID == STID).FirstOrDefault();
                            if (objData != null)
                            {
                                try
                                {
                                    var objModelService = new StaffModelService();
                                    objModelService.ChangeDeletedDisplayOrder(objData.DisplayOrderNbr.Value, objData.StaffID);
                                }
                                catch { }
                            }
                            //***************************************************

                            #region System Change Log
                            SystemChangeLog objSCL = new SystemChangeLog();
                            long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                            User objuser = objContext.Users.Where(x => x.UserID == userid).FirstOrDefault();
                            objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.UsernameTxt = objuser.UserNameTxt;
                            objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                            objSCL.ModuleTxt = "Staff";
                            objSCL.LogTypeTxt = "Delete";
                            objSCL.NotesTxt = "Staff Details deleted for " + value.FirstNameTxt+" "+value.LastNameTxt;
                            objSCL.LogDateTime = DateTime.Now;
                            objContext.SystemChangeLogs.Add(objSCL);
                            objContext.SaveChanges();
                            objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                            var newResult = (from x in objContext.RightSections
                                             where x.RightSectionID == STID
                                             select x);
                            DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
                            foreach (DataColumn col in dtNew.Columns)
                            {
                                try
                                {
                                    SystemChangeLogDetail objSCLD = new SystemChangeLogDetail();
                                    objSCLD.ChangeLogID = objSCL.ChangeLogID;
                                    objSCLD.FieldNameTxt = col.ColumnName.ToString();
                                    objSCLD.OldValueTxt = dtOld.Rows[0][col.ColumnName].ToString();
                                    objSCLD.NewValueTxt = col.ColumnName == "IsDeletedInd" ? dtNew.Rows[0][col.ColumnName].ToString() : "";
                                    objContext.SystemChangeLogDetails.Add(objSCLD);
                                    objContext.SaveChanges();
                                }
                                catch { }
                            }
                            #endregion
                        }
                        TempData["Message"] = "Staff details deleted successfully.";
                    }
                }
                catch (Exception e)
                {
                    TempData["Message"] = "Some error occured while deleting the Member, Please try again later.";
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
            return Json(Url.Action("Index", "Staff", rvd));
        }

        public ActionResult StaffListing(string did, int? Page, int? PageSize, GridSortOptions gridSortOptions, string ObjResult)
        {
            ViewBag.Title = "Department Staff Listing";
            did = !string.IsNullOrEmpty(Convert.ToString(did)) ? EncryptDecrypt.Decrypt(did) : "0";
            var deptid = Convert.ToInt64(did);
            var objContext = new db_KISDEntities();
            var currentLoggedUserId = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
            var CurrentUserRoleID = objContext.UserRoles.Where(x => x.UserID == currentLoggedUserId).FirstOrDefault().RoleID;

            #region Check Tab is Accessible or Not
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.ParentStudents), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin) || RoleID == Convert.ToInt32(UserType.DepartmentUser)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

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

                    //Ajax Call for update status for images
                    if (objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && ObjResult != null && !string.IsNullOrEmpty(ObjResult))
                    {
                        var ListingID = Convert.ToInt64(objAjaxRequest.hfid);
                        var Listing = objContext.Staffs.Find(ListingID);
                        if (Listing != null)
                        {
                            #region System Change Log
                            var oldresult = (from a in objContext.Staffs
                                             where a.StaffID == ListingID
                                             select a).ToList();

                            DataTable dtOld = Models.Common.LINQResultToDataTable(oldresult);
                            #endregion

                            if (objAjaxRequest.qs_Type == "status")
                            {
                                Listing.StatusInd = objAjaxRequest.hfvalue == "1";
                                TempData["Message"] = "Status updated successfully.";
                            }
                            else if (objAjaxRequest.qs_Type == "displayorder")
                            {
                                try
                                {
                                    if (StaffModelService.ChangeDisplayOrder(Convert.ToInt64(Listing.DisplayOrderNbr), Convert.ToInt64(objAjaxRequest.qs_value)))
                                    {
                                        TempData["Message"] = "Display Order has been changed successfully.";
                                    }
                                }
                                catch
                                {
                                    TempData["Message"] = "Some Error Occured while changing Display Order, Please try again later.";
                                }
                            }
                            objContext.SaveChanges();

                            #region System Change Log
                            SystemChangeLog objSCL = new SystemChangeLog();
                            User objuser = objContext.Users.Where(x => x.UserID == currentLoggedUserId).FirstOrDefault();
                            objSCL.NameTxt = objuser.FirstNameTxt + " " + objuser.LastNameTxt;
                            objSCL.UsernameTxt = objuser.UserNameTxt;
                            objSCL.UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == objuser.UserID).First().RoleID;
                            objSCL.ModuleTxt = "Staff Listing";
                            objSCL.LogTypeTxt = "Update";
                            objSCL.NotesTxt = (objAjaxRequest.qs_Type == "status" ? "Status " : "Display order ") + "updated for " + Listing.FirstNameTxt+" "+ Listing.LastNameTxt;
                            objSCL.LogDateTime = DateTime.Now;
                            objContext.SystemChangeLogs.Add(objSCL);
                            objContext.SaveChanges();

                            objSCL = objContext.SystemChangeLogs.OrderByDescending(x => x.ChangeLogID).FirstOrDefault();
                            var newResult = (from x in objContext.Staffs
                                             where x.StaffID == Listing.StaffID
                                             select x);

                            DataTable dtNew = Models.Common.LINQResultToDataTable(newResult);
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
                }
                ObjResult = string.Empty;
            }
            #endregion Ajax Call

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
                gridSortOptions.Column = "StaffCreateDate";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            if (gridSortOptions.Column == "NameTxt" || gridSortOptions.Column == "TitleTxt"
                || gridSortOptions.Column == "DisplayOrderNbr" || gridSortOptions.Column == "DesignationTxt"
                )
            {

            }
            else
            {
                gridSortOptions.Column = "StaffCreateDate";
            }

            var pagesize = PageSize.HasValue ? PageSize.Value : Models.Common._pageSize;
            var page = Page.HasValue ? Page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;
            var lststaffid = objContext.DepartmentStaffs.Where(x => x.DepartmentID == deptid).Select(x=>x.StaffID).ToList();
            var pagedViewModel = new PagedViewModel<StaffModel>
            {
                ViewData = ViewData,
                Query = _service.GetStaff().Where(x=>lststaffid.Contains(x.StaffID) && x.IsDeletedInd == false).AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "StaffCreateDate",
                Page = page,
                PageSize = pagesize,
            }.Setup();

            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("StaffPartial", pagedViewModel);
            }
            else
            {
                return View(pagedViewModel);
            }
        }

        public ActionResult ViewStaffDetails(long StaffID)
        {
            var objModel = new StaffModel();
            if (StaffID > 0)
            {
                var objBOMContext = objContext.Staffs.Find(StaffID);
                if (objBOMContext != null)
                {
                    objModel.StaffID = objBOMContext.StaffID;
                    objModel.StatusInd = objBOMContext.StatusInd.Value;
                    objModel.StaffCreateDate = objBOMContext.StaffCreateDate.HasValue ? objBOMContext.StaffCreateDate.Value : DateTime.Now;
                    objModel.PhoneTxt = objBOMContext.PhoneTxt;
                    objModel.CreateByID = objBOMContext.CreateByID;
                    objModel.CreateDate = objBOMContext.CreateDate.HasValue ? objBOMContext.CreateDate.Value : DateTime.Now;
                    objModel.EmailTxt = objBOMContext.EmailTxt;
                    objModel.DisplayOrderNbr = objBOMContext.DisplayOrderNbr;
                    objModel.DesignationTxt = objBOMContext.DesignationTxt;
                    objModel.IsDeletedInd = objBOMContext.IsDeletedInd;
                    objModel.LastModifyByID = objBOMContext.LastModifyByID;
                    objModel.LastModifyDate = objBOMContext.LastModifyDate.HasValue ? objBOMContext.LastModifyDate.Value : DateTime.Now;
                    objModel.FirstNameTxt = objBOMContext.FirstNameTxt;
                    objModel.LastNameTxt = objBOMContext.LastNameTxt;
                    objModel.strStaffCreateDate = objBOMContext.StaffCreateDate.HasValue ? objBOMContext.StaffCreateDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString();
                    objModel.DepartmentsName = _service.GetDepartmentsName(StaffID, true, false);
                    objModel.SubDepartmentsName = _service.GetDepartmentsName(StaffID, false, true);
                    objModel.SchoolCategoryName = _service.GetSchoolData(objBOMContext.SchoolCategoryID.Value);
                    objModel.SchoolName = _service.GetSchoolData(objBOMContext.SchoolID.Value);
                }
            }

            return View("ViewStaffDetails", objModel);
        }

        #region Private Methods
        private List<SelectListItem> GetDepartments()
        {
            List<SelectListItem> lst = new List<SelectListItem>();
            var values = objContext.Departments.Where(x => x.IsDeletedInd == false && x.StatusInd == true && x.ParentID == null).ToList();
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

        private List<SelectListItem> GetSubDepartments(long deptID)
        {
            List<SelectListItem> lst = new List<SelectListItem>();
            var values = objContext.Departments.Where(x => x.IsDeletedInd == false && x.StatusInd == true && x.ParentID == deptID).ToList();
            if (values != null)
            {
                foreach (var item in values)
                {
                    SelectListItem sli = new SelectListItem();
                    sli.Value = item.DepartmentID.ToString();
                    sli.Text = item.NameTxt;
                    lst.Add(sli);
                }
            }

            SelectListItem objsli = new SelectListItem();
            objsli.Text = "--- Select Sub Departments ---";
            objsli.Value = "0";
            objsli.Selected = true;
            lst.Insert(0, objsli);
            return lst;
        }

        private List<SelectListItem> GetSchoolCategories(long value)
        {
            List<SelectListItem> lst = new List<SelectListItem>();
            var values = objContext.Schools.Where(x => x.IsDeletedInd == false && x.StatusInd == true && x.SchoolCategoryID == null).ToList();
            foreach (var item in values)
            {
                SelectListItem sli = new SelectListItem();
                sli.Value = item.SchoolID.ToString();
                sli.Text = item.NameTxt;
                sli.Selected = value == item.SchoolID ? true : false;
                lst.Add(sli);
            }

            SelectListItem objsli = new SelectListItem();
            objsli.Text = "--- Select School Category ---";
            objsli.Value = "0";
            objsli.Selected = value == 0 ? true : false;
            lst.Insert(0, objsli);
            return lst;
        }

        private List<SelectListItem> GetSchools(long value)
        {
            List<SelectListItem> lst = new List<SelectListItem>();
            var values = objContext.Schools.Where(x => x.IsDeletedInd == false && x.StatusInd == true && x.SchoolCategoryID != null).ToList();
            if (value != 0)
            {
                foreach (var item in values)
                {
                    SelectListItem sli = new SelectListItem();
                    sli.Value = item.SchoolID.ToString();
                    sli.Text = item.NameTxt;
                    sli.Selected = value == item.SchoolID ? true : false;
                    lst.Add(sli);
                }
            }

            SelectListItem objsli = new SelectListItem();
            objsli.Text = "--- Select School ---";
            objsli.Value = "0";
            objsli.Selected = value == 0 ? true : false;
            lst.Insert(0, objsli);
            return lst;
        }

        [HttpPost]
        public JsonResult GetSubDepartments(string departmentID)
        {
            List<SelectListItem> lst = new List<SelectListItem>();
            string[] splitted = departmentID.Split(',');
            if (splitted != null)
            {
                foreach (var value in splitted)
                {
                    long s = Convert.ToInt64(value);
                    var values = objContext.Departments.Where(x => x.IsDeletedInd == false && x.StatusInd == true && x.ParentID == s).ToList();
                    if (values != null)
                    {
                        foreach (var item in values)
                        {
                            SelectListItem sli = new SelectListItem();
                            sli.Value = item.DepartmentID.ToString();
                            sli.Text = item.NameTxt;
                            sli.Selected = departmentID == Convert.ToString(item.DepartmentID) ? true : false;
                            lst.Add(sli);
                        }
                    }
                }
            }

            SelectListItem objsli = new SelectListItem();
            objsli.Text = "--- Select Sub Departments ---";
            objsli.Value = "0";
            objsli.Selected = departmentID == "0" ? true : false;
            lst.Insert(0, objsli);
            return Json(lst);
        }

        [HttpPost]
        public JsonResult GetSchool(string schoolCatID)
        {
            List<SelectListItem> lst = new List<SelectListItem>();
            long s = Convert.ToInt64(schoolCatID);
            var values = objContext.Schools.Where(x => x.IsDeletedInd == false && x.StatusInd == true && x.SchoolCategoryID == s).ToList();
            if (values != null)
            {
                foreach (var item in values)
                {
                    SelectListItem sli = new SelectListItem();
                    sli.Value = item.SchoolID.ToString();
                    sli.Text = item.NameTxt;
                    sli.Selected = schoolCatID == Convert.ToString(item.SchoolID) ? true : false;
                    lst.Add(sli);
                }
            }

            SelectListItem objsli = new SelectListItem();
            objsli.Text = "--- Select School ---";
            objsli.Value = "0";
            objsli.Selected = schoolCatID == "0" ? true : false;
            lst.Insert(0, objsli);
            return Json(lst);
        }

        public void ManageStaffDetails(long StaffID, long DeptId, bool IsParentDept)
        {
            if (DeptId != 0)
            {
                var obj = new DepartmentStaff();
                obj.StaffID = StaffID;
                obj.DepartmentID = DeptId;
                obj.CreateDate = DateTime.Now;
                obj.IsParentDepartment = IsParentDept;
                obj.CreateByID = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                objContext.DepartmentStaffs.Add(obj);
                objContext.SaveChanges();
            }
        }
        #endregion
    }
}