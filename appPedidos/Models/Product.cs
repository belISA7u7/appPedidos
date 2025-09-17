using System.ComponentModel.DataAnnotations;

namespace appPedidos.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Nombre { get; set; }
        [StringLength(200)]
        public string Descripcion { get; set; }
        [Range(0.01, double.MaxValue)]
        public decimal Precio { get; set; }
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }
}
