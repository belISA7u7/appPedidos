namespace appPedidos.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public User Cliente { get; set; }
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } // Pendiente, procesado, enviado, entregado
        public decimal Total { get; set; }
        public ICollection<OrderItem> Items { get; set; }
    }
}
