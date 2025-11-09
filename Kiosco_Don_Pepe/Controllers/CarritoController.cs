using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kiosco_Don_Pepe;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Kiosco_Don_Pepe.Controllers
{
    public class CarritoController : Controller
    {
        // ✅ Ver carrito
        public ActionResult VerCarrito()
        {
            List<CarritoItem> carrito = Session["carrito"] as List<CarritoItem>;
            if (carrito == null)
                carrito = new List<CarritoItem>();

            return View(carrito);
        }

        // ✅ Agregar al carrito
        [HttpPost]
        public ActionResult AgregarCarrito(int idProducto, int cantidad)
        {
            using (var db = new SistemaFacturacionEntities())
            {
                var producto = db.Productos.Find(idProducto);

                if (producto == null || producto.Stock < cantidad)
                {
                    TempData["Error"] = "No hay stock suficiente.";
                    return RedirectToAction("Index", "Producto");
                }

                List<CarritoItem> carrito = Session["carrito"] as List<CarritoItem> ?? new List<CarritoItem>();

                var item = carrito.FirstOrDefault(x => x.IdProducto == idProducto);

                if (item == null)
                {
                    carrito.Add(new CarritoItem
                    {
                        IdProducto = producto.IdProducto,
                        Nombre = producto.Nombre,
                        Precio = producto.Precio,
                        Cantidad = cantidad
                    });
                }
                else
                {
                    item.Cantidad += cantidad;
                }

                Session["carrito"] = carrito;
                return RedirectToAction("VerCarrito");
            }
        }

        // ✅ Eliminar item del carrito
        public ActionResult Eliminar(int idProducto)
        {
            List<CarritoItem> carrito = Session["carrito"] as List<CarritoItem>;

            if (carrito != null)
            {
                var item = carrito.FirstOrDefault(x => x.IdProducto == idProducto);
                if (item != null)
                {
                    carrito.Remove(item);
                }
            }

            Session["carrito"] = carrito;
            return RedirectToAction("VerCarrito");
        }

        // ✅ Finalizar compra y descargar PDF
        public ActionResult FinalizarCompra()
        {
            List<CarritoItem> carrito = Session["carrito"] as List<CarritoItem>;

            if (carrito == null || carrito.Count == 0)
            {
                TempData["Error"] = "El carrito está vacío.";
                return RedirectToAction("VerCarrito");
            }

            Ticket ticket;

            using (var db = new SistemaFacturacionEntities())
            {
                // Crear Ticket
                ticket = new Ticket
                {
                    Fecha = DateTime.Now,
                    IdUsuario = 1,
                    Total = carrito.Sum(x => x.Total)
                };

                db.Tickets.Add(ticket);
                db.SaveChanges();

                // Crear detalles + descontar stock
                foreach (var item in carrito)
                {
                    var producto = db.Productos.Find(item.IdProducto);
                    producto.Stock -= item.Cantidad;

                    db.DetalleTickets.Add(new DetalleTicket
                    {
                        IdTicket = ticket.IdTicket,
                        IdProducto = item.IdProducto,
                        Cantidad = item.Cantidad,
                        PrecioUnitario = item.Precio
                    });
                }

                db.SaveChanges();
            }

            // Vaciar carrito
            Session["carrito"] = null;

            // Descargar PDF
            return GenerarPDF(ticket);
        }


        // ✅ Generar ticket PDF
        public FileResult GenerarPDF(Ticket ticket)
        {
            using (var db = new SistemaFacturacionEntities())
            {
                var detalles = db.DetalleTickets.Where(d => d.IdTicket == ticket.IdTicket).ToList();
                string nombreArchivo = $"Ticket_{ticket.IdTicket}.pdf";

                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                Document pdf = new Document();
                PdfWriter.GetInstance(pdf, stream);

                pdf.Open();
                pdf.Add(new Paragraph("KIOSCO DON PEPE\n"));
                pdf.Add(new Paragraph($"Ticket Nº: {ticket.IdTicket}"));
                pdf.Add(new Paragraph($"Fecha: {ticket.Fecha}"));
                pdf.Add(new Paragraph("--------------------------------------"));

                foreach (var item in detalles)
                {
                    var producto = db.Productos.Find(item.IdProducto);
                    pdf.Add(new Paragraph($"{producto.Nombre} x{item.Cantidad} = ${item.Cantidad * item.PrecioUnitario}"));
                }

                pdf.Add(new Paragraph("--------------------------------------"));
                pdf.Add(new Paragraph($"TOTAL: ${ticket.Total}"));
                pdf.Close();

                return File(stream.ToArray(), "application/pdf", nombreArchivo);
            }
        }
    }

    // ✅ Carrito en memoria
    public class CarritoItem
    {
        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public decimal Total => Precio * Cantidad;
    }
}
