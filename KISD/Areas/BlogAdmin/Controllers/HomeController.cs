using System.Web.Mvc;

namespace KISD.Areas.BlogAdmin.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Blog/Home/

        public ActionResult Index()
        {
            return View();
        }

    }
}
