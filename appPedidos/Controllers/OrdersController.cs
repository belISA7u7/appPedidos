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
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Método auxiliar para restringir acceso a admin o empleado
        private bool IsAdminOrEmpleado()
        {
            var role = HttpContext.Session.GetString("UserRole");
            return role == "admin" || role == "empleado";
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            var applicationDbContext = _context.Orders.Include(o => o.Cliente);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
                return NotFound();

            // Obtener los OrderItems relacionados
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Producto)
                .Where(oi => oi.OrderId == order.Id)
                .ToListAsync();

            ViewBag.OrderItems = orderItems;

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            ViewData["ClienteId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ClienteId")] Order order)
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            // Asignar fecha y estado por defecto
            order.Fecha = DateTime.Now;
            order.Estado = "Pendiente";
            order.Total = 0;

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(order);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Pedido creado correctamente.";
                    // Redirigir a la edición para que agreguen items al pedido
                    return RedirectToAction("Edit", new { id = order.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el pedido: " + ex.Message);
                }
            }
            ViewData["ClienteId"] = new SelectList(_context.Users, "Id", "Email", order.ClienteId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.Cliente)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            // Obtener los OrderItems relacionados
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Producto)
                .Where(oi => oi.OrderId == order.Id)
                .ToListAsync();

            ViewBag.OrderItems = orderItems;

            ViewData["ClienteId"] = new SelectList(_context.Users, "Id", "Email", order.ClienteId);
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,Fecha,Estado")] Order order)
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            if (id != order.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Recalcular total automáticamente sumando los subtotales de los OrderItems
                    var orderItems = await _context.OrderItems
                        .Where(oi => oi.OrderId == id)
                        .ToListAsync();

                    order.Total = orderItems.Sum(oi => oi.Subtotal);

                    _context.Update(order);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Pedido actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                        return NotFound();
                    else
                        ModelState.AddModelError("", "Error de concurrencia al actualizar el pedido.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el pedido: " + ex.Message);
                }
            }
            ViewData["ClienteId"] = new SelectList(_context.Users, "Id", "Email", order.ClienteId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
                return NotFound();

            // Obtener los OrderItems relacionados
            var orderItems = await _context.OrderItems
                .Include(oi => oi.Producto)
                .Where(oi => oi.OrderId == order.Id)
                .ToListAsync();

            ViewBag.OrderItems = orderItems;

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order != null)
                {
                    // Elimina primero los OrderItems asociados
                    var orderItems = await _context.OrderItems.Where(oi => oi.OrderId == id).ToListAsync();
                    if (orderItems.Any())
                        _context.OrderItems.RemoveRange(orderItems);

                    _context.Orders.Remove(order);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Pedido eliminado correctamente.";
                }
                else
                {
                    TempData["Error"] = "Pedido no encontrado.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el pedido: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}