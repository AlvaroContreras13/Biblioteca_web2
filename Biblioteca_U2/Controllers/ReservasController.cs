using System;
using System.Linq;
using System.Web.Mvc;
using Biblioteca_U2.Models;
using Biblioteca_U2.Filters;
using System.Data.Entity;

namespace Biblioteca_U2.Controllers
{
    public class ReservasController : Controller
    {
        private Model1 db = new Model1();

        #region Funcionalidades para Estudiantes

        // 📌 Reservar un libro no disponible
        [AuthorizeUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReservarLibro(int idLibro)
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                var usuario = db.tbusuario.Find(userId);
                var libro = db.tblibro.Find(idLibro);

                if (libro == null)
                {
                    TempData["ErrorMessage"] = "❌ El libro no existe.";
                    return RedirectToAction("Catalogo", "Libros");
                }

                // Validar que el libro NO esté disponible
                if (libro.disponible == true)
                {
                    TempData["ErrorMessage"] = "⚠️ Este libro está disponible. Puedes solicitarlo directamente.";
                    return RedirectToAction("Catalogo", "Libros");
                }

                // Validar estado de cuenta
                if (usuario.estado_cuenta == "suspendida" || usuario.estado_cuenta == "bloqueada")
                {
                    TempData["ErrorMessage"] = "❌ Tu cuenta está " + usuario.estado_cuenta + ". No puedes hacer reservas.";
                    return RedirectToAction("Catalogo", "Libros");
                }

                // Verificar si ya tiene una reserva activa para este libro
                var reservaExistente = db.tbreserva
                    .FirstOrDefault(r => r.id_libro == idLibro &&
                                        r.id_usuario == userId &&
                                        (r.estado == "activa" || r.estado == "notificada"));

                if (reservaExistente != null)
                {
                    TempData["ErrorMessage"] = "⚠️ Ya tienes una reserva activa para este libro.";
                    return RedirectToAction("MisReservas");
                }

                // Calcular posición en la cola
                int posicionCola = db.tbreserva
                    .Where(r => r.id_libro == idLibro && r.estado == "activa")
                    .Count() + 1;

                // Crear reserva
                var reserva = new tbreserva
                {
                    id_libro = idLibro,
                    id_usuario = userId,
                    estado = "activa",
                    posicion_cola = posicionCola,
                    fecha_reserva = DateTime.Now
                };

                db.tbreserva.Add(reserva);
                db.SaveChanges();

                TempData["SuccessMessage"] = $"✅ Reserva creada. Estás en posición #{posicionCola} de la cola para '{libro.titulo}'. Te notificaremos cuando esté disponible.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al crear reserva: " + ex.Message;
            }

            return RedirectToAction("Catalogo", "Libros");
        }

        // 📋 Ver mis reservas
        [AuthorizeUser]
        public ActionResult MisReservas()
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);

                var reservas = db.tbreserva
                    .Include(r => r.tblibro)
                    .Include(r => r.tblibro.tbgenero)
                    .Where(r => r.id_usuario == userId &&
                               (r.estado == "activa" || r.estado == "notificada"))
                    .OrderBy(r => r.posicion_cola)
                    .ToList();

                return View(reservas);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al cargar reservas: " + ex.Message;
                return View();
            }
        }

        // ❌ Cancelar una reserva
        [AuthorizeUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CancelarReserva(int idReserva)
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                var reserva = db.tbreserva
                    .FirstOrDefault(r => r.id_reserva == idReserva && r.id_usuario == userId);

                if (reserva == null)
                {
                    TempData["ErrorMessage"] = "❌ Reserva no encontrada.";
                    return RedirectToAction("MisReservas");
                }

                if (reserva.estado != "activa" && reserva.estado != "notificada")
                {
                    TempData["ErrorMessage"] = "⚠️ Esta reserva ya fue procesada.";
                    return RedirectToAction("MisReservas");
                }

                // Marcar como cancelada
                reserva.estado = "cancelada";

                // Reajustar posiciones de la cola
                var reservasPosterior = db.tbreserva
                    .Where(r => r.id_libro == reserva.id_libro &&
                               r.posicion_cola > reserva.posicion_cola &&
                               r.estado == "activa")
                    .ToList();

                foreach (var r in reservasPosterior)
                {
                    r.posicion_cola--;
                }

                db.SaveChanges();

                TempData["SuccessMessage"] = "✅ Reserva cancelada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al cancelar reserva: " + ex.Message;
            }

            return RedirectToAction("MisReservas");
        }

        // ✅ Confirmar interés en libro disponible
        [AuthorizeUser]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmarInteresReserva(int idReserva)
        {
            try
            {
                int userId = Convert.ToInt32(Session["UserId"]);
                var reserva = db.tbreserva
                    .Include(r => r.tblibro)
                    .FirstOrDefault(r => r.id_reserva == idReserva && 
                                        r.id_usuario == userId &&
                                        r.estado == "notificada");

                if (reserva == null)
                {
                    TempData["ErrorMessage"] = "❌ Reserva no encontrada o ya fue procesada.";
                    return RedirectToAction("MisReservas");
                }

                // Verificar que no haya expirado
                if (reserva.fecha_expiracion_confirmacion.HasValue &&
                    DateTime.Now > reserva.fecha_expiracion_confirmacion.Value)
                {
                    reserva.estado = "expirada";
                    db.SaveChanges();
                    TempData["ErrorMessage"] = "⚠️ Tu reserva ha expirado. El libro fue asignado al siguiente en la cola.";
                    return RedirectToAction("MisReservas");
                }

                // Crear solicitud de préstamo automáticamente
                var solicitud = new tbsolicitud_prestamo
                {
                    id_libro = reserva.id_libro,
                    id_usuario_solicitante = userId,
                    estado = "pendiente",
                    fecha_solicitud = DateTime.Now
                };

                db.tbsolicitud_prestamo.Add(solicitud);

                // Marcar reserva como completada
                reserva.estado = "completada";
                reserva.confirmada = true;

                db.SaveChanges();

                TempData["SuccessMessage"] = $"✅ Interés confirmado. Se creó una solicitud de préstamo para '{reserva.tblibro.titulo}'. Un administrador la procesará pronto.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al confirmar interés: " + ex.Message;
            }

            return RedirectToAction("MisReservas");
        }

        #endregion

        #region Funcionalidades para Administradores

        // 📋 Ver todas las reservas activas
        [AuthorizeAdmin]
        public ActionResult ReservasActivas()
        {
            try
            {
                var reservas = db.tbreserva
                    .Include(r => r.tblibro)
                    .Include(r => r.tblibro.tbgenero)
                    .Include(r => r.tbusuario)
                    .Where(r => r.estado == "activa" || r.estado == "notificada")
                    .OrderBy(r => r.id_libro)
                    .ThenBy(r => r.posicion_cola)
                    .ToList();

                return View(reservas);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Error al cargar reservas: " + ex.Message;
                return View();
            }
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
