﻿using System.Web;
using System.Web.Mvc;

namespace KISD.Areas.BlogAdmin.Models
{
    public class SessionExpireAttribute : System.Web.Mvc.ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;
            // check  sessions here
            if (HttpContext.Current.User.Identity.IsAuthenticated == false)
            {
                filterContext.Result = new RedirectResult("~/Blog/Login");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

    }
}