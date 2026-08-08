using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS_v0_1.Models
{
    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity;
        private decimal _discount;
        private string? _selectedImei;
        private System.Collections.ObjectModel.ObservableCollection<string> _availableImeis = new System.Collections.ObjectModel.ObservableCollection<string>();

        public Product Product { get; set; }

        public string? SelectedImei
        {
            get => _selectedImei;
            set { _selectedImei = value; OnPropertyChanged(); }
        }

        public System.Collections.ObjectModel.ObservableCollection<string> AvailableImeis
        {
            get => _availableImeis;
            set { _availableImeis = value; OnPropertyChanged(); }
        }

        public bool HasImeis => AvailableImeis.Count > 0;
        public bool ShowImeiWarning => HasImeis && string.IsNullOrEmpty(SelectedImei);

        public CartItem() { }
        public CartItem(Product product)
        {
            Product = product;
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal Discount
        {
            get => _discount;
            set
            {
                _discount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal Subtotal => (Product.SellingPrice * Quantity) - Discount;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
