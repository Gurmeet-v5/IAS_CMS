using System;

namespace KISD.Areas.BlogAdmin.Models
{
    public class CommentModel
    {
        public CommentModel()
        {
            this.CommentID = 0;
            this.BlogID = 0;
            this.FullNameTxt = "";
            this.EmailTxt = "";
            this.PhoneNoTxt = "";
            this.CommentDescriptionTxt = "";
            this.PostedDate = DateTime.Now;
            this.IsActiveInd = false;
            this.BlogTitle = "";
        }
        public CommentModel(int CommentID,int BlogID, string FullNameTxt, string EmailTxt, string PhoneNoTxt, string CommentDescriptionTxt, DateTime PostedDate, bool IsActiveInd)
        {
            this.CommentID = CommentID;
            this.BlogID = BlogID;
            this.FullNameTxt = FullNameTxt;
            this.EmailTxt = EmailTxt;
            this.PhoneNoTxt = PhoneNoTxt;
            this.CommentDescriptionTxt = CommentDescriptionTxt;
            this.PostedDate = PostedDate;
            this.IsActiveInd = IsActiveInd;
        }
        public int CommentID { get; set; }
        public int BlogID { get; set; }
        public string FullNameTxt { get; set; }
        public string EmailTxt { get; set; }
        public string PhoneNoTxt { get; set; }
        public string CommentDescriptionTxt { get; set; }
        public DateTime PostedDate { get; set; }
        public bool IsActiveInd { get; set; }
        public string BlogTitle { get; set; }
        public string No_of_days { get; set; }
    }
}