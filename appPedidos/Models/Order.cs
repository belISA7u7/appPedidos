using System;
using System.ComponentModel.DataAnnotations;

namespace appPedidos.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        public string Estado { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El total no puede ser negativo.")]
        public decimal Total { get; set; }

        // Propiedad de navegación
        public User Cliente { get; set; }
    }
}