using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace KISD.Areas.Blog.Models
{
    public class ContentModel
    {
        public ContentModel()
        {
            BlogID = 0;
            TitleTxt = "";
            AuthorNameTxt = "";
            BlogDescription = "";
            ImagePathTxt = "";
            PostedDate = DateTime.Now;
            SlagTxt = "";
            IsActiveInd = true;
            MetaTitleTxt = "";
            MetaKeywordTxt = "";
            MetaDescriptionTxt = "";
            IsCommentEnabledInd = false;
            SocialMediaTxt = "";
            CategoryID = 0;
        }

        public ContentModel(int BlogID, string TitleTxt, string AuthorNameTxt, string BlogDescription, string ImagePathTxt, DateTime PostedDate, string SlagTxt, bool IsActiveInd, string MetaTitleTxt, string MetaKeywordTxt, string MetaDescriptionTxt, bool IsCommentEnabledInd, string SocialMediaTxt, int CategoryID)
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
        }

        public int BlogID { get; set; }
        public string TitleTxt { get; set; }
        public string AbstractTxt { get; set; }
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
        public string CategoryName { get; set; }
        public string SearchTxt { get; set; }
        public string SearchType { get; set; }
        public SelectList TagList { get; set; }
        //public List<Areas.Blog.Models.categoryList> categoryList { get; set; }
        public List<categoryList> categoryList { get; set; }
        public List<TagNameList> TagNameList { get; set; }
        public List<IGrouping<int, BlogAdmin.Models.BlogsModel>> MonthList { get; set; }
        public int[] SelectedTags { get; set; }
        public IPagedList<ContentModel> PagedBlog { get; set; }
        public bool IsPagingVisible { get; set; }
        public List<BlogAdmin.Models.ContentModel> HeaderList { get; set; }
        public List<BlogAdmin.Models.ContentModel> FooterList { get; set; }
        public List<BlogAdmin.Models.SettingModel> SettingList { get; set; }
        public List<BlogAdmin.Models.CommentModel> CommentList { get; set; }
        public List<BlogAdmin.Models.FormBlogSocialMedia> SocialList { get; set; }

        public int[] monthDay = new int[12] { 31, -1, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        /// <summary>
        /// Gets the count of Years, Months and Days between two specific dates.
        /// </summary>
        /// <param name="frm_date"></param>
        /// <param name="to_date"></param>
        /// <returns></returns>
        public string Get_Year_month_days(DateTime frm_date, DateTime to_date)
        {
            int day, year, month;
            var No_of_days = string.Empty;
            var date_of_comment = frm_date;
            var today = to_date;
            var increment = 0;
            if (date_of_comment.Day > today.Day)
            {
                increment = this.monthDay[date_of_comment.Month - 1];
            }
            if (increment == -1)// Check for leap year
            {
                if (DateTime.IsLeapYear(date_of_comment.Year))
                {
                    increment = 29;
                }
                else
                {
                    increment = 28;
                }
            }
            if (increment != 0)// Get no of days
            {
                day = (today.Day + increment) - date_of_comment.Day;
                increment = 1;
            }
            else
            {
                day = today.Day - date_of_comment.Day;
            }

            if ((date_of_comment.Month + increment) > today.Month)// Get no of months
            {
                month = (today.Month + 12) - (date_of_comment.Month + increment);
                increment = 1;
            }
            else
            {
                month = (today.Month) - (date_of_comment.Month + increment);
                increment = 0;
            }
            year = today.Year - (date_of_comment.Year + increment);// Get no of Years
            return No_of_days = (year == 0 ? "" : (year + " Year, ")) + (month == 0 ? "" : (month + " Month, ")) + (day == 0 ? "Today" : (day == 1 ? day + " day ago" : day + " Days ago"));
        }
    }
    public class categoryList
    {
        public string CategoryNameTxt { get; set; }
        public string CategoryCount { get; set; }
        public int CategoryID { get; set; }
    }
    public class TagNameList
    {
        public string TagNameTxt { get; set; }
        public int TagID { get; set; }
        public int BlogID { get; set; }
    }
}