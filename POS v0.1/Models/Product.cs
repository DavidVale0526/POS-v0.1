using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS_v0_1.Models
{
    public class Product : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public string? SKU { get; set; }
        public string? Description { get; set; }

        // New Foreign Key Relations
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public int? ProviderId { get; set; }
        public virtual Provider? Provider { get; set; }

        // Legacy string properties (keeping them for migration or UI compatibility if needed,
        // but they should be phased out in favor of navigation properties)
        public string? CategoryName => Category?.Name;
        public string? ProviderName => Provider?.Name;

        public string? Location { get; set; }

        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }

        private int _stock;
        public int Stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(); }
        }

        public int MinStock { get; set; }

        public string Status { get; set; } = "Activo";
        public string? ImagePath { get; set; }

        public decimal Price
        {
            get => SellingPrice;
            set => SellingPrice = value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
