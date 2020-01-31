using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KISD.Areas.BlogAdmin.Models
{
    public class TagsModel
    {
        public TagsModel()
        {
            this.TagID = 0;
            this.TagNameTxt = "";
        }
        public TagsModel(int TagID,string TagNameTxt)
        {
            this.TagID = TagID;
            this.TagNameTxt = TagNameTxt;
        }
        public int TagID { get; set; }
        public string TagNameTxt { get; set; }
    }
}