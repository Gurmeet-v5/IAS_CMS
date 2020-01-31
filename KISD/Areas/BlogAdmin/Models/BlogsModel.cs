using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KISD.Areas.BlogAdmin.Models
{
    public class BlogsModel
    {
        public BlogsModel()
        {
            this.BlogID = 0;
            this.TitleTxt = "";
            this.AuthorNameTxt = "";
            this.BlogDescription = "";
            this.ImagePathTxt = "";
            this.PostedDate = DateTime.Now;
            this.SlagTxt = "";
            this.IsActiveInd = true;
            this.MetaTitleTxt = "";
            this.MetaKeywordTxt = "";
            this.MetaDescriptionTxt = "";
            this.IsCommentEnabledInd = false;
            this.SocialMediaTxt = "";
            this.CategoryID = 0;
            this.AbstractTxt = "";
        }

        public BlogsModel(int BlogID,string TitleTxt, string AuthorNameTxt,string BlogDescription, string ImagePathTxt,DateTime PostedDate,string SlagTxt,bool IsActiveInd,string MetaTitleTxt,string MetaKeywordTxt,string MetaDescriptionTxt,bool IsCommentEnabledInd,string SocialMediaTxt, int CategoryID,string AbstractTxt)
        {
            this.BlogID = BlogID;
            this.TitleTxt = TitleTxt;
            this.AuthorNameTxt = AuthorNameTxt;
            this.BlogDescription = BlogDescription;
            this.ImagePathTxt = ImagePathTxt;
            this.PostedDate = PostedDate;
            this.SlagTxt = SlagTxt;
            this.IsActiveInd = IsActiveInd;
            this.MetaTitleTxt = MetaTitleTxt;
            this.MetaKeywordTxt = MetaKeywordTxt;
            this.MetaDescriptionTxt = MetaDescriptionTxt;
            this.IsCommentEnabledInd = IsCommentEnabledInd;
            this.SocialMediaTxt = SocialMediaTxt;
            this.CategoryID = CategoryID;
            this.AbstractTxt = AbstractTxt;
        }

        public int BlogID { get; set; }
        public string TitleTxt { get; set; }
        public string AuthorNameTxt { get; set; }
        public string AuthorNameID { get; set; }
        public string BlogDescription { get; set; }
        public string ImagePathTxt { get; set; }
        public DateTime PostedDate { get; set; }
        public string SlagTxt { get; set; }
        public bool IsActiveInd { get; set; }
        public string MetaTitleTxt { get; set; }
        public string MetaKeywordTxt { get; set; }
        public string MetaDescriptionTxt { get; set; }
        public bool IsCommentEnabledInd { get; set; }
        public string SocialMediaTxt { get; set; }
        
        public int CategoryID { get; set; }
        public string strCategoryid { get; set; }
        public System.Web.Mvc.SelectList TagList { get; set; }
        public string[] SelectedTags { get; set; }
        public string[] SelectedTagsID { get; set; }
        public System.Web.Mvc.SelectList SocialMediaList { get; set; }
        public string[] SelectedSocialMedia { get; set; }

        public string AbstractTxt { get; set; }

    }

    public class FormBlogTags
    {
        
        public FormBlogTags()
        {
            this.BlogTagID = 0;
            this.BlogID = 0;
            this.TagID = "";
        }
        public FormBlogTags(int BlogTagID,int BlogID,string TagID)
        {
            var _TagContexts=new Contexts.TagsContexts();
            this.BlogTagID = BlogTagID;
            this.BlogID = BlogID;
            this.TagID = TagID;
            this.TagModel = _TagContexts.GetTags().AsEnumerable();
        }
        public int BlogTagID { get; set; }
        public int BlogID { get; set; }
        public string TagID { get; set; }
        public IEnumerable<TagsModel> TagModel { get; set; }
    }

    public class FormBlogSocialMedia
    {
        public FormBlogSocialMedia()
        {
            this.BlogSocialMediaID = 0;
            this.BlogID = 0;
            this.SocialMedia = 0;
        }
        public FormBlogSocialMedia(int BlogSocialMediaID, int BlogID, int SocialMedia)
        {
            this.BlogSocialMediaID = BlogSocialMediaID;
            this.BlogID = BlogID;
            this.SocialMedia = SocialMedia;
        }
        public int BlogSocialMediaID { get; set; }
        public int BlogID { get; set; }
        public int SocialMedia { get; set; }
    }
}