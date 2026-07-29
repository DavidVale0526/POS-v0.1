using System;
using System.Collections.Generic;

namespace POS_v0_1.Models
{
    public class CashCount
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string PerformedBy { get; set; }
        public decimal TotalCalculated { get; set; }
        public string LinkedToCash { get; set; } = "Caja General";
        public string Notes { get; set; }
        public List<CashDenomination> Details { get; set; } = new List<CashDenomination>();
    }

    public class CashDenomination
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => Value * Quantity;
    }
}
