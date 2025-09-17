using appPedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace appPedidos.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor que llama al base con las opciones
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet para cada modelo (tabla)
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}
