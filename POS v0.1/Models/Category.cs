using System;
using System.Collections.Generic;
using System.Linq;

namespace POS_v0_1.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        // Navigation property
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        // Calculated property instead of manual counter
        public int ProductCount => Products?.Count ?? 0;
    }
}
