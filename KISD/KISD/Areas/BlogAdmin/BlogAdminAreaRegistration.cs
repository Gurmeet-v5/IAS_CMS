using System.Web.Mvc;

namespace KISD.Areas.BlogAdmin
{
    public class BlogAdminAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "BlogAdmin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "BlogAdmin_default",
                "BlogAdmin/{controller}/{action}/{id}",
              new { controller = "Account", action = "Login", id = UrlParameter.Optional },
              namespaces: new[] { "KISD.Areas.BlogAdmin.Controllers" }
            );
        }
    }
}