using System;

namespace POS_v0_1.Models
{
    public class Topup
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string? Company { get; set; } // TIGO / CLARO
        public string? PhoneNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit => Amount - Cost;
        public string? Seller { get; set; } = "Usuario";
        public string? Notes { get; set; }
    }

    public class TopupCompany
    {
        public int Id { get; set; }
        public string Name { get; set; } // TIGO, CLARO, etc.
        public string? Color { get; set; } // For UI
    }

    public class TopupPreset
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public decimal Amount { get; set; }
        public decimal Cost { get; set; }
        public string DisplayLabel => $"{CompanyName} - C$ {Amount:N0}";
    }
}
