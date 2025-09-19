using System.ComponentModel.DataAnnotations;

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

    // Navigation property
    public User Cliente { get; set; }
}