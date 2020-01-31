using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KISD.Areas.Admin.Models
{
    public class ManageRightSectionModel
    {
        public long ContentID { get; set; }
        public string ParentID { get; set; }
        public long ContentTypeID { get; set; }
        public string RightSectionTitleTxt { get; set; }
        public string RightSectionAbstractTxt { get; set; }
        public string[] SelectedRightSections { get; set; }
        public SelectList RightSections { get; set; }
        public string MenuTypeID { get; set; }
        public string SubMenuTypeID { get; set; }
        public string TypeMasterID { get; set; }
        public bool IsFromMenu { get; set; }
    }
}