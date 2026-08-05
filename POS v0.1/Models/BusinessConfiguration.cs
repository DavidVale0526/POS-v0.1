using System.ComponentModel.DataAnnotations;

namespace POS_v0_1.Models
{
    public class BusinessConfiguration
    {
        [Key]
        public int Id { get; set; }
        public string BusinessName { get; set; } = "Mi Tienda";
        public string? Phone { get; set; } = "";
        public string? Email { get; set; } = "";
        public string? Address { get; set; } = "";
        public string? LogoPath { get; set; } = "";
        public string TicketFormat { get; set; } = "58mm";
        public decimal TaxRate { get; set; }

        // Permisos para el rol "Usuario"
        public bool AllowUserSalesHistory { get; set; } = false;
        public bool AllowUserInventory { get; set; } = false;
        public bool AllowUserPurchases { get; set; } = false;
        public bool AllowUserRepairs { get; set; } = true;
        public bool AllowUserTreasury { get; set; } = false;
        public bool AllowUserClients { get; set; } = false;
        public bool AllowUserReports { get; set; } = false;

        // Fase 5: Estabilidad y Seguridad
        public bool AutoBackupEnabled { get; set; } = true;
        public System.DateTime? LastBackupDate { get; set; }
    }
}
