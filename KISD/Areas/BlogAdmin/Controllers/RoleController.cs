﻿using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using KISD.Areas.BlogAdmin.Models;

namespace KISD.Areas.BlogAdmin.Controllers
{
    public class RoleController : Controller
    {
        /// <summary>
        /// Method to get display the the Added Rentals in Grid.
        /// Code to update the status and show on home property by clicking the checkboxes added for the status and show on home.
        /// </summary>
        /// <param name="gridSortOptions">It will pass sorting parameters</param>
        /// <param name="page">Defines the page on which Role is currently viewing records</param>
        /// <param name="pagesize">Number of records to be displayed on he page.</param>
        /// <param name="type">Rentals or Featured Homes for Sale</param>
        /// <param name="fm">Form hidden field values</param>
        /// <returns></returns>
        [Authorize]
        [SessionExpire]
        public ActionResult BlogRoleListing(GridSortOptions gridSortOptions, int? pagetype, int? page, int? pagesize, FormCollection fm, string objresult)
        {
            var _objContext = new Contexts.RolesContexts();
            var pageSize = pagesize.HasValue ? pagesize.Value : Models.Common._pageSize;
            var Page = page.HasValue ? page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;
            ViewBag.Title = " Role Listing";
            var _RoleModel = new RoleModel();
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
                gridSortOptions.Column = "RoleId";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            if (gridSortOptions.Column != null && gridSortOptions.Column == "RoleName" || gridSortOptions.Column == "RoleDescription")
            {
            }
            else
            {
                gridSortOptions.Column = "EmailTxt";
            }

            var pagedViewModel = new PagedViewModel<RoleModel>
            {
                ViewData = ViewData,
                Query = _objContext.GetRoles().AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "RoleId",
                Page = Page,
                PageSize = pageSize,
            }.Setup();

            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("BlogRoleListingPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }

        [Authorize]
        [SessionExpire]
        public ActionResult CreateRole(int? Roleid)
        {
            var _RoleContext = new Contexts.RolesContexts();
            var _RoleModel = new RoleModel();
            ViewBag.Title = (Roleid.HasValue ? "Edit " : "Add ") + " Role Details ";
            ViewBag.Submit = Roleid.HasValue && Roleid.Value > 0 ? "Update" : "Save";
            if (Roleid.HasValue && Roleid.Value > 0)
            {
                if (_RoleModel != null)
                {
                    _RoleModel = _RoleContext.GetRoles().Where(x => x.RoleId == Roleid).FirstOrDefault();
                }
            }
            return View(_RoleModel);
        }

        /// <summary>
        /// Method to save/ update the added role into the database.
        /// </summary>
        /// <param name="_Rolemodel">RoleModel that will contain the values saved in the Role View</param>
        /// <param name="command">This will contain value on click of Cancel button</param>
        /// <param name="fm">It contains the form collection (hidden field etc) values.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [SessionExpire]
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateRole(RoleModel _Rolemodel, string command, FormCollection fm)
        {
            ViewBag.Title = (_Rolemodel.RoleId > 0 ? "Edit " : "Add ") + " Role Details ";
            ViewBag.Submit = _Rolemodel.RoleId > 0 ? "Update" : "Save";
            var RoleContext = new Contexts.RolesContexts();
            if (string.IsNullOrEmpty(command))
            {
                if (RoleContext.GetRoles().Where(x => x.RoleName == _Rolemodel.RoleName && x.RoleId != _Rolemodel.RoleId).Any())
                {
                    ModelState.AddModelError("RoleName", _Rolemodel.RoleName + " Role already exists.");
                    return View(_Rolemodel);
                }
                try
                {
                    if (ViewBag.Submit == "Save")
                    {
                        _Rolemodel.LoweredRoleNameTxt = _Rolemodel.RoleName.ToLower();
                        RoleContext.AddRole(_Rolemodel);
                        TempData["AlertMessage"] = "Role details saved successfully.";
                    }
                    else
                    {
                        _Rolemodel.LoweredRoleNameTxt = _Rolemodel.LoweredRoleNameTxt.ToLower();
                        RoleContext.EditRole(_Rolemodel);
                        TempData["AlertMessage"] = "Role details Updated successfully.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["AlertMessage"] = "Some error occured, Please try after some time. " + ex.Message.ToString();
                }
            }
            var rvd = new RouteValueDictionary();
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "FirstName");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Ascending");
            rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
            rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
            return RedirectToAction("BlogRoleListing", "Role", rvd);
        }

        /// <summary>
        /// Method to delete the role based on property id.
        /// </summary>
        /// <param name="RoleId"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [SessionExpire]
        public JsonResult Delete(int? RoleId)
        {
            var RoleContext = new Contexts.RolesContexts();
            if (RoleId.HasValue)
            {
                try
                {
                    if (RoleContext != null)
                    {
                        RoleContext.DeleteRoles(RoleId);
                        TempData["AlertMessage"] = "Role details deleted successfully.";
                    }
                }
                catch
                {
                    TempData["AlertMessage"] = "Some error occured while deleting the Role, Please try again later.";
                }
            }
            var rvd = new RouteValueDictionary();
            int? Page = 1;
            var count = 1;
            count = RoleContext.GetRoles().Count();
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
            return Json(Url.Action("BlogRoleListing", "Role", rvd));
        }
    }
}

