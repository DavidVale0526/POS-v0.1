using System;
using System.Collections.ObjectModel;

namespace POS_v0_1.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? CustomerName { get; set; } = "Consumidor Final";
        public string? SellerPin { get; set; }
        public string? SellerName { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public string PaymentMethod { get; set; } = "Efectivo";
        public decimal ReceivedAmount { get; set; }
        public decimal ChangeAmount { get; set; }
        public bool IsCancelled { get; set; }
        public string? CancellationReason { get; set; }
        public ObservableCollection<SaleItem> Items { get; set; } = new ObservableCollection<SaleItem>();
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
