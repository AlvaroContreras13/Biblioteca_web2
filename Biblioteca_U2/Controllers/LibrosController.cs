using System;
using System.Linq;
using System.Web.Mvc;
using Biblioteca_U2.Models;
using Biblioteca_U2.Filters;
using System.Data.Entity;

namespace Biblioteca_U2.Controllers
{
    public class LibrosController : Controller
    {
        private Model1 db = new Model1();

        // 🔒 Solo para administradores
        [AuthorizeAdmin]
        public ActionResult Registrar()
        {
            CargarCombos();
            return View();
        }

        // 🔒 Solo para administradores
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeAdmin]
        public ActionResult Registrar(tblibro libro)
        {
            ModelState.Remove("codigo_referencia");
            ModelState.Remove("id_admin_registrador");

            try
            {
                if (Session["UserId"] == null)
                    return RedirectToAction("Login", "Account");

                if (ModelState.IsValid)
                {
                    libro.id_libro = db.tblibro.Any() ? db.tblibro.Max(l => l.id_libro) + 1 : 1;
                    string codigo = "LB-" + (db.tblibro.Count() + 1).ToString("D5");
                    libro.codigo_referencia = codigo;
                    libro.fecha_donacion = DateTime.Now;
                    libro.disponible = true;
                    libro.id_admin_registrador = Convert.ToInt32(Session["UserId"]);

                    db.tblibro.Add(libro);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = $"📚 Libro '{libro.titulo}' registrado correctamente con código {codigo}.";
                    return RedirectToAction("Registrar");
                }
                else
                {
                    TempData["ErrorMessage"] = "⚠️ Verifica los campos del formulario.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al registrar el libro: " + ex.Message;
            }

            CargarCombos();
            return View(libro);
        }

        private void CargarCombos()
        {
            ViewBag.Usuarios = new SelectList(
                db.tbusuario
                    .Where(u => u.rol == "estudiante" && u.activo == true)
                    .Select(u => new { u.id_usuario, Nombre = u.nombre + " " + u.apellido }),
                "id_usuario", "Nombre"
            );

            ViewBag.Carreras = new SelectList(
                db.tbcarrera.Where(c => c.activa == true),
                "id_carrera", "nombre_carrera"
            );

            ViewBag.Generos = new SelectList(
                db.tbgenero.ToList(),
                "id_genero", "nombre_genero"
            );
        }

        // 👥 Solo para usuarios logueados (estudiantes)
        [AuthorizeUser]
        public ActionResult Catalogo(string busqueda, int? idGenero, int? idCarrera, bool? disponible, int pagina = 1)
        {
            const int registrosPorPagina = 8;

            var query = db.tblibro
                .Include(l => l.tbgenero)
                .Include(l => l.tbcarrera)
                .Include(l => l.tbusuario1)
                .AsQueryable();

            if (!string.IsNullOrEmpty(busqueda))
                query = query.Where(l => l.titulo.Contains(busqueda) || l.autor.Contains(busqueda));
            if (idGenero.HasValue)
                query = query.Where(l => l.id_genero == idGenero.Value);
            if (idCarrera.HasValue)
                query = query.Where(l => l.id_carrera == idCarrera.Value);
            if (disponible.HasValue)
                query = query.Where(l => l.disponible == disponible.Value);

            var totalRegistros = query.Count();
            var libros = query.OrderBy(l => l.titulo)
                              .Skip((pagina - 1) * registrosPorPagina)
                              .Take(registrosPorPagina)
                              .ToList();

            int userId = Convert.ToInt32(Session["UserId"]);
            var usuario = db.tbusuario.Find(userId);

            var vm = new CatalogoViewModel
            {
                Busqueda = busqueda,
                IdGenero = idGenero,
                IdCarrera = idCarrera,
                Disponible = disponible,
                Generos = new SelectList(db.tbgenero.ToList(), "id_genero", "nombre_genero"),
                Carreras = new SelectList(db.tbcarrera.ToList(), "id_carrera", "nombre_carrera"),
                Libros = libros,
                UsuarioNombre = $"{usuario.nombre} {usuario.apellido}",
                PaginaActual = pagina,
                TotalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina),
                TotalResultados = totalRegistros
            };

            return View(vm);
        }
    }
}
