using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS_v0_1.Models
{
    public class ProductImei : INotifyPropertyChanged
    {
        private int _id;
        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        private int _productId;
        public int ProductId { get => _productId; set { _productId = value; OnPropertyChanged(); } }

        private string _imeiNumber = string.Empty;
        public string ImeiNumber { get => _imeiNumber; set { _imeiNumber = value; OnPropertyChanged(); } }

        private string _status = "Disponible"; // Disponible, Vendido
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        private int? _saleId;
        public int? SaleId { get => _saleId; set { _saleId = value; OnPropertyChanged(); } }

        private DateTime? _saleDate;
        public DateTime? SaleDate { get => _saleDate; set { _saleDate = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
