using KISD.Areas.Admin.Models;
using System;
using System.Web.Mvc;
using ModuleTypeAlias = KISD.Areas.Admin.Models.Common.ModuleType;

namespace KISD.Areas.Admin.Controllers
{
    public class DetailsController : Controller
    {
        public ActionResult Index(string mID)
        {
            var ID = EncryptDecrypt.Decrypt(mID);
            if (ID == Convert.ToInt32(ModuleTypeAlias.Images).ToString())
            {
                ViewBag.DashboardTitle = "Image Dashboard";
            }
            else if (ID == Convert.ToInt32(ModuleTypeAlias.Email).ToString())
            {
                ViewBag.DashboardTitle = "Email Dashboard";
            }
            else if (ID == Convert.ToInt32(ModuleTypeAlias.AboutKISD).ToString())
            {
                ViewBag.DashboardTitle = "About KISD Dashboard";
            }
            else if (ID == Convert.ToInt32(ModuleTypeAlias.Departments).ToString())
            {
                ViewBag.DashboardTitle = "Department Dashboard";
            }
            else if (ID == Convert.ToInt32(ModuleTypeAlias.Employment).ToString())
            {
                ViewBag.DashboardTitle = "Employment Dashboard";
            }
            else if (ID == Convert.ToInt32(ModuleTypeAlias.Home).ToString())
            {
                ViewBag.DashboardTitle = "Home Dashboard";
            }
            else if (ID == Convert.ToInt32(ModuleTypeAlias.Masters).ToString())
            {
                ViewBag.DashboardTitle = "Masters Dashboard";
            }
            else if (ID == Convert.ToInt32(ModuleTypeAlias.NewToKISD).ToString())
            {
                ViewBag.DashboardTitle = "New To KISD Dashboard";
            }
            else if (ID == Convert.ToInt32(ModuleTypeAlias.ParentStudents).ToString())
            {
                ViewBag.DashboardTitle = "Parent & Students Dashboard";
            }
            else if (ID == Convert.ToInt32(ModuleTypeAlias.Reports).ToString())
            {
                ViewBag.DashboardTitle = "Reports Dashboard";
            }
            else if (ID == Convert.ToInt32(ModuleTypeAlias.School).ToString())
            {
                ViewBag.DashboardTitle = "School Dashboard";
            }
            else if (ID == Convert.ToInt32(ModuleTypeAlias.SchoolBoard).ToString())
            {
                ViewBag.DashboardTitle = "School Board Dashboard";
            }
            else if (ID == Convert.ToInt32(ModuleTypeAlias.Users).ToString())
            {
                ViewBag.DashboardTitle = "Users Dashboard";
            }
           
            ViewBag.ID = ID;
            return View();
        }
    }
}