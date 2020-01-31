using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KISD.Areas.Admin.Models
{
    public class SystemChangeLogDetailsModel
    {
        public long ChangeLogDetailID { get; set; }
        public long ChangeLogID { get; set; }
        public string FieldNameTxt { get; set; }
        public string OldValueTxt { get; set; }
        public string NewValueTxt { get; set; }

    }
}