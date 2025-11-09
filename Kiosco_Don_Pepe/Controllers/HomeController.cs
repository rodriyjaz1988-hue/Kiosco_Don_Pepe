using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kiosco_Don_Pepe.Controllers
{
    public class HomeController : Controller
    {
        private SistemaFacturacionEntities db = new SistemaFacturacionEntities();

        // Vista del Login
        public ActionResult Login()
        {
            return View();
        }

        // Procesa el Login
        [HttpPost]
        public ActionResult Login(string Email, string Password)
        {
            var user = db.Usuarios
                .FirstOrDefault(u => u.Email == Email && u.Password == Password);

            if (user == null)
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

            // Guardamos sesión
            Session["IdUsuario"] = user.IdUsuario;
            Session["Nombre"] = user.Nombre;
            Session["TipoUsuario"] = user.TipoUsuario;

            // Redirección según tipo
            if (user.TipoUsuario == "Admin")
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Usuarios");
        }

        // Cerrar Sesión
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}