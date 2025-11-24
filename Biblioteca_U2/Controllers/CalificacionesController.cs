using System;
using System.Linq;
using System.Web.Mvc;
using Biblioteca_U2.Models;
using Biblioteca_U2.Filters;
using System.Data.Entity;
using System.Collections.Generic;

namespace Biblioteca_U2.Controllers
{
    public class CalificacionesController : Controller
    {
        private Model1 db = new Model1();

        // ⭐ Ver todas las reseñas de un libro específico
        [AuthorizeUser]
        public ActionResult VerResenas(int idLibro)
        {
            try
            {
                var libro = db.tblibro
                    .Include(l => l.tbgenero)
                    .Include(l => l.tbcarrera)
                    .FirstOrDefault(l => l.id_libro == idLibro);

                if (libro == null)
                {
                    TempData["ErrorMessage"] = "❌ Libro no encontrado.";
                    return RedirectToAction("Catalogo", "Libros");
                }

                // Obtener todas las calificaciones del libro
                var calificaciones = db.tbcalificacion
                    .Include(c => c.tbusuario) // calificador
                    .Include(c => c.tbprestamo)
                    .Where(c => c.tbprestamo.id_libro == idLibro && c.tipo_calificacion == "comunicacion")
                    .OrderByDescending(c => c.fecha_calificacion)
                    .ToList();

                // Calcular promedio
                double promedioCalificacion = 0;
                if (calificaciones.Any())
                {
                    promedioCalificacion = calificaciones.Average(c => c.puntuacion);
                }

                ViewBag.Libro = libro;
                ViewBag.PromedioCalificacion = promedioCalificacion;
                ViewBag.TotalResenas = calificaciones.Count;

                return View(calificaciones);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al cargar reseñas: " + ex.Message;
                return RedirectToAction("Catalogo", "Libros");
            }
        }

        // ⭐ Formulario para calificar un libro después de devolverlo
        [AuthorizeUser]
        public ActionResult CalificarLibro(int idPrestamo)
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);

                var prestamo = db.tbprestamo
                    .Include(p => p.tblibro)
                    .Include(p => p.tblibro.tbgenero)
                    .Include(p => p.tbusuario3) // donador
                    .FirstOrDefault(p => p.id_prestamo == idPrestamo &&
                                        p.id_usuario_prestatario == userId &&
                                        p.estado == "completado");

                if (prestamo == null)
                {
                    TempData["ErrorMessage"] = "❌ Préstamo no encontrado o no completado.";
                    return RedirectToAction("MisPrestamos", "Prestamos");
                }

                // Verificar si ya calificó este libro
                var yaCalificado = db.tbcalificacion
                    .Any(c => c.id_prestamo == idPrestamo &&
                             c.id_usuario_calificador == userId &&
                             c.tipo_calificacion == "libro");

                if (yaCalificado)
                {
                    TempData["ErrorMessage"] = "⚠️ Ya has calificado este libro.";
                    return RedirectToAction("MisPrestamos", "Prestamos");
                }

                ViewBag.Prestamo = prestamo;
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error: " + ex.Message;
                return RedirectToAction("MisPrestamos", "Prestamos");
            }
        }

        // ⭐ Guardar calificación del libro
        [AuthorizeUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarCalificacionLibro(int idPrestamo, int puntuacion, string comentario)
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);

                var prestamo = db.tbprestamo
                    .Include(p => p.tblibro)
                    .Include(p => p.tblibro.tbusuario1) // Incluir el donador del libro
                    .FirstOrDefault(p => p.id_prestamo == idPrestamo &&
                                        p.id_usuario_prestatario == userId &&
                                        p.estado == "completado");

                if (prestamo == null)
                {
                    TempData["ErrorMessage"] = "❌ Préstamo no válido.";
                    return RedirectToAction("MisPrestamos", "Prestamos");
                }

                // Validar puntuación
                if (puntuacion < 1 || puntuacion > 5)
                {
                    TempData["ErrorMessage"] = "❌ La puntuación debe estar entre 1 y 5.";
                    return RedirectToAction("CalificarLibro", new { idPrestamo });
                }

                // Verificar si ya calificó
                var yaCalificado = db.tbcalificacion
                    .Any(c => c.id_prestamo == idPrestamo &&
                             c.id_usuario_calificador == userId &&
                             c.tipo_calificacion == "comunicacion");

                if (yaCalificado)
                {
                    TempData["ErrorMessage"] = "⚠️ Ya has calificado este libro.";
                    return RedirectToAction("MisPrestamos", "Prestamos");
                }

                // Obtener id del donador del libro
                int idDonador = prestamo.tblibro.id_usuario_donador;
                string tituloLibro = prestamo.tblibro.titulo;
                
                if (idDonador == 0 || idDonador < 0)
                {
                    TempData["ErrorMessage"] = "❌ No se puede calificar: libro sin donador registrado.";
                    return RedirectToAction("MisPrestamos", "Prestamos");
                }

                // Verificar que el donador existe
                var donadorExiste = db.tbusuario.Any(u => u.id_usuario == idDonador);
                if (!donadorExiste)
                {
                    TempData["ErrorMessage"] = "❌ El donador del libro no existe en el sistema.";
                    return RedirectToAction("MisPrestamos", "Prestamos");
                }

                // Crear calificación sin cargar relaciones de navegación
                var calificacion = new tbcalificacion
                {
                    id_prestamo = idPrestamo,
                    id_usuario_calificador = userId,
                    id_usuario_calificado = idDonador,
                    puntuacion = puntuacion,
                    comentario = string.IsNullOrWhiteSpace(comentario) ? null : comentario.Trim(),
                    tipo_calificacion = "comunicacion",
                    fecha_calificacion = DateTime.Now
                };

                db.tbcalificacion.Add(calificacion);
                
                // Guardar con manejo de errores específico
                try
                {
                    db.SaveChanges();
                    TempData["SuccessMessage"] = $"✅ ¡Gracias por tu reseña de '{tituloLibro}'! Tu opinión ayuda a otros estudiantes.";
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    string errorMsg = "Errores de validación: ";
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            errorMsg += $"{validationError.PropertyName}: {validationError.ErrorMessage}; ";
                        }
                    }
                    TempData["ErrorMessage"] = "❌ " + errorMsg;
                    return RedirectToAction("MisPrestamos", "Prestamos");
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    var innerMessage = ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message;
                    TempData["ErrorMessage"] = "❌ Error de base de datos: " + innerMessage;
                    return RedirectToAction("MisPrestamos", "Prestamos");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al guardar calificación: " + ex.Message;
            }

            return RedirectToAction("MisPrestamos", "Prestamos");
        }

        // 🏆 Ver mi perfil público (para estudiantes)
        [AuthorizeUser]
        public ActionResult MiPerfil()
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                var usuario = db.tbusuario
                    .Include(u => u.tbcarrera)
                    .FirstOrDefault(u => u.id_usuario == userId);

                if (usuario == null)
                {
                    TempData["ErrorMessage"] = "❌ Usuario no encontrado.";
                    return RedirectToAction("Index", "Home");
                }

                // Estadísticas del usuario
                int librosLeidos = usuario.numero_prestamos_completados ?? 0;
                int librosDonados = db.tblibro.Count(l => l.id_usuario_donador == userId);
                int creditosActuales = usuario.creditos_disponibles ?? 0;

                // Calificaciones recibidas (como donador)
                var calificacionesRecibidas = db.tbcalificacion
                    .Where(c => c.id_usuario_calificado == userId && c.tipo_calificacion == "comunicacion")
                    .ToList();

                double promedioCalificaciones = calificacionesRecibidas.Any() 
                    ? calificacionesRecibidas.Average(c => c.puntuacion) 
                    : 0;

                // Calcular nivel
                string nivel = ObtenerNivelUsuario(librosLeidos);
                string iconoBadge = ObtenerIconoBadge(nivel);
                string colorBadge = ObtenerColorBadge(nivel);

                // Calcular badges desbloqueados
                var badges = CalcularBadges(usuario, librosLeidos, librosDonados);

                // Historial reciente de préstamos
                var prestamosRecientes = db.tbprestamo
                    .Include(p => p.tblibro)
                    .Where(p => p.id_usuario_prestatario == userId)
                    .OrderByDescending(p => p.fecha_entrega)
                    .Take(5)
                    .ToList();

                ViewBag.Usuario = usuario;
                ViewBag.LibrosLeidos = librosLeidos;
                ViewBag.LibrosDonados = librosDonados;
                ViewBag.CreditosActuales = creditosActuales;
                ViewBag.PromedioCalificaciones = promedioCalificaciones;
                ViewBag.TotalCalificaciones = calificacionesRecibidas.Count;
                ViewBag.Nivel = nivel;
                ViewBag.IconoBadge = iconoBadge;
                ViewBag.ColorBadge = colorBadge;
                ViewBag.Badges = badges;
                ViewBag.PrestamosRecientes = prestamosRecientes;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al cargar perfil: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // 🏆 Ranking de usuarios
        [AuthorizeUser]
        public ActionResult Ranking()
        {
            try
            {
                // Top 10 donadores (más libros donados)
                var topDonadores = db.tbusuario
                    .Where(u => u.rol == "estudiante" && u.activo == true)
                    .Select(u => new
                    {
                        Usuario = u,
                        LibrosDonados = db.tblibro.Count(l => l.id_usuario_donador == u.id_usuario)
                    })
                    .OrderByDescending(x => x.LibrosDonados)
                    .Take(10)
                    .ToList();

                // Top 10 lectores (más préstamos completados)
                var topLectores = db.tbusuario
                    .Where(u => u.rol == "estudiante" && u.activo == true)
                    .OrderByDescending(u => u.numero_prestamos_completados)
                    .Take(10)
                    .ToList();

                // Top 10 con mejor reputación (mejor promedio de calificaciones)
                var topReputacion = db.tbusuario
                    .Where(u => u.rol == "estudiante" && u.activo == true)
                    .Select(u => new
                    {
                        Usuario = u,
                        PromedioCalificacion = db.tbcalificacion
                            .Where(c => c.id_usuario_calificado == u.id_usuario && c.tipo_calificacion == "comunicacion")
                            .Select(c => (double?)c.puntuacion)
                            .Average() ?? 0,
                        TotalCalificaciones = db.tbcalificacion
                            .Count(c => c.id_usuario_calificado == u.id_usuario && c.tipo_calificacion == "comunicacion")
                    })
                    .Where(x => x.TotalCalificaciones >= 3) // Mínimo 3 calificaciones
                    .OrderByDescending(x => x.PromedioCalificacion)
                    .Take(10)
                    .ToList();

                ViewBag.TopDonadores = topDonadores;
                ViewBag.TopLectores = topLectores;
                ViewBag.TopReputacion = topReputacion;

                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al cargar ranking: " + ex.Message;
                return View();
            }
        }

        #region Métodos Auxiliares

        // 🏆 Calcular nivel del usuario según libros leídos
        private string ObtenerNivelUsuario(int librosLeidos)
        {
            if (librosLeidos >= 31) return "Maestro Lector";
            if (librosLeidos >= 16) return "Lector Avanzado";
            if (librosLeidos >= 6) return "Lector Aplicado";
            return "Lector Novato";
        }

        // 🎖️ Obtener icono del badge según nivel
        private string ObtenerIconoBadge(string nivel)
        {
            switch (nivel)
            {
                case "Maestro Lector": return "fa-crown";
                case "Lector Avanzado": return "fa-medal";
                case "Lector Aplicado": return "fa-star";
                default: return "fa-book-reader";
            }
        }

        // 🎨 Obtener color del badge según nivel
        private string ObtenerColorBadge(string nivel)
        {
            switch (nivel)
            {
                case "Maestro Lector": return "#FFD700"; // Dorado
                case "Lector Avanzado": return "#C0C0C0"; // Plateado
                case "Lector Aplicado": return "#CD7F32"; // Bronce
                default: return "#6B7280"; // Gris
            }
        }

        // 🎖️ Calcular badges del usuario
        private System.Dynamic.ExpandoObject CalcularBadges(tbusuario usuario, int librosLeidos, int librosDonados)
        {
            dynamic badges = new System.Dynamic.ExpandoObject();
            var badgesDict = (IDictionary<string, object>)badges;

            // Badges de lectura
            badgesDict["PrimerLibro"] = librosLeidos >= 1;
            badgesDict["Lector5"] = librosLeidos >= 5;
            badgesDict["Lector10"] = librosLeidos >= 10;
            badgesDict["Lector25"] = librosLeidos >= 25;
            badgesDict["Lector50"] = librosLeidos >= 50;

            // Badges de donación
            badgesDict["PrimeraDonacion"] = librosDonados >= 1;
            badgesDict["Donador5"] = librosDonados >= 5;
            badgesDict["Donador10"] = librosDonados >= 10;
            badgesDict["DonadorOro"] = librosDonados >= 20;

            // Badges de créditos
            badgesDict["Ahorrador"] = (usuario.creditos_disponibles ?? 0) >= 100;
            badgesDict["Millonario"] = (usuario.creditos_disponibles ?? 0) >= 500;

            // Badges de puntualidad
            int devolucionesPuntuales = db.tbprestamo
                .Where(p => p.id_usuario_prestatario == usuario.id_usuario &&
                           p.estado == "completado" &&
                           p.fecha_devolucion_real <= p.fecha_prevista_devolucion)
                .Count();
            badgesDict["Puntual"] = devolucionesPuntuales >= 10;

            // Badge de veterano (más de 1 año activo)
            badgesDict["Veterano"] = usuario.fecha_registro.HasValue &&
                      (DateTime.Now - usuario.fecha_registro.Value).Days >= 365;

            return badges;
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
