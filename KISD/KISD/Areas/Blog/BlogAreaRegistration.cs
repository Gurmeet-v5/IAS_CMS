using System.Web.Mvc;

namespace KISD.Areas.Blog
{
    public class BlogAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Blog";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
              name: "BlogContent",
              url: "blog/{Blogurl}",
              defaults: new { controller = "Content", action = "BlogContent" },
              namespaces: new[] { "KISD.Areas.Blog.Controllers" }
              );

            context.MapRoute(
                "Blog_default",
                "Blog/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}