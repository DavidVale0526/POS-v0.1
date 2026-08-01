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
    }
}
