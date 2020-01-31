using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using KISD.Areas.BlogAdmin.Models;

namespace KISD.Areas.BlogAdmin.Controllers
{
    public class CommentController : Controller
    {
        //
        // GET: /BlogAdmin/Comment/

        /// <summary>
        /// Method to get display the the Added Rentals in Grid.
        /// Code to update the status and show on home property by clicking the checkboxes added for the status and show on home.
        /// </summary>
        /// <param name="gridSortOptions">It will pass sorting parameters</param>
        /// <param name="page">Defines the page on which Comment is currently viewing records</param>
        /// <param name="pagesize">Number of records to be displayed on he page.</param>
        /// <param name="type">Rentals or Featured Homes for Sale</param>
        /// <param name="fm">Form hidden field values</param>
        /// <returns></returns>
        [Authorize]
        [SessionExpire]
        public ActionResult CommentListing(int BlogID, GridSortOptions gridSortOptions, int? pagetype, int? page, int? pagesize, FormCollection fm, string objresult)
        {
            var _objContext = new Contexts.CommentContexts();
            ViewBag.Title = " Comment Listing";
            var _CommentModel = new CommentModel();
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
                if (objAjaxRequest.hfid != null && objAjaxRequest.hfvalue != null && !string.IsNullOrEmpty(objAjaxRequest.hfid) && !string.IsNullOrEmpty(objAjaxRequest.hfvalue) && objresult != null && !string.IsNullOrEmpty(objresult) && objAjaxRequest.hfvalue.ToString().Trim().ToLower() != "displayOrder".Trim().ToLower())
                {
                    var id1 = Convert.ToInt64(objAjaxRequest.hfid);
                    var comment = _objContext.GetComments().Where(x => x.CommentID == id1).FirstOrDefault();
                    if (comment != null)
                    {
                        //CategoryID = Convert.ToInt32(Request.QueryString["CategoryID"]);
                        comment.IsActiveInd = objAjaxRequest.hfvalue == "1";
                        try
                        {
                            _objContext.EditComment(comment);
                            TempData["AlertMessage"] = "Comment " + (comment.IsActiveInd == true ? "Approved" : "Disapproved") + " successfully.";
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
                //gridSortOptions.Column = "CreateDate";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }

            var pageSize = pagesize.HasValue ? pagesize.Value : Models.Common._pageSize;
            var Page = page.HasValue ? page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;

            if (gridSortOptions.Column != null && gridSortOptions.Column == "FullNameTxt" || gridSortOptions.Column == "EmailTxt" || gridSortOptions.Column == "PostedDate")
            {
            }
            else
            {
                gridSortOptions.Column = "FullNameTxt";
            }
            var pagedViewModel = new PagedViewModel<CommentModel>
            {
                ViewData = ViewData,
                Query = _objContext.GetComments().Where(x => x.BlogID == BlogID).AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "FullNameTxt",
                Page = Page,
                PageSize = pageSize,
            }.Setup();

            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {

                return View("CommentListingPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }

        [Authorize]
        [SessionExpire]
        public ActionResult CommentDetail(int? Commentid)
        {
            var _CommentContext = new Contexts.CommentContexts();
            var _BlogContext = new Contexts.BlogsContexts();
            var _CommentModel = new CommentModel();
            var Approve = _CommentContext.GetComments().Where(x => x.CommentID == Commentid).Select(x => x.IsActiveInd).FirstOrDefault();
            ViewBag.Title = "View Comment Details ";
            ViewBag.Submit = Approve == true ? "Disapprove" : "Approve";
            if (Commentid.HasValue && Commentid.Value > 0)
            {
                if (_CommentModel != null)
                {
                    _CommentModel = _CommentContext.GetComments().Where(x => x.CommentID == Commentid).FirstOrDefault();
                    _CommentModel.BlogTitle = _BlogContext.GetBlogs().Where(x => x.BlogID == _CommentModel.BlogID).Select(x => x.TitleTxt).FirstOrDefault();
                }
            }
            return View(_CommentModel);
        }

        /// <summary>
        /// Method to save/ update the added rentals into the database.
        /// </summary>
        /// <param name="_Commentmodel">CommentModel that will contain the values saved in the Comment View</param>
        /// <param name="command">This will contain value on click of Cancel button</param>
        /// <param name="fm">It contains the form collection (hidden field etc) values.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [SessionExpire]
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CommentDetail(CommentModel _Commentmodel, string command, FormCollection fm)
        {
            Session["Edit/Delete"] = "Edit";
            ViewBag.Title = "View Comment Details ";

            var CommentContext = new Contexts.CommentContexts();
            if (string.IsNullOrEmpty(command))
            {
                try
                {
                    _Commentmodel = CommentContext.GetComments().Where(x => x.CommentID == _Commentmodel.CommentID).FirstOrDefault();
                    ViewBag.Submit = _Commentmodel.IsActiveInd == true ? "Disapprove" : "Approve";
                    _Commentmodel.IsActiveInd = _Commentmodel.IsActiveInd == true ? false : true;
                    CommentContext.EditComment(_Commentmodel);
                    TempData["AlertMessage"] = "Comment " + (_Commentmodel.IsActiveInd != true ? "Disapproved" : "Approved") + " successfully.";
                }
                catch (Exception ex)
                {
                    TempData["AlertMessage"] = "Some error occured, Please try after some time." + ex.Message.ToString();
                }
            }
            var rvd = new RouteValueDictionary();
            rvd.Add("BlogID", _Commentmodel.BlogID);
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "FirstName");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Ascending");
            rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
            rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
            return RedirectToAction("CommentListing", "Comment", rvd);
        }
    }
}
