using System;

namespace POS_v0_1.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string Role { get; set; } = "Usuario";
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public string? Pin { get; set; } // 4-6 dígitos
        public string? Password { get; set; }
    }
}
