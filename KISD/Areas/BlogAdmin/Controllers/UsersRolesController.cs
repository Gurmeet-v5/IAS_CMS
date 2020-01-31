using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using KISD.Areas.BlogAdmin.Models;

namespace KISD.Areas.BlogAdmin.Controllers
{
    public class UsersRolesController : Controller
    {
        //
        // GET: /Blog/UsersRoles/
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
        public ActionResult BlogUsersRolesListing(GridSortOptions gridSortOptions, int? pagetype, int? page, int? pagesize, FormCollection fm, string objresult, int ? UserID)
        {
            var _objContext = new Contexts.UsersRolesContext();
            var _objUsersContexts = new Contexts.UsersContexts();
            var pageSize = pagesize.HasValue ? pagesize.Value : Models.Common._pageSize;
            var Page = page.HasValue ? page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;
            ViewBag.Title = " User Role Listing";
            var _userroleModel = new UserRoleModel();
            #region Ajax Call
            if (objresult != null)
            {
                AjaxRequest objAjaxRequest = JsonConvert.DeserializeObject<AjaxRequest>(objresult);//Convert json String to object Model
                if (objAjaxRequest.ajaxcall != null && !string.IsNullOrEmpty(objAjaxRequest.ajaxcall) && objresult != null && !string.IsNullOrEmpty(objresult))
                {
                    if (objAjaxRequest.ajaxcall == "paging")//Ajax Call type = paging i.e. Next|Previous|Back|Last
                    {
                        Session["pageNo"] = Page;// stores the page no for status
                    }
                    else if (objAjaxRequest.ajaxcall == "sorting")//Ajax Call type = sorting i.e. column sorting Asc or Desc
                    {
                        Page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : Page);
                        Session["GridSortOption"] = gridSortOptions;
                        pagesize = (Session["PageSize"] != null ? Convert.ToInt32(Session["PageSize"].ToString()) : pagesize);
                    }
                    else if (objAjaxRequest.ajaxcall == "ddlPaging")//Ajax Call type = drop down paging i.e. drop down value 10, 25, 50, 100, ALL
                    {
                        Session["PageSize"] = (Request.QueryString["pagesize"] != null ? Convert.ToInt32(Request.QueryString["pagesize"].ToString()) : pagesize);
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
                        page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"].ToString()) : page);
                        gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                    }
                    objAjaxRequest.ajaxcall = null; ;//remove parameter value
                }
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
            if (gridSortOptions.Column != null && gridSortOptions.Column == "UserName" || gridSortOptions.Column == "RoleID")
            {
            }
            else
            {
                gridSortOptions.Column = "UserName";
            }
            var userid = UserID != null ? UserID : 0;
            var pagedViewModel = new PagedViewModel<UserRoleModel>
            {
                ViewData = ViewData,
                Query = _objContext.GetUsersRoles_().AsQueryable().Where(x=> x.UserID == userid),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "UserName",
                Page = Page,
                PageSize = pageSize,
            }.Setup();

            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("BlogUserRoleListingPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }
        [Authorize]
        [SessionExpire]
        public ActionResult CreateUserRole(int? UserID)
        {
            Session["Edit/Delete"] = "Edit";
            var _userroleContext = new Contexts.UsersRolesContext();
            var _userContext = new Contexts.UsersContexts();
            var _roleContext = new Contexts.RolesContexts();
            var _userroleModel = new UserRoleModel();
            ViewBag.Title = (UserID.HasValue ? "Edit " : "Add ") + " User Role Details ";
            ViewBag.Submit = UserID.HasValue && UserID.Value > 0 ? "Update" : "Save";
            ViewBag.Role = new SelectList(_roleContext.GetRoles().ToList(), "RoleID", "RoleName");
            if (UserID.HasValue && UserID.Value > 0)
            {
                if (_userroleModel != null)
                {
                    _userroleModel = _userroleContext.GetUsersRoles().Where(x => x.UserID == UserID).FirstOrDefault();
                    _userroleModel.UserName = _userContext.GetUsers().Where(x => x.UserID == _userroleModel.UserID).Select(x=> x.UserNameTxt).FirstOrDefault();
                }
            }
            return View(_userroleModel);
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
        public ActionResult CreateUserRole(UserRoleModel _userrolemodel, string command, FormCollection fm)
        {
            Session["Edit/Delete"] = "Edit";
            ViewBag.Title = (_userrolemodel.UserRoleID > 0 ? "Edit " : "Add ") + " User Role Details ";
            ViewBag.Submit = _userrolemodel.UserRoleID > 0 ? "Update" : "Save";
            var userroleContext = new Contexts.UsersRolesContext();
            var _roleContext = new Contexts.RolesContexts();
            ViewBag.Role = new SelectList(_roleContext.GetRoles().ToList(), "RoleID", "RoleName");
            if (string.IsNullOrEmpty(command))
            {
                try
                {
                    if (ViewBag.Submit == "Save")
                    {
                        userroleContext.AssignRoleToUser(_userrolemodel);
                        TempData["AlertMessage"] = "User Role details saved successfully.";
                    }
                    else
                    {
                        userroleContext.EditUserRole(_userrolemodel);
                        TempData["AlertMessage"] = "User Role details updated successfully.";
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
            rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
            rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
            return RedirectToAction("BlogUserListing", "User", rvd);
        }
    }
}
