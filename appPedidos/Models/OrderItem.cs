using System.ComponentModel.DataAnnotations;

namespace appPedidos.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ProductoId { get; set; }
        public Product Producto { get; set; }
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }
        public decimal Subtotal { get; set; }
    }
}
