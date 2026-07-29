using System;

namespace POS_v0_1.Models
{
    public class CashSession
    {
        public bool IsOpen { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime OpeningTime { get; set; }
        public DateTime? ClosingTime { get; set; }
    }
}
