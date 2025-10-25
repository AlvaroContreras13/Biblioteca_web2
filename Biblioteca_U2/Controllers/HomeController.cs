using System.Web.Mvc;
using Biblioteca_U2.Filters;

namespace Biblioteca_U2.Controllers
{
    [AuthorizeUser]
    public class HomeController : Controller
    {
        // GET: Home/Index
        public ActionResult Index()
        {
            ViewBag.UserName = Session["UserName"];
            ViewBag.UserRole = Session["UserRole"];
            ViewBag.UserId = Session["UserId"];
            ViewBag.UserCarrera = Session["UserCarrera"];
            return View();
        }
    }
}