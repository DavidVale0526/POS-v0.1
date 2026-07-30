using System;

namespace POS_v0_1.Models
{
    public class FinancialSource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int MovementCount { get; set; }
        public string Status { get; set; } = "Activo";
    }

    public class FinancialMovement
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } // Gasto, Ingreso
        public string Concept { get; set; }
        public string Source { get; set; }
        public decimal Amount { get; set; }
        public string RegisteredBy { get; set; }
        public string Reference { get; set; }
        public string Notes { get; set; }
    }
}
