using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KISD.Areas.BlogAdmin.Models;
using System.Xml.Linq;

namespace KISD.Areas.BlogAdmin.Contexts
{
    public class BlogsContexts
    {
        private List<BlogsModel> allBlogs;
        private XDocument BlogsData;
        private XDocument BlogsTagsData;
        private List<FormBlogTags> allFormBlogsTags;
        private XDocument BlogsSocialMediaData;
        private List<FormBlogSocialMedia> allFormBlogsSocialMedia;
        public BlogsContexts()
        {
            try
            {
                allBlogs = new List<BlogsModel>();
                BlogsData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/blogs.xml"));
                var Blogs = from t in BlogsData.Descendants("Blog")
                            select new BlogsModel(
                                (int)t.Element("BlogID"),
                                t.Element("TitleTxt").Value,
                                t.Element("AuthorNameTxt").Value,
           t.Element("BlogDescription").Value,
           t.Element("ImagePathTxt").Value,
           (DateTime)t.Element("PostedDate"),
           t.Element("SlagTxt").Value,
           (bool)t.Element("IsActiveInd"),
           t.Element("MetaTitleTxt").Value,
           t.Element("MetaKeywordTxt").Value,
           t.Element("MetaDescriptionTxt").Value,
           (bool)t.Element("IsCommentEnabledInd"),
           t.Element("SocialMediaTxt").Value,
           (int)t.Element("CategoryID"),
           t.Element("AbstractTxt").Value);
                allBlogs.AddRange(Blogs.ToList<BlogsModel>());

                allFormBlogsTags = new List<FormBlogTags>();
                BlogsTagsData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/formblogstag.xml"));
                var FormBlogTag = from t in BlogsTagsData.Descendants("FormBlogTag")
                                  select new FormBlogTags(
                                      (int)t.Element("BlogTagID"),
                                      (int)t.Element("BlogID"),
                                      t.Element("TagID").Value);
                allFormBlogsTags.AddRange(FormBlogTag.ToList<FormBlogTags>());

                allFormBlogsSocialMedia = new List<FormBlogSocialMedia>();
                BlogsSocialMediaData = XDocument.Load(HttpContext.Current.Server.MapPath("~/App_Data/formblogsSocialMedia.xml"));
                var FormBlogSM = from t in BlogsSocialMediaData.Descendants("FormBlogSocialMedia")
                                 select new FormBlogSocialMedia(
                                     (int)t.Element("BlogSocialMediaID"),
                                     (int)t.Element("BlogID"),
                                     (int)t.Element("SocialMediaID"));
                allFormBlogsSocialMedia.AddRange(FormBlogSM.ToList<FormBlogSocialMedia>());
            }
            catch (Exception ex)
            {

                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets information from the data source for a Blogs. 
        /// </summary>
        /// <returns>A Blog object populated with all blog's information from the data source.</returns>
        public IEnumerable<BlogsModel> GetBlogs()
        {
            var list = allBlogs;
            var _objUsersContexts = new Areas.BlogAdmin.Contexts.UsersContexts();

            foreach (var item in list)
            {
                int val = 0;//Initialize any integer value
                if (int.TryParse(item.AuthorNameTxt, out val))
                {
                    item.AuthorNameID = item.AuthorNameTxt;
                    item.AuthorNameTxt = _objUsersContexts.GetAccountUsers().Where(x => x.UserID == Convert.ToInt64(item.AuthorNameTxt)).Select(x => x.UserNameTxt).FirstOrDefault();
                    item.AuthorNameTxt = !string.IsNullOrEmpty(item.AuthorNameTxt) ? item.AuthorNameTxt : "N/A";
                }

            }
            return list;
        }

        /// <summary>
        /// Gets information from the data source for a Blog. 
        /// </summary>
        /// <param name="BlogName">The name of the Blog to get information for.</param>
        /// <returns>A Blog object populated with the specified blog's information from the data source.</returns>
        public BlogsModel GetBlog(string BlogName)
        {
            return allBlogs.Find(item => item.TitleTxt == BlogName);
        }
        public void AddBlogs(BlogsModel _BlogsModel)
        {
            _BlogsModel.BlogID = (int)(from S in BlogsData.Descendants("Blog") orderby (short)S.Element("BlogID") descending select (short)S.Element("BlogID")).FirstOrDefault() + 1;
            BlogsData.Root.Add(new XElement("Blog", new XElement("BlogID", _BlogsModel.BlogID),
                               new XElement("TitleTxt", !string.IsNullOrEmpty(_BlogsModel.TitleTxt) ? _BlogsModel.TitleTxt : ""),
                               new XElement("AuthorNameTxt", !string.IsNullOrEmpty(_BlogsModel.AuthorNameID) ? _BlogsModel.AuthorNameID : ""),
                               new XElement("BlogDescription", !string.IsNullOrEmpty(_BlogsModel.BlogDescription) ? _BlogsModel.BlogDescription : ""),
                               new XElement("ImagePathTxt", !string.IsNullOrEmpty(_BlogsModel.ImagePathTxt) ? _BlogsModel.ImagePathTxt : ""),
                               new XElement("PostedDate", !string.IsNullOrEmpty(Convert.ToString(_BlogsModel.PostedDate)) ? _BlogsModel.PostedDate : System.DateTime.Today),
                               new XElement("SlagTxt", !string.IsNullOrEmpty(_BlogsModel.SlagTxt) ? _BlogsModel.SlagTxt : ""),
                               new XElement("IsActiveInd", _BlogsModel.IsActiveInd),
                               new XElement("MetaTitleTxt", !string.IsNullOrEmpty(_BlogsModel.MetaTitleTxt) ? _BlogsModel.MetaTitleTxt : ""),
                               new XElement("MetaKeywordTxt", !string.IsNullOrEmpty(_BlogsModel.MetaKeywordTxt) ? _BlogsModel.MetaKeywordTxt : ""),
                               new XElement("MetaDescriptionTxt", !string.IsNullOrEmpty(_BlogsModel.MetaDescriptionTxt) ? _BlogsModel.MetaDescriptionTxt : ""),
                               new XElement("IsCommentEnabledInd", _BlogsModel.IsCommentEnabledInd),
                               new XElement("SocialMediaTxt", !string.IsNullOrEmpty(_BlogsModel.SocialMediaTxt) ? _BlogsModel.SocialMediaTxt : ""),
                               new XElement("CategoryID", !string.IsNullOrEmpty(_BlogsModel.strCategoryid) ? Convert.ToInt32(_BlogsModel.strCategoryid) : 0),
                               new XElement("AbstractTxt", !string.IsNullOrEmpty(_BlogsModel.AbstractTxt) ? _BlogsModel.AbstractTxt : "")

                               ));

            BlogsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/blogs.xml"));
            #region Tag section

            FormBlogTags _FormBlogTags = new FormBlogTags();
            TagsModel _TagsModel = new TagsModel();
            var _BlogsContexts = new BlogsContexts();
            _FormBlogTags.BlogID = _BlogsModel.BlogID;

            if (_BlogsModel.SelectedTagsID != null)
            {
                foreach (var item in _BlogsModel.SelectedTagsID)
                {
                    foreach (var i in item.Split(','))
                    {
                        var _TagContexts = new TagsContexts();
                        var id = 0;
                        var tagName = string.Empty;
                        int n;
                        var flag = false;
                        bool isNumeric = int.TryParse(i, out n);
                        if (isNumeric)
                        {
                            var formBlogtag_count = _BlogsContexts.GetAllFormBlogTags().Where(x => x.BlogID == _BlogsModel.BlogID && x.TagID == i.ToString()).Count();
                            if (formBlogtag_count == 0)
                            {
                                id = _TagContexts.GetTags().Where(x => x.TagID == Convert.ToInt32(i)).Select(x => x.TagID).FirstOrDefault();
                                tagName = _TagContexts.GetTags().Where(x => x.TagID == Convert.ToInt32(i)).Select(x => x.TagNameTxt).FirstOrDefault();
                                flag = true;
                            }
                        }
                        else
                        {
                            id = 0;
                            tagName = i;
                        }

                        if (id == 0)
                        {
                            _TagsModel.TagNameTxt = tagName;
                            var IsTagName_exist = _TagContexts.GetTags().Where(x => x.TagNameTxt.ToLower().Trim() == tagName.ToLower().Trim()).Count();
                            if (IsTagName_exist == 0)
                            {
                                _TagContexts.AddTags(_TagsModel);// Add tags in tag context
                                id = _TagContexts.GetTags().Select(x => x.TagID).Count() > 0 ? _TagContexts.GetTags().Select(x => x.TagID).Max() + 1 : 1;
                                flag = true;
                            }
                            else
                            {
                                var tagID_exist = _TagContexts.GetTags().Where(x => x.TagNameTxt.ToLower().Trim() == tagName.ToLower().Trim()).Select(x => x.TagID).FirstOrDefault();
                                var formBlogtag_count = _BlogsContexts.GetAllFormBlogTags().Where(x => x.BlogID == _BlogsModel.BlogID && x.TagID == tagID_exist.ToString()).Count();
                                if (formBlogtag_count == 0)
                                {
                                    id = tagID_exist;
                                    flag = true;
                                }
                            }
                        }

                        if (flag == true)
                        {
                            _FormBlogTags.BlogTagID = (int)(from S in BlogsTagsData.Descendants("FormBlogTag") orderby (short)S.Element("BlogTagID") descending select (short)S.Element("BlogTagID")).FirstOrDefault() + 1;
                            BlogsTagsData.Root.Add(new XElement("FormBlogTag", new XElement("BlogTagID", _FormBlogTags.BlogTagID),
                            new XElement("BlogID", _FormBlogTags.BlogID),
                            new XElement("TagID", id)));
                            BlogsTagsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/formblogstag.xml"));
                        }

                    }
                }
            }
            #endregion

            #region social media 
            FormBlogSocialMedia _FormBlogSM = new FormBlogSocialMedia();
            _FormBlogSM.BlogID = _BlogsModel.BlogID;
            if (_BlogsModel.SelectedSocialMedia != null)
            {
                foreach (var item in _BlogsModel.SelectedSocialMedia)
                {
                    _FormBlogSM.BlogSocialMediaID = (int)(from S in BlogsSocialMediaData.Descendants("FormBlogSocialMedia") orderby (short)S.Element("BlogSocialMediaID") descending select (short)S.Element("BlogSocialMediaID")).FirstOrDefault() + 1;
                    BlogsSocialMediaData.Root.Add(new XElement("FormBlogSocialMedia", new XElement("BlogSocialMediaID", _FormBlogSM.BlogSocialMediaID),
                    new XElement("BlogID", _FormBlogSM.BlogID),
                    new XElement("SocialMediaID", item)));
                }
                BlogsSocialMediaData.Save(HttpContext.Current.Server.MapPath("~/App_Data/formblogssocialmedia.xml"));
            }
            #endregion


        }
        /// <summary>
        /// Edit the blog content
        /// </summary>
        /// <param name="_BlogsModel"></param>
        /// <param name="IsEditFromStatusChk">true for edit from status chkbox in listing and false for edit through updation the details</param>
        public void EditBlogs(BlogsModel _BlogsModel, bool IsEditFromStatusChk)
        {
            try
            {
                XElement node = BlogsData.Root.Elements("Blog").Where(i => (int)i.Element("BlogID") == _BlogsModel.BlogID).FirstOrDefault();

                node.SetElementValue("TitleTxt", !string.IsNullOrEmpty(_BlogsModel.TitleTxt) ? _BlogsModel.TitleTxt : "");
                node.SetElementValue("AuthorNameTxt", !string.IsNullOrEmpty(_BlogsModel.AuthorNameID) ? _BlogsModel.AuthorNameID : "");
                node.SetElementValue("BlogDescription", !string.IsNullOrEmpty(_BlogsModel.BlogDescription) ? _BlogsModel.BlogDescription : "");
                node.SetElementValue("ImagePathTxt", !string.IsNullOrEmpty(_BlogsModel.ImagePathTxt) ? _BlogsModel.ImagePathTxt : "");
                node.SetElementValue("PostedDate", !string.IsNullOrEmpty(Convert.ToString(_BlogsModel.PostedDate)) ? _BlogsModel.PostedDate : System.DateTime.Today);
                node.SetElementValue("SlagTxt", !string.IsNullOrEmpty(_BlogsModel.SlagTxt) ? _BlogsModel.SlagTxt : "");
                node.SetElementValue("IsActiveInd", _BlogsModel.IsActiveInd);
                node.SetElementValue("MetaTitleTxt", !string.IsNullOrEmpty(_BlogsModel.MetaTitleTxt) ? _BlogsModel.MetaTitleTxt : "");
                node.SetElementValue("MetaKeywordTxt", !string.IsNullOrEmpty(_BlogsModel.MetaKeywordTxt) ? _BlogsModel.MetaKeywordTxt : "");
                node.SetElementValue("MetaDescriptionTxt", !string.IsNullOrEmpty(_BlogsModel.MetaDescriptionTxt) ? _BlogsModel.MetaDescriptionTxt : "");
                node.SetElementValue("IsCommentEnabledInd", _BlogsModel.IsCommentEnabledInd);
                node.SetElementValue("SocialMediaTxt", !string.IsNullOrEmpty(_BlogsModel.SocialMediaTxt) ? _BlogsModel.SocialMediaTxt : "");
                node.SetElementValue("CategoryID", !string.IsNullOrEmpty(_BlogsModel.strCategoryid) ? Convert.ToInt32(_BlogsModel.strCategoryid) : 0);
                node.SetElementValue("AbstractTxt", !string.IsNullOrEmpty(_BlogsModel.AbstractTxt) ? _BlogsModel.AbstractTxt : "");
                BlogsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/blogs.xml"));
                // Tag section
                #region Tag section
                if (allFormBlogsTags.Where(x => x.BlogID == _BlogsModel.BlogID) != null && IsEditFromStatusChk == false)
                {

                    if (_BlogsModel.SelectedTagsID != null)
                    {
                        DeleteFormBlogTag(_BlogsModel.BlogID);// delete all formblogtag on the basis of blog id
                        FormBlogTags _FormBlogTags = new FormBlogTags();
                        _FormBlogTags.BlogID = _BlogsModel.BlogID;
                        var _TagsModel = new TagsModel();
                        var _TagContexts = new TagsContexts();
                        var _BlogsContexts = new BlogsContexts();

                        foreach (var item in _BlogsModel.SelectedTagsID)
                        {
                            foreach (var i in item.Split(','))
                            {
                                var flag = false;
                                var id = 0;
                                var tagName = string.Empty;
                                int n;
                                bool isNumeric = int.TryParse(i, out n);
                                if (isNumeric)
                                {
                                    var formBlogtag_count = _BlogsContexts.GetAllFormBlogTags().Where(x => x.BlogID == _BlogsModel.BlogID && x.TagID == i.ToString()).Count();
                                    if (formBlogtag_count == 0)
                                    {
                                        id = _TagContexts.GetTags().Where(x => x.TagID == Convert.ToInt32(i)).Select(x => x.TagID).FirstOrDefault();
                                        tagName = _TagContexts.GetTags().Where(x => x.TagID == Convert.ToInt32(i)).Select(x => x.TagNameTxt).FirstOrDefault();
                                        flag = true;
                                    }

                                }
                                else
                                {
                                    id = 0;
                                    tagName = i;
                                }

                                if (id == 0)
                                {
                                    _TagsModel.TagNameTxt = tagName;
                                    var IsTagName_exist = _TagContexts.GetTags().Where(x => x.TagNameTxt.ToLower().Trim() == tagName.ToLower().Trim()).Count();
                                    if (IsTagName_exist == 0)
                                    {
                                        _TagContexts.AddTags(_TagsModel);// Add tags in tag context
                                        id = _TagContexts.GetTags().Select(x => x.TagID).Count() > 0 ? _TagContexts.GetTags().Select(x => x.TagID).Max() + 1 : 1;
                                        flag = true;
                                    }
                                    else
                                    {
                                        var tagID_exist = _TagContexts.GetTags().Where(x => x.TagNameTxt.ToLower().Trim() == tagName.ToLower().Trim()).Select(x => x.TagID).FirstOrDefault();
                                        var formBlogtag_count = _BlogsContexts.GetAllFormBlogTags().Where(x => x.BlogID == _BlogsModel.BlogID && x.TagID == tagID_exist.ToString()).Count();
                                        if (formBlogtag_count == 0)
                                        {
                                            id = tagID_exist;
                                            flag = true;
                                        }
                                    }
                                }

                                if (flag == true)
                                {
                                    _FormBlogTags.BlogTagID = (int)(from S in BlogsTagsData.Descendants("FormBlogTag") orderby (short)S.Element("BlogTagID") descending select (short)S.Element("BlogTagID")).FirstOrDefault() + 1;
                                    BlogsTagsData.Root.Add(new XElement("FormBlogTag", new XElement("BlogTagID", _FormBlogTags.BlogTagID),
                                    new XElement("BlogID", _FormBlogTags.BlogID),
                                    new XElement("TagID", id)));
                                }

                            }
                        }
                        BlogsTagsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/formblogstag.xml"));
                    }
                    else
                    {
                        DeleteFormBlogTag(_BlogsModel.BlogID);// delete all formblogtag on the basis of blog id
                    }
                }
                #endregion

                #region social media 
                if (allFormBlogsSocialMedia.Where(x => x.BlogID == _BlogsModel.BlogID) != null)
                {
                    if (_BlogsModel.SelectedSocialMedia != null)
                    {
                        DeleteFormBlogSocialMedia(_BlogsModel.BlogID);
                        FormBlogSocialMedia _FormBlogSM = new FormBlogSocialMedia();
                        _FormBlogSM.BlogID = _BlogsModel.BlogID;
                        foreach (var item in _BlogsModel.SelectedSocialMedia)
                        {
                            _FormBlogSM.BlogSocialMediaID = (int)(from S in BlogsSocialMediaData.Descendants("FormBlogSocialMedia") orderby (short)S.Element("BlogSocialMediaID") descending select (short)S.Element("BlogSocialMediaID")).FirstOrDefault() + 1;
                            BlogsSocialMediaData.Root.Add(new XElement("FormBlogSocialMedia", new XElement("BlogSocialMediaID", _FormBlogSM.BlogSocialMediaID),
                            new XElement("BlogID", _FormBlogSM.BlogID),
                            new XElement("SocialMediaID", item)));
                        }
                        BlogsSocialMediaData.Save(HttpContext.Current.Server.MapPath("~/App_Data/formblogssocialmedia.xml"));
                    }
                }
                #endregion
            }
            catch (Exception)
            {

                throw new NotImplementedException();
            }
        }

        public void DeleteBlogs(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    BlogsData.Root.Elements("Blog").Where(i => (int)i.Element("BlogID") == id).Remove();

                    BlogsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/blogs.xml"));

                }
                catch (Exception)
                {

                    throw new NotImplementedException();
                }
            }
        }

        public IEnumerable<string> GetFormBlogTags(int BlogID)
        {
            var tag = allFormBlogsTags.Where(x => x.BlogID == BlogID).Select(x => x.TagID).ToList();
            var _tagContexts = new TagsContexts();
            List<string> lst = new List<string>();
            foreach (var t in tag)
            {
                lst.Add(_tagContexts.GetTags().Where(x => x.TagID == Convert.ToInt32(t)).Select(x => x.TagNameTxt).FirstOrDefault());
            }
            return lst;

        }
        public IEnumerable<string> GetFormBlogTagsIDList(int BlogID)
        {
            var tag = allFormBlogsTags.Where(x => x.BlogID == BlogID).Select(x => x.TagID).ToList();
            return tag;

        }
        public IEnumerable<string> GetAllFormBlogTag()
        {
            return allFormBlogsTags.Select(x => x.TagID);
        }
        public IEnumerable<string> GetFormBlogTag(int BlogID)
        {
            return allFormBlogsTags.Where(x => x.BlogID == BlogID).Select(x => x.TagID);
        }
        public IEnumerable<string> GetFormBlogTagList(int TagID)
        {
            return allFormBlogsTags.Where(x => x.TagID == Convert.ToString(TagID)).Select(x => x.TagID);
        }
        public IEnumerable<int> GetFormBlogSocialMedia(int BlogID)
        {
            return allFormBlogsSocialMedia.Where(x => x.BlogID == BlogID).Select(x => x.SocialMedia);
        }
        public IEnumerable<FormBlogSocialMedia> GetFormBlogSocialMedia_List(int BlogID)
        {
            return allFormBlogsSocialMedia.Where(x => x.BlogID == BlogID).ToList();
        }
        public void DeleteFormBlogTag(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    BlogsTagsData.Root.Elements("FormBlogTag").Where(i => (int)i.Element("BlogID") == id).Remove();

                    BlogsTagsData.Save(HttpContext.Current.Server.MapPath("~/App_Data/formblogstag.xml"));

                }
                catch (Exception)
                {

                    throw new NotImplementedException();
                }
            }
        }

        public void DeleteFormBlogSocialMedia(int? id)
        {
            if (id.HasValue)
            {
                try
                {
                    BlogsSocialMediaData.Root.Elements("FormBlogSocialMedia").Where(i => (int)i.Element("BlogID") == id).Remove();

                    BlogsSocialMediaData.Save(HttpContext.Current.Server.MapPath("~/App_Data/formblogsSocialMedia.xml"));

                }
                catch (Exception)
                {
                    throw new NotImplementedException();
                }
            }
        }
        public IEnumerable<FormBlogTags> GetAllFormBlogTags(int tagID)
        {
            return allFormBlogsTags.Where(x => x.TagID == tagID.ToString()).ToList();
        }
        public IEnumerable<FormBlogTags> GetAllFormBlogTags()
        {
            return allFormBlogsTags.ToList();
        }

    }
}