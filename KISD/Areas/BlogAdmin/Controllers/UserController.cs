using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using KISD.Areas.BlogAdmin.Models;

namespace KISD.Areas.BlogAdmin.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /Blog/User/
        /// <summary>
        /// Method to get display the the Added Rentals in Grid.
        /// Code to update the status and show on home property by clicking the checkboxes added for the status and show on home.
        /// </summary>
        /// <param name="gridSortOptions">It will pass sorting parameters</param>
        /// <param name="page">Defines the page on which user is currently viewing records</param>
        /// <param name="pagesize">Number of records to be displayed on he page.</param>
        /// <param name="type">Rentals or Featured Homes for Sale</param>
        /// <param name="fm">Form hidden field values</param>
        /// <returns></returns>
        [Authorize]
        [SessionExpire]
        public ActionResult BlogUserListing(GridSortOptions gridSortOptions, int? pagetype, int? page, int? pagesize, FormCollection fm, string objresult)
        {
            var _objContext = new Contexts.UsersContexts();
            ViewBag.Title = " User Listing";
            var _userModel = new UsersModel();
            var isAdminChangedhisOwnStatus = 0;
            #region Ajax Call
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
                    else if (objAjaxRequest.ajaxcall == "displayorder")//Ajax Call type = Display Order i.e. drop down values
                    {
                        page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                    objAjaxRequest.ajaxcall = null; ;//remove parameter value
                }

                //Ajax CAll for update status for images
                if (objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && objresult != null && !string.IsNullOrEmpty(objresult) && objAjaxRequest.hfvalue.ToString().Trim().ToLower() != "displayOrder".Trim().ToLower())
                {
                    var id1 = Convert.ToInt64(objAjaxRequest.hfid);
                    var user = _objContext.GetUsers().Where(x => x.UserID == id1).FirstOrDefault();
                    if (user != null)
                    {
                        //CategoryID = Convert.ToInt32(Request.QueryString["CategoryID"]);
                        user.StatusInd = objAjaxRequest.hfvalue == "1";
                        try
                        {
                            _objContext.EditUser(user);
                            var loggedIn_user = User.Identity.Name;
                            if (user.UserNameTxt.Trim().ToLower() == loggedIn_user.Trim().ToLower())
                            {
                                isAdminChangedhisOwnStatus = 1;
                            }
                            TempData["AlertMessage"] = "Status updated successfully.";
                        }
                        catch
                        {
                            TempData["AlertMessage"] = "Some error occured, Please try after some time.";
                        }

                        objAjaxRequest.hfid = null;//remove parameter value
                        objAjaxRequest.hfvalue = null;//remove parameter value
                        pagesize = (Request.QueryString["pagesize"] != null ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : pagesize);
                        page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                }
                else
                {
                    TempData["Message"] = string.Empty;
                }
                objresult = string.Empty;

            }
            #endregion Ajax Call
            //This section is used to retain the values of page , pagesize and gridsortoption on complete page post back(Edit, Dlete)
            if (!Request.IsAjaxRequest() && Session["Edit/Delete"] != null && !string.IsNullOrEmpty(Session["Edit/Delete"].ToString()))
            {
                pagesize = (Session["PageSize"] != null ? Convert.ToInt32(Session["PageSize"]) : Models.Common._pageSize);
                page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"]) : Models.Common._currentPage);
                gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                Session["Edit/Delete"] = null;
            }
            else if (!Request.IsAjaxRequest() && Session["Edit/Delete"] == null)
            {
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            if (gridSortOptions.Column != null && gridSortOptions.Column == "FirstName" || gridSortOptions.Column == "UserNameTxt")
            {
            }
            else
            {
                gridSortOptions.Column = "FirstName";
            }
            var pageSize = pagesize.HasValue ? pagesize.Value : Models.Common._pageSize;
            var Page = page.HasValue ? page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;

            var UsersRolesContext = new Contexts.UsersRolesContext().GetUsersRoles();
            var UsersContexts = new Contexts.UsersContexts().GetAccountUsers();
            var userID = UsersRolesContext.Where(x => x.RoleID == 1).Select(x => x.UserID).FirstOrDefault();
            var UserType = UsersContexts.Count() > 0 ? UsersContexts.Where(x => x.UserID == userID).Select(x => x.UserNameTxt).FirstOrDefault().ToString().ToLower().Trim() : "";

            var pagedViewModel = new PagedViewModel<UsersModel>();

            pagedViewModel = new PagedViewModel<UsersModel>
            {
                ViewData = ViewData,
                Query = _objContext.GetUsers().AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "FirstName",
                Page = Page,
                PageSize = pageSize,
            }.Setup();
            if (isAdminChangedhisOwnStatus == 1)
            {
                Session.Abandon();
                FormsAuthentication.SignOut();
            }

            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("BlogUserListingPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }

        [Authorize]
        [SessionExpire]
        public ActionResult CreateUser(int? userid)
        {
            Session["Edit/Delete"] = "Edit";
            var _userContext = new Contexts.UsersContexts();
            var _userModel = new UsersModel();
            ViewBag.Title = (userid.HasValue ? "Edit " : "Add ") + " User ";
            ViewBag.Submit = userid.HasValue && userid.Value > 0 ? "Update" : "Save";
            bool isActive = true;
            if (userid.HasValue && userid.Value > 0)
            {
                if (_userModel != null)
                {
                    _userModel = _userContext.GetUsers().Where(x => x.UserID == userid).FirstOrDefault();
                    isActive = _userModel.StatusInd;
                }
            }
            ViewBag.StatusInd = Models.Common.GetStatusListBoolean(isActive ? "true" : "false");
            return View(_userModel);
        }

        /// <summary>
        /// Method to save/ update the added rentals into the database.
        /// </summary>
        /// <param name="_usermodel">UserModel that will contain the values saved in the User View</param>
        /// <param name="command">This will contain value on click of Cancel button</param>
        /// <param name="fm">It contains the form collection (hidden field etc) values.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [SessionExpire]
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateUser(UsersModel _usermodel, string command, FormCollection fm)
        {
            Session["Edit/Delete"] = "Edit";
            UserRoleModel _userrolemodel = new UserRoleModel();
            ViewBag.Title = (_usermodel.UserID > 0 ? "Edit " : "Add ") + " User ";
            ViewBag.Submit = _usermodel.UserID > 0 ? "Update" : "Save";
            ViewBag.StatusInd = Models.Common.GetStatusListBoolean(_usermodel.StatusInd ? "True" : "False");
            var userContext = new Contexts.UsersContexts();
            var userroleContext = new Contexts.UsersRolesContext();
            _usermodel.UserNameTxt = _usermodel.UserNameTxt.Trim();
            if(!string.IsNullOrEmpty(_usermodel.Password))
            { 
            _usermodel.Password = _usermodel.Password.Trim();
            }
            if (string.IsNullOrEmpty(command))
            {
                if (userContext.GetUsers().Where(x => x.UserNameTxt.ToLower().Trim() == _usermodel.UserNameTxt.ToLower().Trim() && _usermodel.UserID != x.UserID).Any())
                {
                    ModelState.AddModelError("UserNameTxt", _usermodel.UserNameTxt + " username already exists.");
                    return View(_usermodel);
                }
                try
                {
                    if (ViewBag.Submit == "Save")
                    {
                        userContext.AddUser(_usermodel);
                        _userrolemodel.UserID = userContext.GetAccountUsers().Count() > 0 ? userContext.GetAccountUsers().Select(x => x.UserID).Max() + 1 : 2;
                        _userrolemodel.RoleID = 2;// Assign User Role
                        userroleContext.AssignRoleToUser(_userrolemodel);
                        TempData["AlertMessage"] = "User saved successfully.";
                    }
                    else
                    {
                        var loggedIn_user = User.Identity.Name;
                        TempData["AlertMessage"] = "User updated successfully.";
                        var userNm = userContext.GetUsers().Where(x => x.UserID == _usermodel.UserID).Select(x => x.UserNameTxt).FirstOrDefault();
                        var objuser = userContext.GetUsers().Where(x => x.UserID == _usermodel.UserID).FirstOrDefault();
                        if (userNm.Trim().ToLower() == loggedIn_user.Trim().ToLower() && (_usermodel.Password.Trim().ToLower() != objuser.Password.Trim().ToLower() || objuser.UserNameTxt.Trim().ToLower() != _usermodel.UserNameTxt.Trim().ToLower()))
                        {
                            userContext.EditUser(_usermodel);
                            Session.Abandon();
                            FormsAuthentication.SignOut();
                            return RedirectToAction("Login", "Account", new { isChanged = "1" });
                        }
                        userContext.EditUser(_usermodel);
                    }
                }
                catch
                {
                    TempData["AlertMessage"] = "Some error occured, Please try after some time.";
                }
            }
            var rvd = new RouteValueDictionary();
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "FirstName");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Ascending");
            rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Areas.BlogAdmin.Models.Common._pageSize.ToString());
            rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Areas.BlogAdmin.Models.Common._currentPage.ToString());
            return RedirectToAction("BlogUserListing", "User", rvd);
        }

        /// <summary>
        /// Method to delete the user based on property id.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [SessionExpire]
        public JsonResult Delete(int? userId)
        {
            Session["Edit/Delete"] = "Delete";
            var userContext = new Contexts.UsersContexts();
            var UsersRolesContext = new Contexts.UsersRolesContext();
            if (userId.HasValue)
            {
                try
                {
                    if (userContext != null)
                    {
                        var userRoleList = UsersRolesContext.GetUsersRoles().Where(x => x.UserID == userId).FirstOrDefault();
                        var UserRoleID = userRoleList.UserRoleID;
                        var RoleID = userRoleList.RoleID;
                        if (RoleID == 1 || RoleID == 2)
                        {
                            userContext.DeleteUsers(userId);
                            UsersRolesContext.DeleteUsersRole(UserRoleID);
                            TempData["AlertMessage"] = "User deleted successfully.";
                        }
                        else
                        {
                            TempData["AlertMessage"] = "Some error occured while deleting the User, Please try again later.";
                        }
                    }
                }
                catch
                {
                    TempData["AlertMessage"] = "Some error occured while deleting the User, Please try again later.";
                }
            }
            var rvd = new RouteValueDictionary();
            int? Page = 1;
            var count = 1;
            count = userContext.GetUsers().Count();
            var page = Request.QueryString["page"] ?? Models.Common._currentPage.ToString();
            var pagesize = Request.QueryString["pagesize"] ?? Models.Common._pageSize.ToString();
            if (Convert.ToInt32(page) > 1)
            {
                Page = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
            }
            rvd.Add("page", Page);
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "FirstName");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Ascending");
            rvd.Add("pagesize", pagesize);
            return Json(Url.Action("BlogUserListing", "User", rvd));
        }
    }
}
