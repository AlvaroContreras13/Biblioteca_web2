using System;
using System.Linq;
using System.Web.Mvc;
using Biblioteca_U2.Models;
using Biblioteca_U2.Filters;

namespace Biblioteca_U2.Controllers
{
    [AuthorizeAdmin]
    public class AdminController : Controller
    {
        private readonly Model1 db = new Model1();

        // GET: Admin/Dashboard
        public ActionResult Dashboard()
        {
            ViewBag.AdminName = Session["UserName"];
            ViewBag.UserRole = Session["UserRole"];
            ViewBag.UserId = Session["UserId"];

            try
            {
                // ✅ Total de usuarios (sin filtrar por activo, ya que todos tienen b'1')
                ViewBag.TotalUsuarios = db.tbusuario.Count();

                // ✅ Total de libros
                ViewBag.TotalLibros = db.tblibro.Count();

                // ✅ Total de préstamos activos o pendientes (sin distinción de mayúsculas)
                ViewBag.TotalPrestamos = db.tbprestamo
                    .Where(p => p.estado.ToLower() == "activo" || p.estado.ToLower() == "pendiente")
                    .Count();
            }
            catch (Exception ex)
            {
                ViewBag.TotalUsuarios = 0;
                ViewBag.TotalLibros = 0;
                ViewBag.TotalPrestamos = 0;
                ViewBag.Error = ex.Message;
            }

            return View();
        }
    }
}
