using System.Web.Mvc;
using Biblioteca_U2.Filters;

namespace Biblioteca_U2.Controllers
{
    [AuthorizeAdmin]
    public class AdminController : Controller
    {
        // GET: Admin/Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.AdminName = Session["UserName"];
            ViewBag.UserRole = Session["UserRole"];
            ViewBag.UserId = Session["UserId"];
            return View();
        }
    }
}