using System;
using System.Linq;
using System.Web.Mvc;
using Kiosco_Don_Pepe;

namespace Kiosco_Don_Pepe.Controllers
{
    public class CrearCuentaController : Controller
    {
        // Instancia el contexto de la base de datos.
        private SistemaFacturacionEntities db = new SistemaFacturacionEntities();

        // GET: /CrearCuenta/Cuenta
        public ActionResult Cuenta()
        {
            // Redirige a la vista Index.cshtml de la carpeta Cuenta.
            return View("~/Views/Cuenta/Index.cshtml");
        }

        // POST: /CrearCuenta/Cuenta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cuenta(Usuario nuevoUsuario)
        {
            // Verifica que los datos del modelo sean válidos.
            if (ModelState.IsValid)
            {
                // Asigna el rol "Usuario" por defecto.
                nuevoUsuario.TipoUsuario = "Usuario";

                try
                {
                    // Añade el nuevo usuario a la base de datos.
                    db.Usuarios.Add(nuevoUsuario);
                    // Guarda los cambios.
                    db.SaveChanges();

                    // Redirige a la vista Index del controlador Usuario.
                    return RedirectToAction("login", "Home");
                }
                catch (Exception ex)
                {
                    // Muestra error si falla la base de datos.
                    ModelState.AddModelError("", "Ocurrió un error al intentar crear la cuenta: " + ex.Message);
                }
            }

            // Vuelve a la vista si hay errores.
            return View("~/Views/Cuenta/Index.cshtml", nuevoUsuario);
        }

        protected override void Dispose(bool disposing)
        {
            // Libera los recursos del contexto de la base de datos.
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}