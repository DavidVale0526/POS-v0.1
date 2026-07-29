using System;

namespace POS_v0_1.Models
{
    public class Topup
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public string Company { get; set; } // TIGO / CLARO
        public string PhoneNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal Cost { get; set; }
        public decimal Profit => Amount - Cost;
        public string Seller { get; set; } = "Administrador";
        public string Notes { get; set; }
    }
}
