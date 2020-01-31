using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KISD.Areas.Admin.Models
{
    public class SysytemAccessLogModel
    {
        public long SystemAccessLogID { get; set; }
        public string NameTxt { get; set; }
        public string UserNameTxt { get; set; }
        public Nullable<short> UserRoleID { get; set; }
        public Nullable<System.DateTime> LoginDateTime { get; set; }
        public Nullable<System.DateTime> LogoutDateTime { get; set; }

        public virtual Role Role { get; set; }
    }
    public class SysytemAccessLogService
    {
        private db_KISDEntities _context;
        public SysytemAccessLogService()
        {
            _context = new db_KISDEntities();
        }

        /// <summary>
        /// IQueryable<ImageModel> Method for Getting the System Access Log Model
        /// And Query the From database by calling the GetSystemAccessLog() Method.
        /// </summary>
        /// <returns>GetSystemAccessLogModel</returns>
        public IQueryable<SysytemAccessLogModel> GetSystemAccessLogView()
        {
            var query = from x in GetSystemAccessLog()
                        select new SysytemAccessLogModel
                        {
                            SystemAccessLogID = x.SystemAccessLogID,
                            LoginDateTime = x.LoginDateTime,
                            LogoutDateTime = x.LogoutDateTime,
                            UserNameTxt = x.UserNameTxt,
                            UserRoleID = x.UserRoleID,
                            NameTxt = x.NameTxt
                        
                        };
            return query.AsQueryable();
        }

        /// <summary>
        /// IQueryable<ParrieHaynesRanch.Image> get the System Access log Data.
        /// </summary>
        /// <returns>SystemAccessLogObject</returns>
        public IQueryable<SystemAccessLog> GetSystemAccessLog()
        {
            return _context.SystemAccessLogs;
        }

      
    }
}