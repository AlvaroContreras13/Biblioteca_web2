using System;
using System.Linq;
using System.Web.Mvc;
using Biblioteca_U2.Models;
using Biblioteca_U2.Filters;
using System.Data.Entity;

namespace Biblioteca_U2.Controllers
{
    public class PrestamosController : Controller
    {
        private Model1 db = new Model1();

        #region Funcionalidades para Estudiantes

        // 📚 Solicitar préstamo de un libro
        [AuthorizeUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SolicitarPrestamo(int idLibro)
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                var usuario = db.tbusuario.Find(userId);
                var libro = db.tblibro.Find(idLibro);

                // Validaciones
                if (libro == null)
                {
                    TempData["ErrorMessage"] = "❌ El libro no existe.";
                    return RedirectToAction("Catalogo", "Libros");
                }

                if (libro.disponible.HasValue && !libro.disponible.Value)
                {
                    TempData["ErrorMessage"] = "⚠️ El libro no está disponible en este momento.";
                    return RedirectToAction("Catalogo", "Libros");
                }

                if (usuario.estado_cuenta == "suspendida" || usuario.estado_cuenta == "bloqueada")
                {
                    TempData["ErrorMessage"] = "❌ Tu cuenta está " + usuario.estado_cuenta + ". No puedes solicitar préstamos.";
                    return RedirectToAction("Catalogo", "Libros");
                }

                // Verificar si ya tiene una solicitud pendiente para este libro
                var solicitudExistente = db.tbsolicitud_prestamo
                    .FirstOrDefault(s => s.id_libro == idLibro &&
                                        s.id_usuario_solicitante == userId &&
                                        s.estado == "pendiente");

                if (solicitudExistente != null)
                {
                    TempData["ErrorMessage"] = "⚠️ Ya tienes una solicitud pendiente para este libro.";
                    return RedirectToAction("Catalogo", "Libros");
                }

                // Crear solicitud
                var solicitud = new tbsolicitud_prestamo
                {
                    id_libro = idLibro,
                    id_usuario_solicitante = userId,
                    estado = "pendiente",
                    fecha_solicitud = DateTime.Now
                };

                db.tbsolicitud_prestamo.Add(solicitud);
                db.SaveChanges();

                TempData["SuccessMessage"] = $"✅ Solicitud de préstamo enviada para '{libro.titulo}'. Un administrador la revisará pronto.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al procesar la solicitud: " + ex.Message;
            }

            return RedirectToAction("Catalogo", "Libros");
        }

        // 📋 Ver mis préstamos activos
        [AuthorizeUser]
        public ActionResult MisPrestamos()
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);

                var prestamos = db.tbprestamo
                    .Include(p => p.tblibro)
                    .Include(p => p.tbusuario3) // donador
                    .Where(p => p.id_usuario_prestatario == userId &&
                               (p.estado == "activo" || p.estado == "retrasado")) // CAMBIO: "Prestado" → "activo"
                    .OrderBy(p => p.fecha_prevista_devolucion)
                    .ToList();

                var solicitudes = db.tbsolicitud_prestamo
                    .Include(s => s.tblibro)
                    .Where(s => s.id_usuario_solicitante == userId && s.estado == "pendiente")
                    .OrderByDescending(s => s.fecha_solicitud)
                    .ToList();

                ViewBag.Solicitudes = solicitudes;
                return View(prestamos);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al cargar préstamos: " + ex.Message;
                return View();
            }
        }

        // 🔄 Solicitar renovación de préstamo
        [AuthorizeUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SolicitarRenovacion(int idPrestamo)
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                var prestamo = db.tbprestamo
                    .Include(p => p.tblibro)
                    .FirstOrDefault(p => p.id_prestamo == idPrestamo &&
                                        p.id_usuario_prestatario == userId);

                if (prestamo == null)
                {
                    TempData["ErrorMessage"] = "❌ Préstamo no encontrado.";
                    return RedirectToAction("MisPrestamos");
                }

                if (prestamo.estado == "Devuelto") // CAMBIADO: "completado" → "Devuelto"
                {
                    TempData["ErrorMessage"] = "⚠️ Este préstamo ya fue completado.";
                    return RedirectToAction("MisPrestamos");
                }

                // Validar límite de renovaciones (máximo 2 renovaciones)
                if (prestamo.numero_renovaciones >= 2)
                {
                    TempData["ErrorMessage"] = "⚠️ Has alcanzado el límite máximo de renovaciones (2).";
                    return RedirectToAction("MisPrestamos");
                }

                // Verificar si hay reservas pendientes para este libro
                var hayReservas = db.tbreserva
                    .Any(r => r.id_libro == prestamo.id_libro && r.estado == "activa");

                if (hayReservas)
                {
                    TempData["ErrorMessage"] = "⚠️ No se puede renovar porque hay usuarios esperando este libro.";
                    return RedirectToAction("MisPrestamos");
                }

                // Realizar renovación (extender 14 días más)
                prestamo.fecha_prevista_devolucion = prestamo.fecha_prevista_devolucion.AddDays(14);
                prestamo.numero_renovaciones = (prestamo.numero_renovaciones ?? 0) + 1;

                // Si estaba retrasado, volver a activo
                if (prestamo.estado == "retrasado")
                {
                    prestamo.estado = "activo"; // CAMBIO: "Prestado" → "activo"
                }

                db.SaveChanges();

                TempData["SuccessMessage"] = $"✅ Préstamo renovado exitosamente. Nueva fecha de devolución: {prestamo.fecha_prevista_devolucion:dd/MM/yyyy}.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al renovar préstamo: " + ex.Message;
            }

            return RedirectToAction("MisPrestamos");
        }

        #endregion

        #region Funcionalidades para Administradores

        // 📋 Ver todas las solicitudes de préstamo
        [AuthorizeAdmin]
        public ActionResult SolicitudesPendientes()
        {
            try
            {
                var solicitudes = db.tbsolicitud_prestamo
                    .Include(s => s.tblibro)
                    .Include(s => s.tblibro.tbgenero)
                    .Include(s => s.tbusuario1) // solicitante
                    .Where(s => s.estado == "pendiente")
                    .OrderBy(s => s.fecha_solicitud)
                    .ToList();

                return View(solicitudes);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al cargar solicitudes: " + ex.Message;
                return View();
            }
        }

        // ✅ Aprobar solicitud de préstamo
        [AuthorizeAdmin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AprobarSolicitud(int idSolicitud)
        {
            try
            {
                int adminId = Convert.ToInt32(Session["UserId"]);
                var solicitud = db.tbsolicitud_prestamo
                    .Include(s => s.tblibro)
                    .Include(s => s.tbusuario1)
                    .FirstOrDefault(s => s.id_solicitud == idSolicitud);

                if (solicitud == null || solicitud.estado != "pendiente")
                {
                    TempData["ErrorMessage"] = "❌ Solicitud no encontrada o ya procesada.";
                    return RedirectToAction("SolicitudesPendientes");
                }

                var libro = solicitud.tblibro;
                var solicitante = solicitud.tbusuario1;

                // Validaciones extra
                if (libro == null || solicitante == null)
                {
                    TempData["ErrorMessage"] = "❌ El libro o el usuario no existen.";
                    return RedirectToAction("SolicitudesPendientes");
                }
                if (db.tbprestamo.Any(p => p.id_libro == libro.id_libro && p.estado != "completado" && p.estado != "Devuelto"))
                {
                    TempData["ErrorMessage"] = "❌ El libro ya está prestado y no se ha devuelto.";
                    return RedirectToAction("SolicitudesPendientes");
                }

                // Validar que el libro esté disponible
                if (libro.disponible.HasValue && !libro.disponible.Value)
                {
                    TempData["ErrorMessage"] = "⚠️ El libro ya no está disponible.";
                    return RedirectToAction("SolicitudesPendientes");
                }

                // Validar estado de cuenta del solicitante
                if (solicitante.estado_cuenta == "suspendida" || solicitante.estado_cuenta == "bloqueada")
                {
                    TempData["ErrorMessage"] = "⚠️ La cuenta del usuario está " + solicitante.estado_cuenta + ".";
                    return RedirectToAction("SolicitudesPendientes");
                }

                // Crear el préstamo
                var prestamo = new tbprestamo
                {
                    id_libro = solicitud.id_libro,
                    id_usuario_prestatario = solicitud.id_usuario_solicitante,
                    id_usuario_donador = libro.id_usuario_donador,
                    id_admin_entrega = adminId,
                    fecha_entrega = DateTime.Now,
                    fecha_prevista_devolucion = DateTime.Now.AddDays(14),
                    estado = "activo", // CAMBIO: "Prestado" → "activo"
                    numero_renovaciones = 0
                };

                db.tbprestamo.Add(prestamo);

                // Actualizar la solicitud
                solicitud.estado = "aceptada";
                solicitud.id_admin_procesador = adminId;
                solicitud.fecha_procesamiento = DateTime.Now;

                // Marcar el libro como no disponible
                libro.disponible = false;

                db.SaveChanges();

                TempData["SuccessMessage"] = $"✅ Solicitud aprobada y préstamo creado. Libro '{libro.titulo}' entregado a {solicitante.nombre} {solicitante.apellido}.";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al aprobar solicitud: " + ex); // Log en consola de Visual Studio
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                TempData["ErrorMessage"] = "❌ Error al aprobar solicitud: " + innerMsg;
            }

            return RedirectToAction("SolicitudesPendientes");
        }

        // ❌ Rechazar solicitud de préstamo
        [AuthorizeAdmin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RechazarSolicitud(int idSolicitud, string motivoRechazo)
        {
            try
            {
                int adminId = Convert.ToInt32(Session["UserId"]);
                var solicitud = db.tbsolicitud_prestamo
                    .Include(s => s.tblibro)
                    .Include(s => s.tbusuario1)
                    .FirstOrDefault(s => s.id_solicitud == idSolicitud);

                if (solicitud == null || solicitud.estado != "pendiente")
                {
                    TempData["ErrorMessage"] = "❌ Solicitud no encontrada o ya procesada.";
                    return RedirectToAction("SolicitudesPendientes");
                }

                if (string.IsNullOrWhiteSpace(motivoRechazo))
                {
                    TempData["ErrorMessage"] = "⚠️ Debe proporcionar un motivo de rechazo.";
                    return RedirectToAction("SolicitudesPendientes");
                }

                solicitud.estado = "rechazada";
                solicitud.id_admin_procesador = adminId;
                solicitud.fecha_procesamiento = DateTime.Now;
                solicitud.motivo_rechazo = motivoRechazo;

                db.SaveChanges();

                TempData["SuccessMessage"] = $"✅ Solicitud rechazada.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al rechazar solicitud: " + ex.Message;
            }

            return RedirectToAction("SolicitudesPendientes");
        }

        // 📚 Ver préstamos activos (todos)
        [AuthorizeAdmin]
        public ActionResult PrestamosActivos()
        {
            try
            {
                var prestamos = db.tbprestamo
                    .Include(p => p.tblibro)
                    .Include(p => p.tbusuario2) // prestatario
                    .Include(p => p.tbusuario3) // donador
                    .Where(p => p.estado == "activo" || p.estado == "retrasado") // CAMBIO: "Prestado" → "activo"
                    .OrderBy(p => p.fecha_prevista_devolucion)
                    .ToList();

                // Actualizar estados de préstamos retrasados
                var hoy = DateTime.Now;
                foreach (var prestamo in prestamos)
                {
                    if (prestamo.fecha_prevista_devolucion < hoy && prestamo.estado == "activo") // CAMBIO: "Prestado" → "activo"
                    {
                        prestamo.estado = "retrasado";
                    }
                }
                db.SaveChanges();

                return View(prestamos);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al cargar préstamos: " + ex.Message;
                return View();
            }
        }

        // 📖 Registrar devolución de libro
        [AuthorizeAdmin]
        public ActionResult RegistrarDevolucion(int idPrestamo)
        {
            try
            {
                var prestamo = db.tbprestamo
                    .Include(p => p.tblibro)
                    .Include(p => p.tbusuario2) // prestatario
                    .Include(p => p.tbusuario3) // donador
                    .FirstOrDefault(p => p.id_prestamo == idPrestamo);

                if (prestamo == null)
                {
                    TempData["ErrorMessage"] = "❌ Préstamo no encontrado.";
                    return RedirectToAction("PrestamosActivos");
                }

                if (prestamo.estado == "Devuelto") // CAMBIADO: "completado" → "Devuelto"
                {
                    TempData["ErrorMessage"] = "⚠️ Este préstamo ya fue completado.";
                    return RedirectToAction("PrestamosActivos");
                }

                // Cargar condiciones para el select
                ViewBag.Condiciones = new SelectList(new[] {
                    new { valor = "Excelente", texto = "Excelente" },
                    new { valor = "Muy bueno", texto = "Muy bueno" },
                    new { valor = "Bueno", texto = "Bueno" },
                    new { valor = "Regular", texto = "Regular" },
                    new { valor = "Dañado", texto = "Dañado" }
                }, "valor", "texto");

                return View(prestamo);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error: " + ex.Message;
                return RedirectToAction("PrestamosActivos");
            }
        }

        // 📖 Procesar devolución
        [AuthorizeAdmin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcesarDevolucion(int idPrestamo, string condicionDevolucion, string descripcionDanio)
        {
            try
            {
                int adminId = Convert.ToInt32(Session["UserId"]);
                var prestamo = db.tbprestamo
                    .Include(p => p.tblibro)
                    .Include(p => p.tbusuario2) // prestatario
                    .FirstOrDefault(p => p.id_prestamo == idPrestamo);

                if (prestamo == null)
                {
                    TempData["ErrorMessage"] = "❌ Préstamo no encontrado.";
                    return RedirectToAction("PrestamosActivos");
                }

                if (prestamo.estado == "Devuelto") // CAMBIADO: "completado" → "Devuelto"
                {
                    TempData["ErrorMessage"] = "⚠️ Este préstamo ya fue completado.";
                    return RedirectToAction("PrestamosActivos");
                }

                // Registrar devolución
                prestamo.fecha_devolucion_real = DateTime.Now;
                prestamo.id_admin_devolucion = adminId;
                prestamo.condicion_devolucion = condicionDevolucion;
                prestamo.descripcion_danio = descripcionDanio;
                prestamo.estado = "completado"; //

                // Actualizar la condición del libro si es necesario
                if (condicionDevolucion == "Dañado" || condicionDevolucion == "Regular")
                {
                    prestamo.tblibro.condicion = condicionDevolucion;
                }

                // Marcar el libro como disponible nuevamente
                prestamo.tblibro.disponible = true;

                // Actualizar número de préstamos completados del usuario
                var usuario = prestamo.tbusuario2;
                usuario.numero_prestamos_completados = (usuario.numero_prestamos_completados ?? 0) + 1;

                db.SaveChanges();

                // Verificar si hay reservas pendientes para notificar
                var primeraReserva = db.tbreserva
                    .Where(r => r.id_libro == prestamo.id_libro && r.estado == "activa")
                    .OrderBy(r => r.posicion_cola)
                    .FirstOrDefault();

                if (primeraReserva != null)
                {
                    primeraReserva.estado = "notificada";
                    primeraReserva.fecha_notificacion = DateTime.Now;
                    primeraReserva.fecha_expiracion_confirmacion = DateTime.Now.AddDays(2);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = $"✅ Devolución registrada. Se ha notificado al siguiente usuario en la cola de reserva.";
                }
                else
                {
                    TempData["SuccessMessage"] = $"✅ Devolución registrada exitosamente. El libro '{prestamo.tblibro.titulo}' está disponible nuevamente.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al registrar devolución: " + ex.Message;
            }

            return RedirectToAction("PrestamosActivos");
        }

        // 📊 Historial de préstamos
        [AuthorizeAdmin]
        public ActionResult HistorialPrestamos(int pagina = 1)
        {
            try
            {
                const int registrosPorPagina = 20;

                var query = db.tbprestamo
                    .Include(p => p.tblibro)
                    .Include(p => p.tbusuario2) // prestatario
                    .Include(p => p.tbusuario3) // donador
                    .OrderByDescending(p => p.fecha_entrega);

                var totalRegistros = query.Count();
                var prestamos = query
                    .Skip((pagina - 1) * registrosPorPagina)
                    .Take(registrosPorPagina)
                    .ToList();

                ViewBag.PaginaActual = pagina;
                ViewBag.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / registrosPorPagina);

                return View(prestamos);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al cargar historial: " + ex.Message;
                return View();
            }
        }

        // 🔄 Renovar préstamo (desde administrador)
        [AuthorizeAdmin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RenovarPrestamo(int idPrestamo)
        {
            try
            {
                var prestamo = db.tbprestamo
                    .Include(p => p.tblibro)
                    .FirstOrDefault(p => p.id_prestamo == idPrestamo);

                if (prestamo == null)
                {
                    TempData["ErrorMessage"] = "❌ Préstamo no encontrado.";
                    return RedirectToAction("PrestamosActivos");
                }

                if (prestamo.estado == "Devuelto") // CAMBIADO: "completado" → "Devuelto"
                {
                    TempData["ErrorMessage"] = "⚠️ Este préstamo ya fue completado.";
                    return RedirectToAction("PrestamosActivos");
                }

                // Validar límite de renovaciones
                if (prestamo.numero_renovaciones >= 2)
                {
                    TempData["ErrorMessage"] = "⚠️ Este préstamo ha alcanzado el límite máximo de renovaciones (2).";
                    return RedirectToAction("PrestamosActivos");
                }

                // Verificar reservas pendientes
                var hayReservas = db.tbreserva
                    .Any(r => r.id_libro == prestamo.id_libro && r.estado == "activa");

                if (hayReservas)
                {
                    TempData["ErrorMessage"] = "⚠️ No se puede renovar porque hay usuarios esperando este libro.";
                    return RedirectToAction("PrestamosActivos");
                }

                // Realizar renovación
                prestamo.fecha_prevista_devolucion = prestamo.fecha_prevista_devolucion.AddDays(14);
                prestamo.numero_renovaciones = (prestamo.numero_renovaciones ?? 0) + 1;

                if (prestamo.estado == "retrasado")
                {
                    prestamo.estado = "activo"; // CAMBIO: "Prestado" → "activo"
                }

                db.SaveChanges();

                TempData["SuccessMessage"] = $"✅ Préstamo renovado. Nueva fecha: {prestamo.fecha_prevista_devolucion:dd/MM/yyyy}.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al renovar: " + ex.Message;
            }

            return RedirectToAction("PrestamosActivos");
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