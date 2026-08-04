using System;

namespace POS_v0_1.Models
{
    public class StockMovement
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public int QuantityChange { get; set; } // Positivo para entradas, negativo para salidas
        public int NewStock { get; set; } // Stock resultante después del movimiento

        public string MovementType { get; set; } = "Ajuste"; // Venta, Compra, Anulación, Ajuste Manual, Reparación
        public string? Reason { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        public int? UserId { get; set; }
        public string? UserName { get; set; }
    }
}
