using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using appPedidos.Data;
using appPedidos.Models;
using Microsoft.AspNetCore.Http;
using appPedidos.Filters;
using Ganss.Xss;

namespace appPedidos.Controllers
{
    [RequireLogin]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Todos los usuarios logueados pueden ver productos
        public async Task<IActionResult> Index(string searchString, decimal? minPrice, decimal? maxPrice)
        {
            var products = from p in _context.Products select p;

            if (!string.IsNullOrEmpty(searchString))
                products = products.Where(p => p.Nombre.Contains(searchString));

            if (minPrice.HasValue)
                products = products.Where(p => p.Precio >= minPrice.Value);

            if (maxPrice.HasValue)
                products = products.Where(p => p.Precio <= maxPrice.Value);

            return View(await products.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // Solo admin o empleado pueden crear productos
        [RequireRole("admin", "empleado")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [RequireRole("admin", "empleado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Descripcion,Precio,Stock")] Product product)
        {
            var sanitizer = new HtmlSanitizer();
            product.Descripcion = sanitizer.Sanitize(product.Descripcion);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Producto agregado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al guardar el producto: " + ex.Message);
                }
            }
            return View(product);
        }

        [RequireRole("admin", "empleado")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost]
        [RequireRole("admin", "empleado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Precio,Stock")] Product product)
        {
            var sanitizer = new HtmlSanitizer();
            product.Descripcion = sanitizer.Sanitize(product.Descripcion);

            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Producto actualizado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                        return NotFound();
                    else
                        ModelState.AddModelError("", "Error de concurrencia al actualizar el producto.");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el producto: " + ex.Message);
                }
            }
            return View(product);
        }

        [RequireRole("admin", "empleado")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [RequireRole("admin", "empleado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Producto eliminado correctamente.";
            }
            else
            {
                TempData["Error"] = "Producto no encontrado.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}