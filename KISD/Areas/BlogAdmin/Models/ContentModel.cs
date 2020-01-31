using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KISD.Areas.BlogAdmin.Models
{
    public class ContentModel
    {
        public ContentModel()
        {
            this.ContentID = 0;
            this.DescriptionTxt = "";
            this.ContentTypeID = 0;
            this.ContentTypeTitle = "";
            this.TitleTxt = "";
            this.AltImgTxt = "";
            this.ImagePathTxt = "";
            this.MetaTitleTxt= "";
            this.MetaDescriptionTxt = "";
        }

        public ContentModel(int ContentID, string DescriptionTxt, int ContentTypeID,string TitleTxt,string AltImgTxt,string ImagePathTxt, string MetaTitleTxt, string MetaDescriptionTxt)
        {
            this.ContentID = ContentID;
            this.DescriptionTxt = DescriptionTxt;
            this.ContentTypeID = ContentTypeID;
            this.TitleTxt = TitleTxt;
            this.AltImgTxt = AltImgTxt;
            this.ImagePathTxt = ImagePathTxt;
            this.MetaTitleTxt = MetaTitleTxt;
            this.MetaDescriptionTxt = MetaDescriptionTxt;
        }
        public int ContentID { get; set; }
        public string DescriptionTxt { get; set; }
        public string MetaTitleTxt { get; set; }
        public string MetaDescriptionTxt { get; set; }
        public int ContentTypeID { get; set; }
        public string ContentTypeTitle { get; set; }
        public string TitleTxt { get; set; }
        public string AltImgTxt { get; set; }
        public string ImagePathTxt { get; set; }
        public List<SettingModel> SettingList { get; internal set; }
    }
    public class ContentTypeModel
    {
        public int ContentTypeID { get; set; }
        public string TitleTxt { get; set; }
        

        public ContentTypeModel()
        {
            this.ContentTypeID = 0;
            this.TitleTxt = "";
        }

        public ContentTypeModel(int ContentTypeID, string TitleTxt)
        {
            this.ContentTypeID = ContentTypeID;
            this.TitleTxt = TitleTxt;
        }
        public enum ContentType : int
        {
            Header = 1,
            Footer = 2,
            BasicSetting=3,
            EmailSetting=4,
            ThemeSetting=5,
        }
    }

    
}