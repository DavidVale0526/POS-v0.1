using System;

namespace POS_v0_1.Models
{
    public class CreditNote
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public string? CustomerName { get; set; }
    }
}
