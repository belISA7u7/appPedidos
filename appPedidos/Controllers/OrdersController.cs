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

        public async Task<IActionResult> Index()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");
            IQueryable<Order> orders = _context.Orders.Include(o => o.Cliente);

            if (userRole == "cliente" && userId.HasValue)
                orders = orders.Where(o => o.ClienteId == userId.Value);

            return View(await orders.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders.Include(o => o.Cliente).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            var orderItems = await _context.OrderItems.Include(oi => oi.Producto).Where(oi => oi.OrderId == order.Id).ToListAsync();
            ViewBag.OrderItems = orderItems;
            return View(order);
        }

        public IActionResult Create()
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "admin" || userRole == "empleado")
                ViewData["ClienteId"] = new SelectList(_context.Users.Where(u => u.Rol == "cliente"), "Id", "Email");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order order)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userRole == "cliente")
            {
                if (!userId.HasValue)
                {
                    TempData["Error"] = "Debes iniciar sesión para crear pedidos.";
                    return RedirectToAction("Login", "Users");
                }
                order.ClienteId = userId.Value;
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

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders.Include(o => o.Cliente).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            var orderItems = await _context.OrderItems.Include(oi => oi.Producto).Where(oi => oi.OrderId == order.Id).ToListAsync();
            ViewBag.OrderItems = orderItems;
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "admin" || userRole == "empleado")
                ViewData["ClienteId"] = new SelectList(_context.Users.Where(u => u.Rol == "cliente"), "Id", "Email", order.ClienteId);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,Fecha,Estado")] Order order)
        {
            if (id != order.Id) return NotFound();
            var userRole = HttpContext.Session.GetString("UserRole");
            var dbOrder = await _context.Orders.Include(o => o.Cliente).FirstOrDefaultAsync(o => o.Id == id);
            if (dbOrder == null) return NotFound();

            if (userRole == "admin" || userRole == "empleado")
            {
                dbOrder.Estado = order.Estado;
                dbOrder.ClienteId = order.ClienteId;
            }
            dbOrder.Total = await _context.OrderItems.Where(oi => oi.OrderId == id).SumAsync(oi => oi.Subtotal);

            if (ModelState.IsValid)
            {
                _context.Update(dbOrder);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Pedido actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            if (userRole == "admin" || userRole == "empleado")
                ViewData["ClienteId"] = new SelectList(_context.Users.Where(u => u.Rol == "cliente"), "Id", "Email", order.ClienteId);
            ViewBag.OrderItems = await _context.OrderItems.Include(oi => oi.Producto).Where(oi => oi.OrderId == id).ToListAsync();
            return View(dbOrder);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders.Include(o => o.Cliente).FirstOrDefaultAsync(o => o.Id == id);
            if (order == null) return NotFound();
            var orderItems = await _context.OrderItems.Include(oi => oi.Producto).Where(oi => oi.OrderId == order.Id).ToListAsync();
            ViewBag.OrderItems = orderItems;
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
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
            return RedirectToAction(nameof(Index));
        }
    }
}