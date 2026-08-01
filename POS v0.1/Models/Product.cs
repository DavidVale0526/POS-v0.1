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
        public string Category { get; set; } = string.Empty;
        public string? Provider { get; set; }
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
