using System;

namespace POS_v0_1.Models
{
    public class BankTransaction
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } // Depósito / Retiro
        public string Bank { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceAfter { get; set; }
        public string RegisteredBy { get; set; }
        public string Notes { get; set; }
    }
}
