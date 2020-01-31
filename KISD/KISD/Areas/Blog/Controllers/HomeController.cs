using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace KISD.Areas.Blog.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Blog/Home/

        public ActionResult Index(string search, string type_ID, int? page, int? type, FormCollection fm)
        {
            int pageSize = Admin.Models.Common._pageSize;
            int pageNumber = (page ?? Admin.Models.Common._currentPage);

            #region Search Section
            //var search_type_ID = string.IsNullOrEmpty(type_ID) ? "" : type_ID;//Contains search type and ID of search, Tag
            var search_type = string.Empty;
            var typeID = string.Empty;

            //************************************** Search Type and ID***************************
            var Type_ID = Request.QueryString["type_ID"] == null ? "" : Request.QueryString["type_ID"].ToString();

            if (!string.IsNullOrEmpty(type_ID) && type_ID != "")
            {
                var search_type_ID = type_ID.Split(new[] { '0' }, 2);//Split on first occurence of 0
                search_type = search_type_ID[0].ToString().Trim().ToLower();
                typeID = search_type_ID[1] != null ? search_type_ID[1].ToString() : "0";
            }
            else if (!string.IsNullOrEmpty(Type_ID) && Type_ID != "")
            {
                var search_type_ID = type_ID.Split(new[] { '0' }, 2);//Split on first occurence of 0
                search_type = search_type_ID[0].ToString().Trim().ToLower();
                typeID = search_type_ID[1] != null ? search_type_ID[1].ToString() : "0";
            }
            //*************************************************************************************

            var search_txt = string.IsNullOrEmpty(Request.QueryString["search"]) ? "" : Request.QueryString["search"].ToLower().ToString();

            if (string.IsNullOrEmpty(search_txt) && !string.IsNullOrEmpty(search))
            {
                search_txt = search;
            }
            #endregion
            var _BlogsContexts = new BlogAdmin.Contexts.BlogsContexts();//Blog Context
            var _CategoriesContexts = new BlogAdmin.Contexts.CategoriesContexts();//Categories Context
            var _TagsContexts = new BlogAdmin.Contexts.TagsContexts();//Tags Context
            var _SettingContexts = new BlogAdmin.Contexts.SettingContexts();//Settings Context
            var model = new Models.ContentModel();// Content Model

            try
            {
                //**********************************Setting Section****************************************
                var Setting_List = _SettingContexts.GetSettings().ToList();// Setting List
                pageSize = Setting_List.Select(x => x.PostPerPage).FirstOrDefault();// Posts per page
                model.SettingList = Setting_List;
                //------------------------------------------------------------------------------------------

                var blogID = 0;
                if (_BlogsContexts != null)
                {
                    var blog_list = new List<BlogAdmin.Models.BlogsModel>();
                    if (search_txt != "" && !string.IsNullOrEmpty(search_txt) && !string.IsNullOrEmpty(search_type))
                    {
                        switch (search_type)
                        {
                            case "blog_search":
                                blog_list = _BlogsContexts.GetBlogs().Where(x => x.TitleTxt.Trim().ToLower().Contains(search_txt) && x.IsActiveInd == true).OrderByDescending(x => x.PostedDate).ToList();//Get blog list on the basis of search text
                                break;
                            case "tag_search":
                                blog_list = _BlogsContexts.GetBlogs().Where(x => x.IsActiveInd == true).OrderByDescending(x => x.PostedDate).ToList();
                                var objFormBlogTag = _BlogsContexts.GetAllFormBlogTags(Convert.ToInt32(typeID));
                                var list_new = (from a in blog_list
                                                join b in objFormBlogTag
                                                on a.BlogID equals b.BlogID
                                                select new { a }).ToList();
                                blog_list = list_new.Select(x => x.a).OrderByDescending(x => x.PostedDate).ToList();
                                break;
                            case "category_search":
                                blog_list = _BlogsContexts.GetBlogs().Where(x => x.CategoryID == Convert.ToInt32(typeID) && x.IsActiveInd == true).OrderByDescending(x => x.PostedDate).ToList();//Get blog list on the basis of search text
                                break;
                            case "auther_search":
                                blog_list = _BlogsContexts.GetBlogs().Where(x => x.AuthorNameID == typeID && x.IsActiveInd == true).OrderByDescending(x => x.PostedDate).ToList();//Get blog list on the basis of search text
                                break;
                            case "monthlist_search":
                                #region
                                var month_year = typeID.Split('_');
                                var month = month_year[0].ToString();
                                var year = month_year[1].ToString();
                                #endregion
                                blog_list = _BlogsContexts.GetBlogs().Where(x => x.PostedDate.Month == Convert.ToInt32(month) && x.PostedDate.Year == Convert.ToInt32(year) && x.IsActiveInd == true).OrderByDescending(x => x.PostedDate).ToList();//Get blog list on the basis of search text
                                break;
                            default:
                                blog_list = _BlogsContexts.GetBlogs().Where(x => x.IsActiveInd == true).OrderByDescending(x => x.PostedDate).ToList();
                                break;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type_ID))// Search for Text Box Search
                        {
                            blog_list = _BlogsContexts.GetBlogs().Where(x => x.TitleTxt.Trim().ToLower().Contains(search_txt) && x.IsActiveInd == true).OrderByDescending(x => x.PostedDate).ToList();//Get blog list on the basis of search text  
                        }
                        else
                        {
                            blog_list = _BlogsContexts.GetBlogs().Where(x => x.IsActiveInd == true).OrderByDescending(x => x.PostedDate).ToList();
                        }
                    }

                    List<Models.ContentModel> bloglist = new List<Models.ContentModel>();//Create a list of blog items

                    foreach (var a in blog_list)
                    {
                        model = new Models.ContentModel();
                        model.TitleTxt = a.TitleTxt;
                        model.AuthorNameTxt = a.AuthorNameTxt;
                        model.AuthorNameID = a.AuthorNameID;
                        model.BlogDescription = a.BlogDescription;
                        model.CategoryID = a.CategoryID;
                        model.BlogID = blogID = a.BlogID;
                        model.ImagePathTxt = a.ImagePathTxt;
                        model.IsActiveInd = a.IsActiveInd;
                        model.IsCommentEnabledInd = a.IsCommentEnabledInd;
                        model.MetaDescriptionTxt = a.MetaDescriptionTxt;
                        model.MetaTitleTxt = a.MetaTitleTxt;
                        model.PostedDate = a.PostedDate;
                        model.SlagTxt = a.SlagTxt;
                        model.SocialMediaTxt = a.SocialMediaTxt;
                        model.AbstractTxt = a.AbstractTxt;
                        model.CategoryName = a.CategoryID != 0 ? _CategoriesContexts.GetCategories().Where(x => x.CategoryID == a.CategoryID).Select(x => x.CategoryNameTxt).FirstOrDefault() : "";

                        //************************************TagList under each blog*********************************************
                        List<Models.TagNameList> blogTaglistName = new List<Models.TagNameList>();//Create a list of tags of single blog items
                        var getTagIDList = _BlogsContexts.GetFormBlogTag(a.BlogID).ToList();
                        foreach (var b in getTagIDList)
                        {
                            var obj = new Models.TagNameList();
                            obj.TagNameTxt = _TagsContexts.GetTags().Where(x => x.TagID == Convert.ToInt32(b)).Select(x => x.TagNameTxt).FirstOrDefault(); ;
                            obj.TagID = Convert.ToInt32(b);
                            obj.BlogID = a.BlogID;
                            blogTaglistName.Add(obj);
                        }
                        model.TagNameList = blogTaglistName;
                        //****************************Social Media Listing*********************************************
                        List<BlogAdmin.Models.FormBlogSocialMedia> social_listName = new List<BlogAdmin.Models.FormBlogSocialMedia>();//Create a list of tags of single blog items
                        social_listName = _BlogsContexts.GetFormBlogSocialMedia_List(blogID).ToList();
                        model.SocialList = social_listName;
                        bloglist.Add(model);
                    }
                    model.IsPagingVisible = bloglist.Count > pageSize;
                    model.PagedBlog = bloglist.ToPagedList(pageNumber, pageSize);
                }
                var objheader = new BlogAdmin.Contexts.ContentContexts();//Content Context
                var header_list = objheader.GetContent(1).ToList();//Get Header content for ContentTypeID =1 (header)
                model.HeaderList = header_list;

                var objTheme = new BlogAdmin.Contexts.SettingContexts();//Setting Context
                var objTheme_list = objTheme.GetSettings().ToList();// Get Settings Theme list 
                model.SettingList = objTheme_list;
            }
            catch (Exception)
            {
            }
            return View(model);
        }
        /// <summary>
        /// Post Method For Seacrh Operation
        /// </summary>
        /// <param name="searchTxt"></param>
        /// <param name="page"></param>
        /// <param name="fm"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(string search, string type_ID, int? page, FormCollection fm)
        {
            int pageSize = Admin.Models.Common._pageSize;
            int pageNumber = (page ?? Admin.Models.Common._currentPage);

            var search_txt = string.IsNullOrEmpty(Request.QueryString["search"]) ? "" : Request.QueryString["search"].ToString();
            if (string.IsNullOrEmpty(search_txt) && !string.IsNullOrEmpty(search))
            {
                search_txt = search.Trim().ToLower();
            }

            //search = fm["search"] != null ? fm["search"].ToString() : search;
            var _BlogsContexts = new BlogAdmin.Contexts.BlogsContexts();//Blog Context
            var _CategoriesContexts = new BlogAdmin.Contexts.CategoriesContexts();//Categories Context
            var _TagsContexts = new BlogAdmin.Contexts.TagsContexts();//Tags Context           
            var _SettingContexts = new BlogAdmin.Contexts.SettingContexts();//Settings Context
            var model = new Models.ContentModel();// Content Model
            var blog_list = new List<BlogAdmin.Models.BlogsModel>();
            try
            { //**********************************Setting Section****************************************
                var Setting_List = _SettingContexts.GetSettings().ToList();// Setting List
                pageSize = Setting_List.Select(x => x.PostPerPage).FirstOrDefault();// Posts per page
                model.SettingList = Setting_List;
                //-----------------------------------------------------------------------------------------

                var blogID = 0;
                if (_BlogsContexts != null)
                {
                    if (search_txt != "" && !string.IsNullOrEmpty(search_txt))
                    {
                        blog_list = _BlogsContexts.GetBlogs().Where(x => x.TitleTxt.Trim().ToLower().Contains(search_txt) && x.IsActiveInd == true).OrderByDescending(x => x.PostedDate).ToList();//Get blog list on the basis of search text  
                    }
                    else
                    {
                        blog_list = _BlogsContexts.GetBlogs().Where(x => x.IsActiveInd == true).OrderByDescending(x => x.PostedDate).ToList();
                    }
                    List<Models.ContentModel> bloglist = new List<Models.ContentModel>();//Create a list of blog items

                    foreach (var a in blog_list)
                    {
                        model = new Models.ContentModel();
                        model.AuthorNameTxt = a.AuthorNameTxt;
                        model.AuthorNameID = a.AuthorNameID;
                        model.BlogDescription = a.BlogDescription;
                        model.BlogDescription = a.BlogDescription;
                        model.CategoryID = a.CategoryID;
                        model.BlogID = blogID = a.BlogID;
                        model.ImagePathTxt = a.ImagePathTxt;
                        model.IsActiveInd = a.IsActiveInd;
                        model.IsCommentEnabledInd = a.IsCommentEnabledInd;
                        model.MetaDescriptionTxt = a.MetaDescriptionTxt;
                        model.MetaTitleTxt = a.MetaTitleTxt;
                        model.PostedDate = a.PostedDate;
                        model.SlagTxt = a.SlagTxt;
                        model.SocialMediaTxt = a.SocialMediaTxt;
                        model.TitleTxt = a.TitleTxt;
                        model.AbstractTxt = a.AbstractTxt;
                        model.CategoryName = a.CategoryID != 0 ? _CategoriesContexts.GetCategories().Where(x => x.CategoryID == a.CategoryID).Select(x => x.CategoryNameTxt).FirstOrDefault() : "";

                        //************************************TagList under each blog*********************************************
                        List<Models.TagNameList> blogTaglistName = new List<Models.TagNameList>();//Create a list of tags of single blog items
                        var getTagIDList = _BlogsContexts.GetFormBlogTag(a.BlogID).ToList();
                        foreach (var b in getTagIDList)
                        {
                            var obj = new Models.TagNameList();
                            obj.TagNameTxt = _TagsContexts.GetTags().Where(x => x.TagID == Convert.ToInt32(b)).Select(x => x.TagNameTxt).FirstOrDefault(); ;
                            obj.TagID = Convert.ToInt32(b);
                            obj.BlogID = a.BlogID;
                            blogTaglistName.Add(obj);
                        }
                        model.TagNameList = blogTaglistName.OrderBy(x => x.TagNameTxt).ToList();
                        //****************************Social Media Listing*********************************************
                        List<BlogAdmin.Models.FormBlogSocialMedia> social_listName = new List<BlogAdmin.Models.FormBlogSocialMedia>();//Create a list of tags of single blog items
                        social_listName = _BlogsContexts.GetFormBlogSocialMedia_List(blogID).ToList();
                        model.SocialList = social_listName;
                        bloglist.Add(model);
                    }
                    model.IsPagingVisible = bloglist.Count > pageSize;
                    model.PagedBlog = bloglist.ToPagedList(pageNumber, pageSize);
                }
                var objheader = new BlogAdmin.Contexts.ContentContexts();//Content Context
                var header_list = objheader.GetContent(1).ToList();//Get Header content for ContentTypeID =1 (header)
                model.HeaderList = header_list;
                var objTheme = new BlogAdmin.Contexts.SettingContexts();//Setting Context
                var objTheme_list = objTheme.GetSettings().ToList();// Get Setting Theme list 
                model.SettingList = objTheme_list;
            }
            catch (Exception)
            {
            }
            return View(model);
        }
    }
}
