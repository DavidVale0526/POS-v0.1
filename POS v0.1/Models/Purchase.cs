using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace POS_v0_1.Models
{
    public class Purchase
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string? ProviderName { get; set; }
        public string? InvoiceNumber { get; set; }
        public decimal Total { get; set; }
        public string? RegisteredBy { get; set; } = "Usuario";
        public string? Notes { get; set; }
        public ObservableCollection<PurchaseDetail> Items { get; set; } = new ObservableCollection<PurchaseDetail>();
    }

    public class PurchaseDetail : System.ComponentModel.INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value) return;
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (_unitPrice == value) return;
                _unitPrice = value;
                OnPropertyChanged(nameof(UnitPrice));
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal Subtotal => Quantity * UnitPrice;

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
