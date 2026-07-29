using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace POS_v0_1.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string ProviderName { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal Total { get; set; }
        public string RegisteredBy { get; set; } = "Administrador";
        public string Notes { get; set; }
        public ObservableCollection<PurchaseDetail> Items { get; set; } = new ObservableCollection<PurchaseDetail>();
    }

    public class PurchaseDetail
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
    }
}
