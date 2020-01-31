using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
namespace KISD.Areas.Blog.Controllers
{
    public class ContentController : Controller
    {
        //
        // GET: /Blog/Content/

        /// <summary>
        /// Get the Data to bind the header content
        /// </summary>
        /// <returns></returns>
        public ActionResult HeaderSection()
        {
            var model = new Models.ContentModel();// Content Model
            try
            {
                var objheader = new BlogAdmin.Contexts.ContentContexts();//Content Context
                var _SettingContexts = new BlogAdmin.Contexts.SettingContexts();//Setting Context
                var header_list = objheader.GetContent(1).ToList();//Get Header content for ContentTypeID =1 (header)            
                var objSetting_List = _SettingContexts.GetSettings().ToList();// Get Setting Theme list 
                model.HeaderList = header_list;
                model.SettingList = objSetting_List;
            }
            catch (Exception)
            {
            }
            return View(model);
        }

        /// <summary>
        /// get the content to bind links in footer
        /// </summary>
        /// <returns></returns>
        public ActionResult FooterSection()
        {
            var model = new Models.ContentModel();// Content Model
            var objfooter = new BlogAdmin.Contexts.ContentContexts();//Content Context
            var _SettingContexts = new BlogAdmin.Contexts.SettingContexts();//Setting Context
            try
            {
                var footer_list = objfooter.GetContent(2).ToList();//Get Footer content for ContentTypeID =2 (Footer)          
                var objSetting_List = _SettingContexts.GetSettings().ToList();// Get Setting Theme list 
                model.SettingList = objSetting_List;
                model.FooterList = footer_list;
            }
            catch (Exception)
            {

            }

            return View(model);
        }
        /// <summary>
        /// get the content to bind links in Theme
        /// </summary>
        /// <returns></returns>
        public ActionResult ThemeSection()
        {
            var objTheme = new BlogAdmin.Contexts.SettingContexts();//Content Context
            var footer_list = objTheme.GetSettings().ToList();//Get Footer content for ContentTypeID =2 (Footer)
            var model = new Areas.Blog.Models.ContentModel();// Content Model
            model.SettingList = footer_list;
            return View(model);
        }
        /// <summary>
        /// get the content to bind the Categories Section (right section)
        /// </summary>
        /// <returns></returns>
        public ActionResult Categories()
        {
            var objBlogcontext = new BlogAdmin.Contexts.BlogsContexts();//Blog Context
            var objCategorycontext = new BlogAdmin.Contexts.CategoriesContexts();//Categories Context
            var blog_list = objBlogcontext.GetBlogs().ToList();//Blog List
            var category_list = objCategorycontext.GetCategories().OrderBy(x => x.CategoryNameTxt).ToList();//Category List
            var model = new Models.ContentModel();// Content Model
            //************************************Categories section *********************************************
            List<Models.categoryList> categories = new List<Models.categoryList>();
            try
            {
                foreach (var newitem in category_list)
                {
                    var blog_count = blog_list.Where(x => x.CategoryID == newitem.CategoryID && x.IsActiveInd == true).Count();
                    var obj = new Models.categoryList();
                    obj.CategoryID = newitem.CategoryID;
                    obj.CategoryNameTxt = newitem.CategoryNameTxt;
                    obj.CategoryCount = blog_count.ToString();//Category count in 
                    categories.Add(obj);
                }
            }
            catch (Exception)
            {
            }
            model.categoryList = categories;
            //***************************************************************************************************
            return View(model);
        }

        /// <summary>
        /// get the content to bind the Tag Cloud (right section)
        /// </summary>
        /// <returns></returns>
        public ActionResult TagCloud()
        {
            var _TagsContexts = new BlogAdmin.Contexts.TagsContexts();//Tags Context
            var model = new Models.ContentModel();// Content Model
            try
            {
                model.TagList = new SelectList(_TagsContexts.GetTags().OrderBy(x => x.TagNameTxt).ToList(), "TagID", "TagNameTxt");
            }
            catch (Exception)
            {
            }
            return View(model);
        }
        /// <summary>
        /// Get the content to bind Month List (right section)
        /// </summary>
        /// <returns></returns>
        public ActionResult MonthList()
        {
            var _BlogsContexts = new BlogAdmin.Contexts.BlogsContexts();//Blog Context
            var model = new Models.ContentModel();// Content Model
            try
            {
                var bloglist = _BlogsContexts.GetBlogs().OrderByDescending(x => x.PostedDate).ToList();
                var month_list = bloglist.GroupBy(p => p.PostedDate.Year).ToList();
                model.MonthList = month_list;
            }
            catch (Exception)
            { }

            return View(model);
        }
        /// <summary>
        /// get the content for blog details page.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult BlogContent(string Blogurl, int? BlogID, int? page, int? type, FormCollection fm)
        {
            int pageSize = Admin.Models.Common._pageSize;
            int pageNumber = (page ?? Admin.Models.Common._currentPage);
            var comments_per_post = 4;
            var _Blogcontext = new BlogAdmin.Contexts.BlogsContexts();//Blog Context
            var _CategoriesContexts = new BlogAdmin.Contexts.CategoriesContexts();//Categories Context
            var _TagsContexts = new BlogAdmin.Contexts.TagsContexts();//Tags Context
            var _SettingContexts = new BlogAdmin.Contexts.SettingContexts();//Setting Context
            var _ContextsComments = new BlogAdmin.Contexts.CommentContexts();// Comments Context

            var model = new Models.ContentModel();// Content Model
            try
            {
                var blog_list = _Blogcontext.GetBlogs().Where(x => x.SlagTxt.ToLower() == Blogurl.ToLower() && x.IsActiveInd == true).OrderBy(x => x.PostedDate).FirstOrDefault();
                if (blog_list == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                #region BLog Listing Section
                var blogID = 0;
                if (_Blogcontext != null)
                {
                    blogID = blog_list.BlogID;
                    List<Models.ContentModel> bloglist = new List<Models.ContentModel>();//Create a list of blog items
                    model = new Models.ContentModel();
                    model.AuthorNameTxt = blog_list.AuthorNameTxt;
                    model.AuthorNameID = blog_list.AuthorNameID;
                    model.BlogDescription = blog_list.BlogDescription;
                    model.BlogDescription = blog_list.BlogDescription;
                    model.CategoryID = blog_list.CategoryID;
                    model.ImagePathTxt = blog_list.ImagePathTxt;
                    model.IsActiveInd = blog_list.IsActiveInd;
                    model.IsCommentEnabledInd = blog_list.IsCommentEnabledInd;
                    model.MetaDescriptionTxt = blog_list.MetaDescriptionTxt;
                    model.MetaTitleTxt = blog_list.MetaTitleTxt;
                    model.PostedDate = blog_list.PostedDate;
                    model.SlagTxt = blog_list.SlagTxt;
                    model.SocialMediaTxt = blog_list.SocialMediaTxt;
                    model.TitleTxt = blog_list.TitleTxt;
                    model.AbstractTxt = blog_list.AbstractTxt;
                    model.CategoryName = blog_list.CategoryID != 0 ? _CategoriesContexts.GetCategories().Where(x => x.CategoryID == blog_list.CategoryID).Select(x => x.CategoryNameTxt).FirstOrDefault() : "";

                    //************************************TagList under each blog*********************************************
                    List<Models.TagNameList> blogTaglistName = new List<Models.TagNameList>();//Create a list of tags of single blog items
                    var getTagIDList = _Blogcontext.GetFormBlogTag(blog_list.BlogID).ToList();
                    foreach (var b in getTagIDList)
                    {
                        var obj = new Models.TagNameList();
                        obj.TagNameTxt = _TagsContexts.GetTags().Where(x => x.TagID == Convert.ToInt32(b)).Select(x => x.TagNameTxt).FirstOrDefault(); ;
                        obj.TagID = Convert.ToInt32(b);
                        obj.BlogID = blog_list.BlogID;
                        blogTaglistName.Add(obj);
                    }
                    model.TagNameList = blogTaglistName.OrderByDescending(x => x.TagNameTxt).ToList();
                    //****************************Social Media Listing*********************************************
                    var objSocialListing = _Blogcontext.GetFormBlogSocialMedia_List(blogID).ToList();
                    model.SocialList = objSocialListing;
                    bloglist.Add(model);
                    model.IsPagingVisible = true;
                    model.PagedBlog = bloglist.ToPagedList(pageNumber, pageSize);
                }
                #endregion

                #region Seeting Section region
                var objSetting_List = _SettingContexts.GetSettings().ToList();// Get Setting Theme list 
                comments_per_post = objSetting_List.Select(x => x.CommentPerPost).FirstOrDefault();// Comments per post
                model.SettingList = objSetting_List;
                #endregion

                #region  Comment Section
                var objComments_list = _ContextsComments.GetComments().ToList();
                objComments_list = objComments_list.Where(x => x.BlogID == blogID && x.IsActiveInd == true).Take(comments_per_post).ToList();// Comment Listing
                foreach (var item in objComments_list)
                {
                    var date_of_comment = Convert.ToDateTime(item.PostedDate);
                    var today = DateTime.Now;
                    item.No_of_days = model.Get_Year_month_days(date_of_comment, today);
                }

                model.CommentList = objComments_list;
                #endregion
            }
            catch (Exception)
            { }

            return View(model);
        }

        [HttpPost]
        public ActionResult BlogContent(string Blogurl, Content _model, int? page, int? type, FormCollection fm)
        {
            int pageSize = Admin.Models.Common._pageSize;
            int pageNumber = page ?? Admin.Models.Common._currentPage;
            var comments_per_post = 4;
            var _BlogsContexts = new BlogAdmin.Contexts.BlogsContexts();//Blog Context
            var _CategoriesContexts = new BlogAdmin.Contexts.CategoriesContexts();//Categories Context
            var _TagsContexts = new BlogAdmin.Contexts.TagsContexts();//Tags Context
            var _SettingContexts = new BlogAdmin.Contexts.SettingContexts();//Settings Context
            var _EmailsContexts = new BlogAdmin.Contexts.EmailsContexts();//Emails Context
            var _CommentsContext = new BlogAdmin.Contexts.CommentContexts();// Comments Context

            var model = new Models.ContentModel();// Content Model
            try
            {
                #region Blog List Section
                var blogID = 0;
                var blog_list = _BlogsContexts.GetBlogs().Where(x => x.SlagTxt.ToLower() == Blogurl.ToLower() && x.IsActiveInd == true).OrderBy(x => x.PostedDate).FirstOrDefault();
                if (blog_list == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                if (_BlogsContexts != null)
                {
                    blogID = blog_list.BlogID;
                    List<Models.ContentModel> bloglist = new List<Models.ContentModel>();//Create a list of blog items
                    model = new Models.ContentModel();
                    model.AuthorNameTxt = blog_list.AuthorNameTxt;
                    model.AuthorNameID = blog_list.AuthorNameID;
                    model.BlogDescription = blog_list.BlogDescription;
                    model.BlogDescription = blog_list.BlogDescription;
                    model.CategoryID = blog_list.CategoryID;
                    model.ImagePathTxt = blog_list.ImagePathTxt;
                    model.IsActiveInd = blog_list.IsActiveInd;
                    model.IsCommentEnabledInd = blog_list.IsCommentEnabledInd;
                    model.MetaDescriptionTxt = blog_list.MetaDescriptionTxt;
                    model.MetaTitleTxt = blog_list.MetaTitleTxt;
                    model.PostedDate = blog_list.PostedDate;
                    model.SlagTxt = blog_list.SlagTxt;
                    model.SocialMediaTxt = blog_list.SocialMediaTxt;
                    model.TitleTxt = blog_list.TitleTxt;
                    model.AbstractTxt = blog_list.AbstractTxt;
                    model.CategoryName = blog_list.CategoryID != 0 ? _CategoriesContexts.GetCategories().Where(x => x.CategoryID == blog_list.CategoryID).Select(x => x.CategoryNameTxt).FirstOrDefault() : "";

                    //************************************TagList under each blog*********************************************
                    List<Models.TagNameList> blogTaglistName = new List<Models.TagNameList>();//Create a list of tags of single blog items
                    var getTagIDList = _BlogsContexts.GetFormBlogTag(blog_list.BlogID).ToList();
                    foreach (var b in getTagIDList)
                    {
                        var obj = new Models.TagNameList();
                        obj.TagNameTxt = _TagsContexts.GetTags().Where(x => x.TagID == Convert.ToInt32(b)).Select(x => x.TagNameTxt).FirstOrDefault(); ;
                        obj.TagID = Convert.ToInt32(b);
                        obj.BlogID = blog_list.BlogID;
                        blogTaglistName.Add(obj);
                    }
                    model.TagNameList = blogTaglistName.OrderByDescending(x => x.TagNameTxt).ToList();
                    //****************************Social Media Listing*********************************************
                    var objSocialListing = _BlogsContexts.GetFormBlogSocialMedia_List(blogID).ToList();
                    model.SocialList = objSocialListing;
                    bloglist.Add(model);
                    model.IsPagingVisible = true;
                    model.PagedBlog = bloglist.ToPagedList(pageNumber, pageSize);
                }
                #endregion

                #region Comment Section
                //*************************Adds Comment *********************
                var name = fm["full-name"] != null ? fm["full-name"].ToString() : "";
                var email = fm["email"] != null ? fm["email"].ToString() : "";
                var phone = fm["phone"] != null ? fm["phone"].ToString() : "";
                var Comment = fm["Comment"] != null ? fm["Comment"].ToString() : "";

                var comment_model = new BlogAdmin.Models.CommentModel();
                comment_model.BlogID = blogID;
                var commentidList = _CommentsContext.GetComments().ToList();
                comment_model.CommentID = (commentidList.Count() > 0) ? commentidList.Select(x => x.CommentID).Max() + 1 : 1;
                comment_model.FullNameTxt = name;
                comment_model.EmailTxt = email;
                comment_model.PhoneNoTxt = phone;
                comment_model.CommentDescriptionTxt = Comment;
                comment_model.PostedDate = DateTime.Now;
                comment_model.IsActiveInd = false;
                _CommentsContext.AddComment(comment_model);

                //***********************Email Sending to client on every comment post*******************
                #region Email region
                var Email_List = _EmailsContexts.GetEmails().ToList();// Emails List
                var Setting_List = _SettingContexts.GetSettings().ToList();// Setting List
                bool IsSucessSendMail = false;
                var ToEmailTxt = Setting_List.Select(a => a.ReceivingEmail).FirstOrDefault();
                var SendingEmalTxt = Setting_List.Select(a => a.SendingEmail).FirstOrDefault();
                var Toemail_array = new string[] { };
                var ToEmail = string.Empty;
                if (!string.IsNullOrEmpty(ToEmailTxt) && ToEmailTxt.Contains(","))
                {
                    Toemail_array = ToEmailTxt.Split(',');
                }
                else
                {
                    Toemail_array = new string[] { ToEmailTxt };
                }
                foreach (var itememail in Toemail_array)
                {
                    ToEmail = ToEmail + "," + Email_List.Where(x => x.EmailID == Convert.ToInt32(itememail)).Select(x => x.EmailTxt).FirstOrDefault();
                }
                ToEmail = ToEmail.Trim(',');
                var fromEmail = Email_List.Where(x => x.EmailID == Convert.ToInt32(!string.IsNullOrEmpty(SendingEmalTxt) ? SendingEmalTxt : "0")).Select(x => x.EmailTxt).FirstOrDefault();

                if (ToEmail != null && fromEmail != null)
                {
                    var myMailUtilityBAL = new Common.MailUtilityBAL();
                    var body = "";
                    #region Contact US
                    body = "<table style=\"font-family:Arial;font-size:12px\" border=\"0\" align=\"left\" cellpadding=\"4\" cellspacing=\"0\" >";
                    body += "<tr><td colspan='2' > Dear Admin,<br/><br/>Following user has posted comment from Killeen ISD website blog with the following information:</td></tr>";
                    body += @"<tr><td colspan='2' ><div style='float:left; padding:20px; background:#F8F8F8; width:450px; font-family:Arial;font-size: 12px;'>
                                <table width='100%' cellspacing='2' cellpadding='2'
                                style='font-family:Arial;font-size: 12px;'><tbody>";
                    body += @" <tr>
                                            <td width='45%' valign='top' align='left' style='padding: 2px 0;'><b> Name:</b></td>
                                            <td width='55%' valign='top' align='left' style='padding: 2px 0;'>" + name + @"
                                            </td>
                                        </tr>
                                          <tr>
                                            <td valign='top' align='left' style='padding: 2px 0;'><b>Email:</b></td>
                                            <td valign='top' align='left' style='padding: 2px 0;'>" + (string.IsNullOrEmpty(email) ? "-" : email) + @"
                                            </td>
                                        </tr> 
                                        <tr>
                                            <td valign='top' align='left' style='padding: 2px 0;'><b>Phone:</b></td>
                                            <td valign='top' align='left' style='padding: 2px 0;'>" + (string.IsNullOrEmpty(phone) ? "-" : phone) + @"
                                            </td>
                                        </tr> 
                                        <tr>
                                            <td valign='top' align='left' style='padding: 2px 0;'><b>Comment:</b></td>
                                            <td valign='top' align='left' style='padding: 2px 0;'>" + (string.IsNullOrEmpty(Comment) ? "-" : Comment) + @"
                                            </td>
                                        </tr>
                                       ";
                    body += @"</table></div></td></tr>";
                    body = body + "<tr><td colspan='2'><br/>Regards,<br/>Killeen ISD Team.</td></tr></table>";
                    #endregion
                    try
                    {
                        if (ToEmail.Contains(","))
                        {
                            var ToEmailArray = ToEmail.Split(',');
                            foreach (var item in ToEmailArray)
                            {
                                if (myMailUtilityBAL.SendEmail(fromEmail, item, body, ("Blog comment posting")).ToLower().Trim() == "ok")
                                {
                                    IsSucessSendMail = true;
                                }
                            }
                        }
                        else
                        {
                            if (myMailUtilityBAL.SendEmail(fromEmail, ToEmail, body, ("Blog comment posting")).ToLower().Trim() == "ok")
                            {
                                IsSucessSendMail = true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IsSucessSendMail = false;
                    }
                    var str = IsSucessSendMail ? "Comment posted successfully." : "There is an error while sending email. Please try again.";
                    TempData["AlertMessage"] = str;
                    #endregion
                }
                else
                {
                    TempData["AlertMessage"] = "Comment posted successfully.";
                }
                //**********************************Setting Section****************************************
                comments_per_post = Setting_List.Select(x => x.CommentPerPost).FirstOrDefault();// Comments per post
                model.SettingList = Setting_List;

                // ********************Gets updated Commnet List **********************
                var _CommentContexts_Updated = new BlogAdmin.Contexts.CommentContexts();//Setting Context
                var objComments_list = _CommentContexts_Updated.GetComments().OrderByDescending(x => x.PostedDate).ToList();
                objComments_list = objComments_list.Where(x => x.BlogID == blogID && x.IsActiveInd == true).Take(comments_per_post).ToList();// Comment Listing
                foreach (var item in objComments_list)
                {
                    var date_of_comment = Convert.ToDateTime(item.PostedDate);
                    var today = DateTime.Now;
                    item.No_of_days = model.Get_Year_month_days(date_of_comment, today);
                }

                model.CommentList = objComments_list;// Comment List
                #endregion
            }
            catch (Exception)
            {

            }
            return View(model);
        }
    }
}
