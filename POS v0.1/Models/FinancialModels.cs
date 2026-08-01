using System;
using System.Collections.Generic;

namespace POS_v0_1.Models
{
    public class FinancialSource
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation property
        public virtual ICollection<FinancialMovement> Movements { get; set; } = new List<FinancialMovement>();

        public int MovementCount => Movements?.Count ?? 0;
        public string Status { get; set; } = "Activo";
    }

    public class FinancialMovement
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; } = "Gasto"; // Gasto, Ingreso
        public string Concept { get; set; } = string.Empty;

        // New Foreign Key Relation
        public int SourceId { get; set; }
        public virtual FinancialSource? FinancialSource { get; set; }

        public decimal Amount { get; set; }
        public string RegisteredBy { get; set; } = "Usuario";
        public string? Reference { get; set; }
        public string? Notes { get; set; }
    }
}
