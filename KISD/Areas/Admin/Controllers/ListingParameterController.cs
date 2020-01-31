using KISD.Areas.Admin.Models;
using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;
using static KISD.Areas.Admin.Models.Common;

namespace KISD.Areas.Admin.Controllers
{
    public class ListingParameterController : Controller
    {
        #region Initialize service

        /// <summary>
        /// Initialize the object for the SAWTST Database. 
        /// </summary>
        db_KISDEntities objContext = new db_KISDEntities();
        private ListingParameterService _service;
        public ListingParameterController()
        {
            _service = new ListingParameterService();
        }

        #endregion
        /// <summary>
        /// Get the Email Leyword List from the database.
        /// PagedViewModel method bind the email list into gridview.
        /// </summary>
        /// <param name="gridSortOptions">Sort the grid basis on the options defined</param>
        /// <param name="page">Current Page</param>        
        /// <param name="pagesize">Pagesize for records</param>
        /// <returns>Email Keyword of PagedViewModel Data.</returns>
        [Authorize]
        [SessionExpire]
        public ActionResult ListingParameters(GridSortOptions gridSortOptions, int? page, int? pagesize, string objresult)
        {
            ListingParameterService _ListingParameterService = new ListingParameterService();

            #region Check Tab is Accessible or Not
            var userId = objContext.Users.Where(x => x.UserNameTxt == User.Identity.Name).Select(x => x.UserID).FirstOrDefault();
            var RoleID = objContext.UserRoles.Where(x => x.UserID == userId).Select(x => x.RoleID).FirstOrDefault();
            var HasTabAccess = GetAccessibleTabAccess(Convert.ToInt32(ModuleType.ListingParameters), Convert.ToInt32(userId));
            if (!(HasTabAccess || RoleID == Convert.ToInt32(UserType.SuperAdmin)
                || RoleID == Convert.ToInt32(UserType.Admin)))//if tab not accessible then redirect to home
            {
                return RedirectToAction("Index", "Home");
            }
            #endregion

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
                    objAjaxRequest.ajaxcall = null; ;//remove parameter value
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
                pagesize = (Session["PageSize"] != null ? Convert.ToInt32(Session["PageSize"]) : (pagesize != null ? pagesize : Areas.Admin.Models.Common._pageSize));
                page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"]) : (page != null ? page : Areas.Admin.Models.Common._currentPage));
                gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                Session["Edit/Delete"] = null;
            }
            else if (!Request.IsAjaxRequest() && Session["Edit/Delete"] == null)
            {
                gridSortOptions.Column = "ListingParameterTxt";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            var pageSize = pagesize.HasValue ? pagesize.Value : Models.Common._pageSize;
            var Page = page.HasValue ? page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;
            ViewBag.Edit = null;//Check for postback or not
            if (page != null && pagesize != null)
            {
                ViewBag.Edit = true;
            }
            if (gridSortOptions.Column != null)
            {
                if (gridSortOptions.Column == "ListingParameterTxt" || gridSortOptions.Column == "DescriptionTxt")
                {

                }
                else
                {
                    gridSortOptions.Column = "ListingParameterTxt";
                }
            }
            ViewBag.Title = ViewBag.PageTitle = "Listing Parameter Listing";
            var pagedViewModel = new PagedViewModel<ListingParameterModel>
            {
                ViewData = ViewData,
                Query = _ListingParameterService.GetListingParameter(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "ListingParameterTxt",
                Page = Page,
                PageSize = pageSize,
            }.Setup();
            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {

                return View("ListingParameterPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }

        }
    }
}