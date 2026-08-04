using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS_v0_1.Models
{
    public class RepairType : INotifyPropertyChanged
    {
        private int _id;
        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        private int _categoryId;
        public int CategoryId { get => _categoryId; set { _categoryId = value; OnPropertyChanged(); } }

        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private decimal _basePrice;
        public decimal BasePrice { get => _basePrice; set { _basePrice = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private string _categoryName;
        public string CategoryName { get => _categoryName; set { _categoryName = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
