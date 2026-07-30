using System;

namespace POS_v0_1.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // Administrador, Vendedor
        public DateTime RegistrationDate { get; set; } = DateTime.Now;
        public string Pin { get; set; } // 4-6 dígitos
        public string Password { get; set; }
    }
}
