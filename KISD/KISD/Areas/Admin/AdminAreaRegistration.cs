using System.Web.Mvc;

namespace KISD.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                 new { action = "Login", Controller = "Account", id = UrlParameter.Optional },
                namespaces: new[] { "KISD.Areas.Admin.Controllers" }
            );
        }
    }
}