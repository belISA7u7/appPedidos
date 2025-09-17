using System.ComponentModel.DataAnnotations;

namespace appPedidos.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string Nombre { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Rol { get; set; } // puede ser admin, cliente, empleado
    }
}
