using System.ComponentModel.DataAnnotations;

namespace appPedidos.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductoId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe ser al menos 1 unidad.")]
        public int Cantidad { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El subtotal no puede ser negativo.")]
        public decimal Subtotal { get; set; }

        // Propiedades de navegación (opcional)
        public Order Order { get; set; }
        public Product Producto { get; set; }
    }
}