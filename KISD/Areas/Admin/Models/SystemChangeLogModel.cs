using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KISD.Areas.Admin.Models
{
    public class SystemChangeLogModel
    {
        public long ChangeLogID { get; set; }
        public string NameTxt { get; set; }
        public string UsernameTxt { get; set; }
        public Nullable<short> UserRoleID { get; set; }
        public string ModuleTxt { get; set; }
        public string LogTypeTxt { get; set; }
        public string NotesTxt { get; set; }
        public Nullable<System.DateTime> LogDateTime { get; set; }

    }
}