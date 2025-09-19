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
        public async Task<IActionResult> Create([Bind("Id,ClienteId,Fecha,Estado,Total")] Order order)
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            if (ModelState.IsValid)
            {
                try
                {
                    // Lógica para calcular total y validar stock debería ir aquí en el futuro
                    _context.Add(order);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Pedido creado correctamente.";
                    return RedirectToAction(nameof(Index));
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

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            ViewData["ClienteId"] = new SelectList(_context.Users, "Id", "Email", order.ClienteId);
            return View(order);
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,Fecha,Estado,Total")] Order order)
        {
            if (!IsAdminOrEmpleado())
                return RedirectToAction("Login", "Users");

            if (id != order.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Lógica de actualización de totales y validaciones puede ir aquí
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