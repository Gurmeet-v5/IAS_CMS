using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace KISD.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        // GET: Admin/Home
        [Authorize]
        public ActionResult Index()
        {
            db_KISDEntities objContext = new db_KISDEntities();
            string sectionName = string.Empty;
            using (objContext = new db_KISDEntities())
            {
                long userid = Convert.ToInt64(Membership.GetUser().ProviderUserKey);
                var UserRoleID = (short)objContext.UserRoles.Where(x => x.UserID == userid).First().RoleID;
                 sectionName = objContext.Roles.Where(x => x.RoleID == UserRoleID).Select(x => x.RoleNameTxt).FirstOrDefault();                
            }
            ViewBag.SectionName = !string.IsNullOrEmpty(sectionName) ? sectionName : "";
            return View();
        }
    }
}