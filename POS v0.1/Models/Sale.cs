using System;
using System.Collections.Generic;

namespace POS_v0_1.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? CustomerName { get; set; } = "Consumidor Final";
        public string? SellerPin { get; set; }
        public string? SellerName { get; set; }
        public decimal GrossAmount { get; set; } // Nuevo: Precio x Cantidad sin ningún descuento
        public decimal Subtotal { get; set; } // Ya con descuentos de item aplicados
        public decimal Discount { get; set; } // Descuento global
        public decimal TotalDiscount { get; set; } // Nuevo: Suma de descuentos de item + global
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = "Efectivo";
        public decimal ReceivedAmount { get; set; }
        public decimal ChangeAmount { get; set; }

        // Desglose de pago (para pagos mixtos)
        public decimal AmountCash { get; set; }
        public decimal AmountCard { get; set; }
        public decimal AmountTransfer { get; set; }

        public bool IsCancelled { get; set; }
        public string? CancellationReason { get; set; }
        public virtual ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();
    }

    public class SaleItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal PurchasePrice { get; set; } // Costo al momento de la venta
        public decimal Discount { get; set; }
        public decimal Subtotal => (Quantity * UnitPrice) - Discount;
    }
}
