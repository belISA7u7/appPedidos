using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using appPedidos.Data;
using appPedidos.Models;
using Microsoft.AspNetCore.Http;

namespace appPedidos.Controllers
{
    public class OrderItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Solo admin o empleado pueden gestionar los items de pedido
        private bool IsAdminOrEmpleado()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "admin" || role == "empleado";
        }

        // GET: OrderItems
        public async Task<IActionResult> Index()
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            var applicationDbContext = _context.OrderItems
                .Include(o => o.Order)
                .Include(o => o.Producto);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: OrderItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems
                .Include(o => o.Order)
                .Include(o => o.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // GET: OrderItems/Create
        public IActionResult Create()
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id");
            ViewData["ProductoId"] = new SelectList(_context.Products, "Id", "Nombre");
            return View();
        }

        // POST: OrderItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,OrderId,ProductoId,Cantidad,Subtotal")] OrderItem orderItem)
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            if (ModelState.IsValid)
            {
                // Validación de stock
                var producto = await _context.Products.FindAsync(orderItem.ProductoId);
                if (producto == null)
                {
                    ModelState.AddModelError("", "Producto no encontrado.");
                }
                else if (orderItem.Cantidad > producto.Stock)
                {
                    ModelState.AddModelError("", "No hay suficiente stock para este producto.");
                }
                else
                {
                    // Calcular subtotal (puedes mejorar esto usando propiedades calculadas)
                    orderItem.Subtotal = producto.Precio * orderItem.Cantidad;

                    // Actualizar stock del producto
                    producto.Stock -= orderItem.Cantidad;

                    try
                    {
                        _context.Add(orderItem);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Item agregado correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Error al agregar el item: " + ex.Message);
                    }
                }
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            ViewData["ProductoId"] = new SelectList(_context.Products, "Id", "Nombre", orderItem.ProductoId);
            return View(orderItem);
        }

        // GET: OrderItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            ViewData["ProductoId"] = new SelectList(_context.Products, "Id", "Nombre", orderItem.ProductoId);
            return View(orderItem);
        }

        // POST: OrderItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,ProductoId,Cantidad,Subtotal")] OrderItem orderItem)
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            if (id != orderItem.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Validar stock
                    var producto = await _context.Products.FindAsync(orderItem.ProductoId);
                    if (producto == null)
                    {
                        ModelState.AddModelError("", "Producto no encontrado.");
                    }
                    else if (orderItem.Cantidad > producto.Stock)
                    {
                        ModelState.AddModelError("", "No hay suficiente stock para este producto.");
                    }
                    else
                    {
                        // Actualizar subtotal y stock
                        orderItem.Subtotal = producto.Precio * orderItem.Cantidad;
                        // (Opcional: ajustar stock si la cantidad cambió, lógica extra requerida)

                        _context.Update(orderItem);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Item actualizado correctamente.";
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderItemExists(orderItem.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error de concurrencia al actualizar el item.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el item: " + ex.Message);
                }
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "Id", "Id", orderItem.OrderId);
            ViewData["ProductoId"] = new SelectList(_context.Products, "Id", "Nombre", orderItem.ProductoId);
            return View(orderItem);
        }

        // GET: OrderItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            if (id == null)
            {
                return NotFound();
            }

            var orderItem = await _context.OrderItems
                .Include(o => o.Order)
                .Include(o => o.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (orderItem == null)
            {
                return NotFound();
            }

            return View(orderItem);
        }

        // POST: OrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            try
            {
                var orderItem = await _context.OrderItems.FindAsync(id);
                if (orderItem != null)
                {
                    // (Opcional: restaurar stock del producto si se elimina el item)
                    // var producto = await _context.Products.FindAsync(orderItem.ProductoId);
                    // if (producto != null) producto.Stock += orderItem.Cantidad;

                    _context.OrderItems.Remove(orderItem);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Item eliminado correctamente.";
                }
                else
                {
                    TempData["Error"] = "Item no encontrado.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el item: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        private bool OrderItemExists(int id)
        {
            return _context.OrderItems.Any(e => e.Id == id);
        }
    }
}