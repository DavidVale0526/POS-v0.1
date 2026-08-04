using System;

namespace POS_v0_1.Models
{
    public class CashSession
    {
        public int Id { get; set; }
        public bool IsOpen { get; set; }
        public string OpenedBy { get; set; }
        public string? ClosedBy { get; set; }
        public DateTime OpeningTime { get; set; }
        public DateTime? ClosingTime { get; set; }
        public decimal OpeningBalance { get; set; }

        // Totals for the session
        public decimal SalesTotal { get; set; }
        public decimal IncomesTotal { get; set; }
        public decimal ExpensesTotal { get; set; }
        public decimal TopupsTotal { get; set; }
        public decimal RepairsTotal { get; set; }

        // Desglose por método (Solo ventas/servicios)
        public decimal CashTotal { get; set; }
        public decimal CardTotal { get; set; }
        public decimal TransferTotal { get; set; }

        public decimal ClosingBalance { get; set; } // Actual money counted at closing

        public decimal TheoreticalBalance => OpeningBalance + SalesTotal + IncomesTotal + TopupsTotal + RepairsTotal - ExpensesTotal;

        public decimal Difference => ClosingBalance - TheoreticalBalance;

        public string Status => IsOpen ? "ABIERTA" : "CERRADA";
    }
}
