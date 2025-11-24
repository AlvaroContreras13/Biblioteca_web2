using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Biblioteca_U2.Models;
using Biblioteca_U2.Filters;
using Biblioteca_U2.Services;
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

        // 🤖 Método para analizar portada con IA
        [HttpPost]
        [AuthorizeAdmin]
        public async Task<ActionResult> AnalizarPortada()
        {
            try
            {
                // 1. Obtener archivo
                var file = Request.Files["img"];
                
                if (file == null || file.ContentLength == 0)
                {
                    return Json(new { exito = false, mensaje = "No se recibió ninguna imagen" });
                }

                // 2. Convertir a bytes
                byte[] imgBytes;
                using (var ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    imgBytes = ms.ToArray();
                }

                // 3. Analizar con IA
                var info = await VisionService.AnalizarLibro(imgBytes);

                // 4. Clasificar género
                int idGenero = BuscarGeneroEnBD(info.Categoria);

                // 5. Obtener carreras de BD
                var listaCarreras = db.tbcarrera
                    .Where(c => c.activa == true)
                    .Select(c => c.nombre_carrera)
                    .ToList();

                // 6. Clasificar carrera con IA
                string carreraTexto = await VisionService.ClasificarCarreraDinamico(
                    info.Titulo, 
                    info.Descripcion, 
                    info.Categoria, 
                    listaCarreras
                );

                // 7. Buscar ID de carrera
                int idCarrera = db.tbcarrera
                    .Where(c => c.nombre_carrera == carreraTexto && c.activa == true)
                    .Select(c => c.id_carrera)
                    .FirstOrDefault();

                // 8. Devolver JSON
                return Json(new
                {
                    exito = true,
                    titulo = info.Titulo,
                    autor = info.Autor,
                    ano = info.Anio ?? 0,
                    descripcion = info.Descripcion,
                    id_genero = idGenero,
                    id_carrera = idCarrera
                });
            }
            catch (Exception ex)
            {
                return Json(new { exito = false, mensaje = "Error al analizar: " + ex.Message });
            }
        }

        // 🔍 Método auxiliar para buscar género en BD
        private int BuscarGeneroEnBD(string categoriaIA)
        {
            if (string.IsNullOrEmpty(categoriaIA))
                return 0;

            // Normalizar y buscar coincidencias
            var categoria = categoriaIA.ToLower().Trim();

            var genero = db.tbgenero
                .AsEnumerable()
                .FirstOrDefault(g => 
                    categoria.Contains(g.nombre_genero.ToLower()) ||
                    g.nombre_genero.ToLower().Contains(categoria)
                );

            return genero?.id_genero ?? 0;
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

            // ⭐ Calcular calificación promedio para cada libro
            var librosConCalificacion = libros.Select(libro => new
            {
                Libro = libro,
                PromedioCalificacion = db.tbcalificacion
                    .Where(c => c.tbprestamo.id_libro == libro.id_libro && c.tipo_calificacion == "comunicacion")
                    .Select(c => (double?)c.puntuacion)
                    .Average() ?? 0,
                TotalCalificaciones = db.tbcalificacion
                    .Count(c => c.tbprestamo.id_libro == libro.id_libro && c.tipo_calificacion == "comunicacion")
            }).ToList();

            ViewBag.LibrosConCalificacion = librosConCalificacion;

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
                CreditosUsuario = usuario.creditos_disponibles, // 💰 Agregar créditos
                PaginaActual = pagina,
                TotalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina),
                TotalResultados = totalRegistros
            };

            return View(vm);
        }
    }
}
