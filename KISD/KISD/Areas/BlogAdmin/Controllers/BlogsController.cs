using KISD.Areas.BlogAdmin.Models;
using MvcContrib.UI.Grid;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace KISD.Areas.BlogAdmin.Controllers
{
    public class BlogsController : Controller
    {
        /// <summary>
        /// Method to get display the the Added Blog in Grid.
        /// Code to update the status  by clicking the checkboxes added for the status .
        /// </summary>
        /// <param name="gridSortOptions">It will pass sorting parameters</param>
        /// <param name="page">Defines the page on which Blogs is currently viewing records</param>
        /// <param name="pagesize">Number of records to be displayed on he page.</param>
        /// <param name="fm">Form hidden field values</param>
        /// <returns></returns>
        [Authorize]
        [SessionExpire]
        public ActionResult BlogsListing(GridSortOptions gridSortOptions, int? pagetype, int? page, int? pagesize, FormCollection fm, string objresult)
        {
            var _objContext = new Contexts.BlogsContexts();
            ViewBag.Title = " Post Listing";
            var _BlogsModel = new BlogsModel();
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
                    var blog = _objContext.GetBlogs().Where(x => x.BlogID == id1).FirstOrDefault();
                    if (blog != null)
                    {
                        blog.IsActiveInd = objAjaxRequest.hfvalue == "1";
                        try
                        {
                            blog.strCategoryid = Convert.ToString(blog.CategoryID);
                            _objContext.EditBlogs(blog, true);
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
                //gridSortOptions.Column = "CreateDate";
                Session["PageSize"] = null;
                Session["pageNo"] = null;
                Session["GridSortOption"] = null;
            }

            var pageSize = pagesize.HasValue ? pagesize.Value : Models.Common._pageSize;
            var Page = page.HasValue ? page.Value : Models.Common._currentPage;
            TempData["pager"] = pagesize;

            if (gridSortOptions.Column != null && gridSortOptions.Column == "TitleTxt" || gridSortOptions.Column == "AuthorNameTxt" || gridSortOptions.Column == "PostedDate")
            {
            }
            else
            {
                gridSortOptions.Column = "TitleTxt";
            }

            var pagedViewModel = new PagedViewModel<BlogsModel>
            {
                ViewData = ViewData,
                Query = _objContext.GetBlogs().AsQueryable(),
                GridSortOptions = gridSortOptions,
                DefaultSortColumn = "TitleTxt",
                Page = Page,
                PageSize = pageSize,
            }.Setup();

            if (Request.IsAjaxRequest())// check if request comes from ajax, then return Partial view
            {
                return View("BlogsListingPartial", pagedViewModel);// ("partial view name ")
            }
            else
            {
                return View(pagedViewModel);
            }
        }
        [Authorize]
        [SessionExpire]
        public ActionResult CreateBlog(int? Blogid)
        {
            var _BlogsContexts = new Contexts.BlogsContexts();
            var _CategoriesContexts = new Contexts.CategoriesContexts();
            var _UsersContexts = new Contexts.UsersContexts();
            var _BlogsModel = new BlogsModel();
            ViewBag.Title = (Blogid.HasValue ? "Edit " : "Add ") + " Post Details ";
            ViewBag.Submit = Blogid.HasValue && Blogid.Value > 0 ? "Update" : "Save";

            bool isActive = true, allowComments = true;
            _BlogsModel.TagList = GetAllTags();
            _BlogsModel.SocialMediaList = GetAllSocialMedia();
            try
            {
                var taglist = ViewBag.Tag = string.Join(",", _BlogsModel.TagList.Select(x => x.Text));
                if (Blogid.HasValue && Blogid.Value > 0)
                {
                    _BlogsModel = _BlogsContexts.GetBlogs().Where(x => x.BlogID == Blogid).FirstOrDefault();
                    if (_BlogsModel != null)
                    {
                        isActive = _BlogsModel.IsActiveInd;
                        allowComments = _BlogsModel.IsCommentEnabledInd;
                        _BlogsModel.strCategoryid = Convert.ToString(_BlogsModel.CategoryID);

                        var selecetdtagNamearray = Array.ConvertAll<string, string>(_BlogsContexts.GetFormBlogTags(Blogid.Value).ToArray(),
                                                                        delegate (string i)
                                                                        {
                                                                            return (string)i.ToString();
                                                                        });

                        var selecetdtagIDarray = Array.ConvertAll<string, string>(_BlogsContexts.GetFormBlogTagsIDList(Blogid.Value).ToArray(),
                                                                        delegate (string i)
                                                                        {
                                                                            return (string)i.ToString();
                                                                        });

                        var selecetdSocialMediasarray = Array.ConvertAll<int, string>(_BlogsContexts.GetFormBlogSocialMedia(Blogid.Value).ToArray(),
                                                                       delegate (int i)
                                                                       {
                                                                           return (string)i.ToString();
                                                                       });

                        var UserList = _UsersContexts.GetUser(_BlogsModel.AuthorNameTxt);//User list based on userid
                        _BlogsModel.AuthorNameID = Convert.ToString(UserList != null ? UserList.UserID : 0);//UserID
                        ViewBag.Category = new SelectList(_CategoriesContexts.GetSelectedCategories(Convert.ToString(_BlogsModel.CategoryID)), "CategoryID", "CategoryNameTxt");
                        ViewBag.AuthorName = new SelectList(_UsersContexts.GetUserList(_BlogsModel.AuthorNameID), "UserID", "UserNameTxt");//Author drop down listing
                        _BlogsModel.TagList = GetAllTags();
                        _BlogsModel.SocialMediaList = GetAllSocialMedia();
                        _BlogsModel.SelectedTags = selecetdtagNamearray;
                        _BlogsModel.SelectedTagsID = selecetdtagIDarray;
                        _BlogsModel.SelectedSocialMedia = selecetdSocialMediasarray;
                    }
                }
                else
                {
                    ViewBag.Category = new SelectList(_CategoriesContexts.GetSelectedCategories(string.Empty), "CategoryID", "CategoryNameTxt");
                    ViewBag.AuthorName = new SelectList(_UsersContexts.GetUserList(string.Empty), "UserID", "UserNameTxt");//Author drop down listing
                    string[] Tagsarr = new string[] { "0" };
                    _BlogsModel.SelectedTagsID = Tagsarr;
                }
            }
            catch (Exception ex)
            {
                ViewBag.Category = new SelectList(_CategoriesContexts.GetSelectedCategories(string.Empty), "CategoryID", "CategoryNameTxt");
                ViewBag.AuthorName = new SelectList(_UsersContexts.GetUserList(string.Empty), "UserID", "UserNameTxt");//Author drop down listing
                string[] Tagsarr = new string[] { "0" };
                _BlogsModel.SelectedTagsID = Tagsarr;
            }
            ViewBag.IsCommentEnabledInd = Models.Common.GetStatusListBoolean(allowComments ? "true" : "false");
            ViewBag.IsActiveInd = Models.Common.GetStatusListBoolean(isActive ? "true" : "false");
            return View(_BlogsModel);
        }

        /// <summary>
        /// Method to save/ update the added Blogs into the database.
        /// </summary>
        /// <param name="_Blogsmodel">BlogsModel that will contain the values saved in the Blogs View</param>
        /// <param name="command">This will contain value on click of Cancel button</param>
        /// <param name="fm">It contains the form collection (hidden field etc) values.</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [SessionExpire]
        [ValidateInput(false)]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult CreateBlog(BlogsModel _Blogsmodel, string command, FormCollection fm)
        {

            Session["Edit/Delete"] = "Edit";
            var file = Request.Files.Count > 0 ? Request.Files[0] : null;
            ViewBag.Title = (_Blogsmodel.BlogID > 0 ? "Edit " : "Add ") + " Post Details ";
            ViewBag.Submit = _Blogsmodel.BlogID > 0 ? "Update" : "Save";
            bool isActive = true, allowComments = true;
            _Blogsmodel.TagList = GetAllTags();
            var _UsersContexts = new Contexts.UsersContexts();
            var _CategoriesContexts = new Contexts.CategoriesContexts();
            var BlogsContext = new Contexts.BlogsContexts();
            var TagContext = new Contexts.TagsContexts();
            ViewBag.AuthorName = new SelectList(_UsersContexts.GetUserList(_Blogsmodel.AuthorNameID), "UserID", "UserNameTxt");//Author drop down listing
            ViewBag.Category = new SelectList(_CategoriesContexts.GetSelectedCategories(string.Empty), "CategoryID", "CategoryNameTxt");
        
            if (string.IsNullOrEmpty(command))
            {
                isActive = _Blogsmodel.IsActiveInd;
                allowComments = _Blogsmodel.IsCommentEnabledInd;
                ViewBag.IsCommentEnabledInd = Models.Common.GetStatusListBoolean(allowComments ? "true" : "false");
                ViewBag.IsActiveInd = Models.Common.GetStatusListBoolean(isActive ? "true" : "false");
                _Blogsmodel.SocialMediaList = GetAllSocialMedia();
                _Blogsmodel.strCategoryid = Convert.ToString(_Blogsmodel.strCategoryid);
                if (BlogsContext.GetBlogs().Where(x => x.TitleTxt == _Blogsmodel.TitleTxt && _Blogsmodel.BlogID != x.BlogID).Any())
                {
                    ModelState.AddModelError("TitleTxt", _Blogsmodel.TitleTxt + " Post already exists.");
                    return View(_Blogsmodel);
                }

                if (_Blogsmodel.SlagTxt.ToLower()=="error404") //check 404 error
                {
                    ModelState.AddModelError("SlagTxt", _Blogsmodel.SlagTxt + " URL is not allowed.");
                    return View(_Blogsmodel);
                }

                try
                {
                    //Save image path
                    if (file != null && file.ContentLength > 0)
                    {
                        #region Upload Image
                        Models.Common.CreateFolder();
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
                        TempData["CroppedImage"] = null;

                        _Blogsmodel.ImagePathTxt = "~/WebData/images/" + fileName;
                        var width = 250;
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
                        var mysmallImage = Models.Common.CreateImageThumbnail(strPath, 200);
                        mysmallImage.Save(Request.PhysicalApplicationPath + "WebData\\thumbnails_Small\\" + fileName,
                                       fileExtension.ToLower() == ".png" ?
                                       System.Drawing.Imaging.ImageFormat.Png :
                                       fileExtension.ToLower() == ".gif" ?
                                       System.Drawing.Imaging.ImageFormat.Gif :
                                       System.Drawing.Imaging.ImageFormat.Jpeg
                                       );
                        mysmallImage.Dispose();
                        #endregion
                    }
                    else
                    {
                        _Blogsmodel.ImagePathTxt = BlogsContext.GetBlogs().Where(x => x.BlogID == _Blogsmodel.BlogID).Select(x => x.ImagePathTxt).FirstOrDefault();
                    }
                    if (ViewBag.Submit == "Save")
                    {
                        BlogsContext.AddBlogs(_Blogsmodel);
                        TempData["AlertMessage"] = "Post details saved successfully.";
                    }
                    else
                    {
                        BlogsContext.EditBlogs(_Blogsmodel, false);
                        TempData["AlertMessage"] = "Post details updated successfully.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["AlertMessage"] = "Some error occured, Please try after some time. " + ex.Message;
                }
            }
            var rvd = new RouteValueDictionary();
            rvd.Add("Column", Request.QueryString["Column"] != null ? Request.QueryString["Column"].ToString() : "PostedDate");
            rvd.Add("Direction", Request.QueryString["Direction"] != null ? Request.QueryString["Direction"].ToString() : "Descending");
            rvd.Add("pagesize", Request.QueryString["pagesize"] != null ? Request.QueryString["pagesize"].ToString() : Models.Common._pageSize.ToString());
            rvd.Add("page", Request.QueryString["page"] != null ? Request.QueryString["page"].ToString() : Models.Common._currentPage.ToString());
            return RedirectToAction("BlogsListing", "Blogs", rvd);
        }

        private SelectList GetAllTags()
        {
            var _TagContexts = new Contexts.TagsContexts();
            var list = _TagContexts.GetTags().OrderBy(x => x.TagNameTxt).Select(x => new TagsModel()
            {
                TagID = x.TagID,
                TagNameTxt = x.TagNameTxt
            }).ToList();
            var objselectlist = new SelectList(list, "TagID", "TagNameTxt");
            return objselectlist;
        }
        private SelectList GetAllSocialMedia()
        {
            var _SocialMediaContexts = new Contexts.SocialMediasContexts();
            var list = _SocialMediaContexts.GetSocialMedias().OrderBy(x => x.SocialMediaNameTxt).Select(x => new SocialMediasModel()
            {
                SocialMediaID = x.SocialMediaID,
                SocialMediaNameTxt = x.SocialMediaNameTxt
            }).ToList();
            var objSocialMedia = new SocialMediasModel();
            objSocialMedia.SocialMediaNameTxt = "-- Select Social Media --";
            objSocialMedia.SocialMediaID = 0;
            list.Insert(0, objSocialMedia);
            var objselectlist = new SelectList(list, "SocialMediaID", "SocialMediaNameTxt");
            return objselectlist;
        }

        /// <summary>
        /// Method to delete the blogs based on blog id.
        /// </summary>
        /// <param name="blogid"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [SessionExpire]
        public JsonResult Delete(int? blogid)
        {
            Session["Edit/Delete"] = "Delete";
            var BLogContext = new Contexts.BlogsContexts();
            var CommentContext = new Contexts.CommentContexts();

            if (blogid.HasValue)
            {
                try
                {
                    if (BLogContext != null)
                    {
                        BLogContext.DeleteBlogs(blogid);
                        CommentContext.DeleteBlogComments(blogid);
                        BLogContext.DeleteFormBlogSocialMedia(blogid);
                        BLogContext.DeleteFormBlogTag(blogid);
                        TempData["AlertMessage"] = "Post details deleted successfully.";
                    }
                }
                catch
                {
                    TempData["AlertMessage"] = "Some error occured while deleting the Email, Please try again later.";
                }
            }
            var rvd = new RouteValueDictionary();
            int? Page = 1;
            var count = 1;
            count = BLogContext.GetBlogs().Count();
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
            rvd.Add("RoleID", ViewBag.RoleID);
            return Json(Url.Action("BlogsListing", "Blogs", rvd));
        }
    }
}
