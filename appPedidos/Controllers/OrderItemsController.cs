using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using appPedidos.Data;
using appPedidos.Models;
using Microsoft.AspNetCore.Http;
using appPedidos.Filters;

namespace appPedidos.Controllers
{
    [RequireLogin]
    public class OrderItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        

        // POST: OrderItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,ProductoId,Cantidad")] OrderItem orderItem)
        {
            

            var order = await _context.Orders.FindAsync(orderItem.OrderId);
            if (order == null)
            {
                TempData["Error"] = "Pedido no encontrado.";
                return RedirectToAction("Index", "Orders");
            }

            var producto = await _context.Products.FindAsync(orderItem.ProductoId);
            if (producto == null)
            {
                TempData["Error"] = "Producto no encontrado.";
                return RedirectToAction("Edit", "Orders", new { id = orderItem.OrderId });
            }
            if (orderItem.Cantidad > producto.Stock)
            {
                TempData["Error"] = "No hay suficiente stock para este producto.";
                return RedirectToAction("Edit", "Orders", new { id = orderItem.OrderId });
            }

            // Validar que no se agregue dos veces el mismo producto al mismo pedido
            var existe = await _context.OrderItems
                .AnyAsync(oi => oi.OrderId == orderItem.OrderId && oi.ProductoId == orderItem.ProductoId);
            if (existe)
            {
                TempData["Error"] = "El producto ya ha sido agregado a este pedido.";
                return RedirectToAction("Edit", "Orders", new { id = orderItem.OrderId });
            }

            // Calcular subtotal
            orderItem.Subtotal = producto.Precio * orderItem.Cantidad;

            // Actualizar stock
            producto.Stock -= orderItem.Cantidad;

            try
            {
                _context.Add(orderItem);
                await _context.SaveChangesAsync();
                await RecalcularTotal(orderItem.OrderId);
                TempData["Success"] = "Producto agregado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al agregar el producto: " + ex.Message;
            }
            return RedirectToAction("Edit", "Orders", new { id = orderItem.OrderId });
        }

        // GET: OrderItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            

            if (id == null) return NotFound();

            var orderItem = await _context.OrderItems
                .Include(oi => oi.Producto)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (orderItem == null) return NotFound();

            return View(orderItem);
        }

        // POST: OrderItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,ProductoId,Cantidad")] OrderItem orderItem)
        {
            

            if (id != orderItem.Id) return NotFound();

            var existingItem = await _context.OrderItems
                .Include(oi => oi.Producto)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (existingItem == null) return NotFound();

            var producto = existingItem.Producto;

            // Calcular stock disponible sumando lo que ya tenía reservado este item
            int stockDisponible = producto.Stock + existingItem.Cantidad;

            if (orderItem.Cantidad > stockDisponible)
            {
                ModelState.AddModelError("", "No hay suficiente stock disponible para actualizar la cantidad de este producto.");
                return View(existingItem);
            }

            if (orderItem.Cantidad < 1)
            {
                ModelState.AddModelError("", "La cantidad debe ser mayor a cero.");
                return View(existingItem);
            }

            // Ajustar stock
            producto.Stock = stockDisponible - orderItem.Cantidad;

            // Actualizar cantidad y subtotal
            existingItem.Cantidad = orderItem.Cantidad;
            existingItem.Subtotal = producto.Precio * orderItem.Cantidad;

            try
            {
                await _context.SaveChangesAsync();
                await RecalcularTotal(existingItem.OrderId);
                TempData["Success"] = "Producto actualizado correctamente.";
                return RedirectToAction("Edit", "Orders", new { id = existingItem.OrderId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al actualizar: " + ex.Message);
                return View(existingItem);
            }
        }

        // GET: OrderItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            

            if (id == null) return NotFound();

            var orderItem = await _context.OrderItems
                .Include(oi => oi.Producto)
                .Include(oi => oi.Order)
                .FirstOrDefaultAsync(oi => oi.Id == id);

            if (orderItem == null) return NotFound();

            return View(orderItem);
        }

        // POST: OrderItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderItem = await _context.OrderItems
                .Include(oi => oi.Producto)
                .FirstOrDefaultAsync(oi => oi.Id == id);
            if (orderItem == null)
            {
                TempData["Error"] = "Item no encontrado.";
                return RedirectToAction("Index", "Orders");
            }
            // Devolver el stock al producto
            var producto = orderItem.Producto;
            if (producto != null)
                producto.Stock += orderItem.Cantidad;

            int orderId = orderItem.OrderId;

            try
            {
                _context.OrderItems.Remove(orderItem);
                await _context.SaveChangesAsync();
                await RecalcularTotal(orderId);
                TempData["Success"] = "Producto eliminado del pedido.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al eliminar el producto: " + ex.Message;
            }
            return RedirectToAction("Edit", "Orders", new { id = orderId });
        }


        // Ayuda: recalcula el total del pedido después de cambios
        private async Task RecalcularTotal(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                var total = await _context.OrderItems
                    .Where(oi => oi.OrderId == orderId)
                    .SumAsync(oi => oi.Subtotal);
                order.Total = total;
                await _context.SaveChangesAsync();
            }
        }
    }
}