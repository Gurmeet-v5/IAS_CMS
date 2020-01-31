using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace KISD.Areas.BlogAdmin.Models
{
    public class SettingModel
    {
        public SettingModel()
        {
            this.SettingID = 0;
            this.PostPerPage = 0;
            this.IsSearchEnabled = true;
            this.ThemesPath = "";
            this.SendingEmail = "";
            this.ReceivingEmail = "";
            this.CommentPerPost = 0;
            this.PagingColor = "";
            this.PagingActiveColor = "";
            this.PagingHoverColor = "";
            this.ButtonColor = "";
            this.NevigationBarColor = "";
            this.NevigationBarFontFamily = "";
            this.NevigationBarFontFamilyTxt = "";
            this.NevigationBarHoverColor = "";
            this.NevigationBarFontColor = "";
            this.NevigationBarTextFontSize = "";
            this.NevigationBarTextFontStyle = "";
            this.FooterColor = "";
            this.SidebarTitleBackgroundColor = "";
            this.SidebarTitleFontFamily = "";
            this.SidebarTitleFontFamilyTxt = "";
            this.SidebarTitleFontcolor = "";
            this.SidebarTitleFontSize = "";
            this.PostTitleFontColor = "";
            this.PostTitleFontFamily = "";
            this.PostTitleFontFamilyTxt = "";
            this.PostTitleFontSize = "";
            this.IsCommentEnabled = true;
            this.IsSocialSharingEnabled = true;
        }
        public SettingModel(int SettingID,int PostPerPage, bool IsSearchEnabled, string ThemesPath, string SendingEmail, string ReceivingEmail, 
            int CommentPerPost,string PagingColor, string PagingActiveColor, string PagingHoverColor, string ButtonColor, string NevigationBarColor, 
            string NevigationBarFontFamily, string NevigationBarFontFamilyTxt, string NevigationBarHoverColor, string NevigationBarFontColor, string NevigationBarTextFontSize, 
            string NevigationBarTextFontStyle, string FooterColor, string SidebarTitleBackgroundColor, string SidebarTitleFontFamily, string SidebarTitleFontFamilyTxt, string SidebarTitleFontcolor,
            string SidebarTitleFontSize, string PostTitleFontColor, string PostTitleFontFamily, string PostTitleFontFamilyTxt,
            string  PostTitleFontSize, bool IsCommentEnabled, bool IsSocialSharingEnabled)
        {
            this.SettingID = SettingID;
            this.PostPerPage = PostPerPage;
            this.IsSearchEnabled = IsSearchEnabled;
            this.ThemesPath = ThemesPath;
            this.SendingEmail = SendingEmail;
            this.ReceivingEmail = ReceivingEmail;
            this.CommentPerPost = CommentPerPost;
            this.PagingColor = PagingColor;
            this.PagingActiveColor = PagingActiveColor;
            this.PagingHoverColor = PagingHoverColor;
            this.ButtonColor = ButtonColor;
            this.NevigationBarColor = NevigationBarColor;
            this.NevigationBarFontFamily = NevigationBarFontFamily;
            this.NevigationBarFontFamilyTxt = NevigationBarFontFamilyTxt;
            this.NevigationBarHoverColor = NevigationBarHoverColor;
            this.NevigationBarFontColor = NevigationBarFontColor;
            this.NevigationBarTextFontSize = NevigationBarTextFontSize;
            this.NevigationBarTextFontStyle = NevigationBarTextFontStyle;
            this.FooterColor = FooterColor;
            this.SidebarTitleBackgroundColor = SidebarTitleBackgroundColor;
            this.SidebarTitleFontFamily = SidebarTitleFontFamily;
            this.SidebarTitleFontFamilyTxt = SidebarTitleFontFamilyTxt;
            this.SidebarTitleFontcolor = SidebarTitleFontcolor;
            this.SidebarTitleFontSize = SidebarTitleFontSize;
            this.PostTitleFontColor = PostTitleFontColor;
            this.PostTitleFontFamily = PostTitleFontFamily;
            this.PostTitleFontFamilyTxt = PostTitleFontFamilyTxt;
            this.PostTitleFontSize = PostTitleFontSize;
            this.IsCommentEnabled = IsCommentEnabled;
            this.IsSocialSharingEnabled = IsSocialSharingEnabled;
        }
        public int SettingID { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public int PostPerPage { get; set; }
        public bool IsSearchEnabled { get; set; }
        public bool IsSocialSharingEnabled { get; set; }
        public bool IsCommentEnabled { get; set; }

        public string ThemesPath { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public string SendingEmail { get; set; }
        public System.Web.Mvc.SelectList ReceivingEmailList { get; set; }
        public string[] SelectedReceivingEmail { get; set; }
        public string ReceivingEmail { get; set; }
        [Required(ErrorMessage = "This field is required.")]
        public int CommentPerPost { get; set; }

        public string PagingColor { get; set; }
        public string PagingHoverColor { get; set; }
        public string PagingActiveColor { get; set; }
        public string ButtonColor { get; set; }
        public string NevigationBarColor { get; set; }
        public string NevigationBarFontColor { get; set; }
        public string NevigationBarHoverColor { get; set; }
        public string NevigationBarTextFontStyle { get; set; }
        public string NevigationBarTextFontSize { get; set; }
        public string NevigationBarFontFamily { get; set; }
        public IEnumerable<SelectList>NevigationBarTextFontSizet { get; set; }
        public string NevigationBarFontFamilyTxt { get; set; }
        public string FooterColor { get; set; }
      
        public string PostTitleFontFamilyTxt { get; set; }
        public string SidebarTitleBackgroundColor { get; set; }
        
        public string SidebarTitleFontFamily { get; set; }
        public string SidebarTitleFontFamilyTxt { get; set; }
        //***************
        public string PostTitleFontSize { get; set; }
        public string PostTitleFontFamily { get; set; }
        public string PostTitleFontColor { get; set; }
        public string SidebarTitleFontSize { get; set; }
        public string SidebarTitleFontcolor { get; set; }
        
        public Int64 FromEmail { get; set; }

        public System.Web.Mvc.SelectList ToEmailList { get; set; }
        public string[] SelectedToEmails { get; set; }
        public int ContentType { get; set; }

        /// <summary>
        /// Gets Font Size List
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> GetFontList(string value)
        {
            int[] fontSize = new int[21] { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 };

            IList<SelectListItem> items = new List<SelectListItem>();

            SelectListItem data = new SelectListItem();

            //IList<SelectListItem> items = new List<SelectListItem>
            //{ 
            //    new SelectListItem{Text = "United States", Value = "A"},
               
            //};
            foreach (var item in fontSize)
            {
                data = new SelectListItem();
                data.Text = item.ToString();
                data.Value = item.ToString();
                items.Add(data);

            }
            //data.Text = "Active";
            //data.Value = "true";
            //items.Add(data);
            //data = new SelectListItem();
            //data.Text = "Inactive";
            //data.Value = "false";
            //items.Add(data);
            if (!string.IsNullOrEmpty(value))
            {
                items.Where(x => x.Value == value).FirstOrDefault().Selected = true;
            }
            return items;
        }
    }
}