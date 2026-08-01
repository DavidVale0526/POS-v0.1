using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS_v0_1.Models
{
    public class CartItem : INotifyPropertyChanged
    {
        private int _quantity;
        private decimal _discount;
        public Product Product { get; set; }

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
