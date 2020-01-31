using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using KISD.Areas.BlogAdmin.Models;

namespace KISD.Areas.BlogAdmin.Controllers
{
    public class CategoriesController : Controller
    {
        //
        // GET: /Blog/Categories/
        /// <summary>
        /// Method to get display the the Added Categories in Grid.
        /// Code to update the status and show on home property by clicking the checkboxes added for the status and show on home.
        /// </summary>
        /// <param name="gridSortOptions">It will pass sorting parameters</param>
        /// <param name="page">Defines the page on which Categories is currently viewing records</param>
        /// <param name="pagesize">Number of records to be displayed on he page.</param>
        /// <param name="fm">Form hidden field values</param>
        /// <returns></returns>
        [Authorize]
        [SessionExpire]
        public ActionResult BlogCategoriesListing(GridSortOptions gridSortOptions, int? pagetype, int? page, int? pagesize, FormCollection fm, string objresult)
        {
            var _objContext = new Contexts.CategoriesContexts();
            ViewBag.Title = " Categories Listing";
            var _CategoriesModel = new CategoriesModel();
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
            }
            #endregion Ajax Call
            //This section is used to retain the values of page , pagesize and gridsortoption on complete page post back(Edit, Dlete)
            //Pass value for Session["Edit/Delete"] from Edit and delete action on postback
            if (!Request.IsAjaxRequest() && Session["Edit/Delete"] != null && !string.IsNullOrEmpty(Session["Edit/Delete"].ToString()))
            {
                pagesize = (Session["PageSize"] != null ? Convert.ToInt32(Session["PageSize"]) : Models.Common._pageSize);
                page = (Session["pageNo"] != null ? Convert.ToInt32(Session["pageNo"]) : Models.Common._currentPage);
                gridSortOptions = (Session["GridSortOption"] != null ? Session["GridSortOption"] as GridSortOptions : gridSortOptions);
                Session["Edit/Delete"] = null;
            }
            else if (!Request.IsAjaxRequest() && Session["Edit/Delete"] == null)
            {
                //gridSortOptions.Column = "CreateDate";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }
            var PageSize = pagesize.HasValue ? pagesize.Value : Models.Common._pageSize;
            var Page = page.HasValue ? page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;
            if (gridSortOptions.Column != null && gridSortOptions.Column == "CategoryNameTxt")
            {
            }
            else
            {
                gridSortOptions.Column = "CategoryNameTxt";
            }

            var pagedViewModel = new PagedViewModel<CategoriesModel>
            {
                ViewData = ViewData,
                Query = _objContext.GetCategories().AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "CategoryNameTxt",
                Page = Page,
                PageSize = PageSize,
            }.Setup();

            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("BlogCategoriesListingPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }

        [Authorize]
        [SessionExpire]
        public ActionResult CreateCategories(int? Categoriesid)
        {
            Session["Edit/Delete"] = "Edit";
            var _CategoriesContext = new Contexts.CategoriesContexts();
            var _CategoriesModel = new CategoriesModel();
            ViewBag.Title = (Categoriesid.HasValue && Categoriesid.Value > 0 ? "Edit " : "Add ") + " Category";
            ViewBag.Submit = Categoriesid.HasValue && Categoriesid.Value > 0 ? "Update" : "Save";
            if (Categoriesid.HasValue && Categoriesid.Value > 0)
            {
                if (_CategoriesModel != null)
                {
                    _CategoriesModel = _CategoriesContext.GetCategories().Where(x => x.CategoryID == Categoriesid).FirstOrDefault();
                }
            }
            return View(_CategoriesModel);
        }

        /// <summary>
        /// Method to save/ update the added Categories into the database.
        /// </summary>
        /// <param name="_Categoriesmodel">CategoriesModel that will contain the values saved in the Categories View</param>
        /// <param name="command">This will contain value on click of Cancel button</param>
        /// <param name="fm">It contains the form collection (hidden field etc) values.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [SessionExpire]
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateCategories(CategoriesModel _Categoriesmodel, string command, FormCollection fm)
        {
            Session["Edit/Delete"] = "Edit";
            ViewBag.Title = (_Categoriesmodel.CategoryID > 0 ? "Edit " : "Add ") + " Category ";
            ViewBag.Submit = _Categoriesmodel.CategoryID > 0 ? "Update" : "Save";
            var CategoriesContext = new Contexts.CategoriesContexts();
            if (string.IsNullOrEmpty(command))
            {
                if (CategoriesContext.GetCategories().Where(x => x.CategoryNameTxt.ToLower().Trim() == _Categoriesmodel.CategoryNameTxt.ToLower().Trim() && _Categoriesmodel.CategoryID != x.CategoryID).Any())
                {
                    ModelState.AddModelError("CategoryNameTxt", _Categoriesmodel.CategoryNameTxt + " category already exists.");
                    return View(_Categoriesmodel);
                }
                try
                {
                    _Categoriesmodel.CategoryNameTxt = _Categoriesmodel.CategoryNameTxt.Trim();
                    if (ViewBag.Submit == "Save")
                    {
                        CategoriesContext.AddCategories(_Categoriesmodel);
                        TempData["AlertMessage"] = "Category saved successfully.";
                    }
                    else
                    {
                        CategoriesContext.EditCategories(_Categoriesmodel);
                        TempData["AlertMessage"] = "Category updated successfully.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["AlertMessage"] = "Some error occured, Please try after some time. " + ex.Message.ToString();
                }
            }
            var rvd = new RouteValueDictionary();
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "CategoryNameTxt");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Ascending");
            rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
            rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
            return RedirectToAction("BlogCategoriesListing", "Categories", rvd);
        }

        /// <summary>
        /// Method to delete the Categories based on property id.
        /// </summary>
        /// <param name="pId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [SessionExpire]
        public JsonResult Delete(int? CategoriesId)
        {
            Session["Edit/Delete"] = "Delete";
            var rvd = new RouteValueDictionary();
            int? Page = 1;
            var page = Request.QueryString["page"] ?? Models.Common._currentPage.ToString();
            var pagesize = Request.QueryString["pagesize"] ?? Models.Common._pageSize.ToString();
            rvd.Add("page", Page);
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "CategoryNameTxt");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Ascending");
            rvd.Add("pagesize", pagesize);
            var CategoriesContext = new Contexts.CategoriesContexts();
            var BlogsContexts = new Contexts.BlogsContexts();
            if (CategoriesId.HasValue)
            {
                try
                {
                    if (CategoriesContext != null)
                    {
                        var counts = BlogsContexts.GetBlogs().Where(x => x.CategoryID == CategoriesId).Select(x => x.CategoryID).Count();
                        if (counts > 0)
                        {
                            TempData["Message"] = "Category can not be deleted as it contains blog details.";
                            return Json(Url.Action("BlogCategoriesListing", "Categories", rvd));
                        }
                        else
                        {
                            CategoriesContext.DeleteCategories(CategoriesId);
                            TempData["AlertMessage"] = "Category deleted successfully.";
                        }
                    }
                }
                catch
                {
                    TempData["AlertMessage"] = "Some error occured while deleting the Category, Please try again later.";
                }
            }

            var count = 1;
            count = CategoriesContext.GetCategories().Count();

            if (Convert.ToInt32(page) > 1)
            {
                Page = count > ((Convert.ToInt32(page) - 1) * Convert.ToInt32(pagesize)) ? Convert.ToInt32(page) : (Convert.ToInt32(page)) - 1;
            }

            return Json(Url.Action("BlogCategoriesListing", "Categories", rvd));
        }
    }
}
