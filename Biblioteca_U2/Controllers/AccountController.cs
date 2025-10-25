using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Biblioteca_U2.Models;

namespace Biblioteca_U2.Controllers
{
    public class AccountController : Controller
    {
        private Model1 db = new Model1();

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Buscar usuario por email y contraseña SIN HASHEAR
                    var usuario = db.tbusuario.FirstOrDefault(u =>
                        u.email == model.Email &&
                        u.contrasena == model.Contrasena && // Comparación directa
                        u.activo == true);

                    if (usuario != null)
                    {
                        if (usuario.estado_cuenta == "bloqueada" || usuario.estado_cuenta == "suspendida")
                        {
                            ModelState.AddModelError("", "Tu cuenta está " + usuario.estado_cuenta + ". Contacta al administrador.");
                            return View(model);
                        }

                        // Crear cookie de autenticación
                        FormsAuthentication.SetAuthCookie(usuario.email, model.RecordarMe);

                        // Guardar datos en sesión
                        Session["UserId"] = usuario.id_usuario;
                        Session["UserName"] = usuario.nombre + " " + usuario.apellido;
                        Session["UserRole"] = usuario.rol;
                        Session["UserCarrera"] = usuario.tbcarrera != null ? usuario.tbcarrera.nombre_carrera : "Sin carrera";

                        // Redirigir según el rol
                        if (usuario.rol == "admin")
                        {
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Email o contraseña incorrectos.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al iniciar sesión: " + ex.Message);
                }
            }

            return View(model);
        }

        // GET: Registro
        public ActionResult Registro()
        {
            try
            {
                ViewBag.Carreras = new SelectList(db.tbcarrera.Where(c => c.activa == true).ToList(), "id_carrera", "nombre_carrera");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error al cargar las carreras: " + ex.Message;
            }

            return View();
        }

        // POST: Registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registro(RegistroViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Validar que el email no exista
                    if (db.tbusuario.Any(u => u.email == model.Email))
                    {
                        ModelState.AddModelError("Email", "Este email ya está registrado.");
                        ViewBag.Carreras = new SelectList(db.tbcarrera.Where(c => c.activa == true).ToList(), "id_carrera", "nombre_carrera");
                        return View(model);
                    }

                    // Validar que el código de identificación no exista
                    if (db.tbusuario.Any(u => u.codigo_identificacion == model.CodigoIdentificacion))
                    {
                        ModelState.AddModelError("CodigoIdentificacion", "Este código de identificación ya está registrado.");
                        ViewBag.Carreras = new SelectList(db.tbcarrera.Where(c => c.activa == true).ToList(), "id_carrera", "nombre_carrera");
                        return View(model);
                    }

                    // Crear nuevo usuario - CONTRASEÑA SIN HASHEAR
                    var nuevoUsuario = new tbusuario
                    {
                        nombre = model.Nombre,
                        apellido = model.Apellido,
                        email = model.Email,
                        contrasena = model.Contrasena, // Guardamos en texto plano
                        codigo_identificacion = model.CodigoIdentificacion,
                        id_carrera = model.IdCarrera,
                        telefono = model.Telefono,
                        rol = "estudiante",
                        estado_cuenta = "activa",
                        activo = true,
                        reputacion_promedio = 5.0m,
                        numero_prestamos_completados = 0,
                        creditos_disponibles = 0,
                        fecha_registro = DateTime.Now
                    };

                    db.tbusuario.Add(nuevoUsuario);
                    db.SaveChanges();

                    TempData["SuccessMessage"] = "Registro exitoso. Ya puedes iniciar sesión.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al registrar el usuario: " + ex.Message);
                }
            }

            ViewBag.Carreras = new SelectList(db.tbcarrera.Where(c => c.activa == true).ToList(), "id_carrera", "nombre_carrera");
            return View(model);
        }

        // GET: Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }

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