using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS_v0_1.Models
{
    public class Product : INotifyPropertyChanged
    {
        private int _id;
        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        private string _name = string.Empty;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string? _barcode;
        public string? Barcode { get => _barcode; set { _barcode = value; OnPropertyChanged(); } }

        private string? _sku;
        public string? SKU { get => _sku; set { _sku = value; OnPropertyChanged(); } }

        private string? _description;
        public string? Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private int _categoryId;
        public int CategoryId { get => _categoryId; set { _categoryId = value; OnPropertyChanged(); OnPropertyChanged(nameof(CategoryName)); } }

        private Category? _category;
        public virtual Category? Category { get => _category; set { _category = value; OnPropertyChanged(); OnPropertyChanged(nameof(CategoryName)); } }

        public string? CategoryLegacy { get; set; } = "";

        private int? _providerId;
        public int? ProviderId { get => _providerId; set { _providerId = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProviderName)); } }

        private Provider? _provider;
        public virtual Provider? Provider { get => _provider; set { _provider = value; OnPropertyChanged(); OnPropertyChanged(nameof(ProviderName)); } }

        public string? CategoryName => Category?.Name;
        public string? ProviderName => Provider?.Name;

        private string? _location;
        public string? Location { get => _location; set { _location = value; OnPropertyChanged(); } }

        private decimal _purchasePrice;
        public decimal PurchasePrice { get => _purchasePrice; set { _purchasePrice = value; OnPropertyChanged(); } }

        private decimal _sellingPrice;
        public decimal SellingPrice { get => _sellingPrice; set { _sellingPrice = value; OnPropertyChanged(); OnPropertyChanged(nameof(Price)); } }

        private int _stock;
        public int Stock { get => _stock; set { _stock = value; OnPropertyChanged(); } }

        private int _minStock;
        public int MinStock { get => _minStock; set { _minStock = value; OnPropertyChanged(); } }

        private string _status = "Activo";
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        private string? _imagePath;
        public string? ImagePath { get => _imagePath; set { _imagePath = value; OnPropertyChanged(); } }

        public decimal Price
        {
            get => SellingPrice;
            set { SellingPrice = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
