using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KISD.Areas.Admin.Models
{
    public class ReportsModel
    {
        public enum ReportType : int
        {
            SystemAccessLogReport = 1,
            SystemChangeLogReport = 2
        }

        public long ReportTypeID { get; set; }
        public long ChangeLogID { get; set; }
        public long SystemAccessLogID { get; set; }
        public string NameTxt { get; set; }
        public string UserNameTxt { get; set; }
        public string RoleNameTxt { get; set; }
        public long UserRoleID { get; set; }
        public string ModuleNameTxt { get; set; }
        public string NotesTxt { get; set; }
        public string LogTypeTxt { get; set; }
        public DateTime LogDateTime { get; set; }
        public DateTime LoginDateTime { get; set; }
        public DateTime LogoutDateTime { get; set; }
        public string TotalHours { get; set; }
    }

    public class ReportService
    {
        private db_KISDEntities _context;
        public ReportService()
        {
            _context = new db_KISDEntities();
        }
        public List<ReportsModel> GetReportData(int ReportTypeID)
        {
            var list = new List<ReportsModel>();

            if (ReportTypeID == Convert.ToInt32(ReportsModel.ReportType.SystemAccessLogReport))
            {
                foreach (var x in GetAllSystemAccessLogs())
                {
                    list.Add(new ReportsModel
                    {
                        NameTxt = x.NameTxt,
                        UserNameTxt = x.UserNameTxt,
                        UserRoleID = x.UserRoleID.HasValue ? x.UserRoleID.Value : 0,
                        RoleNameTxt = GetRoleName(x.UserRoleID.Value),
                        LoginDateTime = x.LoginDateTime.HasValue ? x.LoginDateTime.Value : DateTime.Now,
                        LogoutDateTime = x.LogoutDateTime.HasValue ? x.LogoutDateTime.Value : DateTime.Now,
                        TotalHours = (x.LogoutDateTime.Value - x.LoginDateTime.Value).Hours + "hr " + (x.LogoutDateTime.Value - x.LoginDateTime.Value).Minutes + "min"
                    });
                }
            }

            if (ReportTypeID == Convert.ToInt32(ReportsModel.ReportType.SystemChangeLogReport))
            {
                foreach (var x in GetAllSystemChangeLogs())
                {
                    list.Add(new ReportsModel
                    {
                        ChangeLogID = x.ChangeLogID,
                        NameTxt = x.NameTxt,
                        UserNameTxt = x.UsernameTxt,
                        UserRoleID = x.UserRoleID.HasValue ? x.UserRoleID.Value : 0,
                        RoleNameTxt = GetRoleName(x.UserRoleID.Value),
                        ModuleNameTxt = x.ModuleTxt,
                        LogTypeTxt = x.LogTypeTxt,
                        LogDateTime = x.LogDateTime.HasValue ? x.LogDateTime.Value : DateTime.Now
                    });
                }
            }

            return list;
        }

        public string GetRoleName(short RoleID)
        {
            return _context.Roles.Where(x => x.RoleID == RoleID).Select(x => x.RoleNameTxt).FirstOrDefault();
        }

        public IQueryable<SystemAccessLog> GetAllSystemAccessLogs()
        {
            return _context.SystemAccessLogs;
        }

        public IQueryable<SystemChangeLog> GetAllSystemChangeLogs()
        {
            return _context.SystemChangeLogs;
        }
    }
}