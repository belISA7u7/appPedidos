using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using appPedidos.Data;
using appPedidos.Models;
using Microsoft.AspNetCore.Http;
using appPedidos.Filters;


namespace appPedidos.Controllers
{
    [RequireLogin]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            

            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            IQueryable<Order> orders = _context.Orders.Include(o => o.Cliente);

            if (userRole == "cliente" && userId.HasValue)
                orders = orders.Where(o => o.ClienteId == userId.Value);

            return View(await orders.ToListAsync());
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            
            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.Cliente)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

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
            

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "cliente")
            {
                // No mostrar select, cliente es el logueado
                return View();
            }
            else
            {
                ViewData["ClienteId"] = new SelectList(_context.Users.Where(u => u.Rol == "cliente"), "Id", "Email");
                return View();
            }
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            

            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userRole == "cliente" && userId.HasValue)
            {
                order.ClienteId = userId.Value;
                // Quitar error de validación si existe
                ModelState.Remove("ClienteId");
            }

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
                    return RedirectToAction("Edit", new { id = order.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al crear el pedido: " + ex.Message);
                }
            }

            if (userRole != "cliente")
                ViewData["ClienteId"] = new SelectList(_context.Users.Where(u => u.Rol == "cliente"), "Id", "Email", order.ClienteId);

            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            
            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.Cliente)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            var orderItems = await _context.OrderItems
                .Include(oi => oi.Producto)
                .Where(oi => oi.OrderId == order.Id)
                .ToListAsync();

            ViewBag.OrderItems = orderItems;

            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "cliente")
                ViewData["ClienteId"] = new SelectList(_context.Users.Where(u => u.Rol == "cliente"), "Id", "Email", order.ClienteId);

            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,Fecha,Estado")] Order order)
        {
            
            if (id != order.Id)
                return NotFound();

            // Solo admin/empleado pueden cambiar estado y cliente
            var userRole = HttpContext.Session.GetString("UserRole");
            var dbOrder = await _context.Orders
                .Include(o => o.Cliente)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (dbOrder == null)
                return NotFound();

            if (userRole == "cliente")
            {
                // El cliente NO puede cambiar estado ni cliente ni fecha
                // Solo puede guardar y continuar con los productos
            }
            else
            {
                dbOrder.Estado = order.Estado; // Solo admin/empleado pueden cambiar estado
                dbOrder.ClienteId = order.ClienteId;
            }

            // Fecha nunca se edita aqui, solo al crear
            // dbOrder.Fecha = order.Fecha; // No se debe actualizar

            // Recalcular total sumando los subtotales de los OrderItems
            dbOrder.Total = await _context.OrderItems
                .Where(oi => oi.OrderId == id)
                .SumAsync(oi => oi.Subtotal);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dbOrder);
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

            if (userRole != "cliente")
                ViewData["ClienteId"] = new SelectList(_context.Users.Where(u => u.Rol == "cliente"), "Id", "Email", order.ClienteId);

            // Recargar items para la vista
            ViewBag.OrderItems = await _context.OrderItems
                .Include(oi => oi.Producto)
                .Where(oi => oi.OrderId == id)
                .ToListAsync();

            return View(dbOrder);
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            
            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.Cliente)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

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
            

            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order != null)
                {
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