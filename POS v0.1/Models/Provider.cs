using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS_v0_1.Models
{
    public class Provider : INotifyPropertyChanged
    {
        private int _id;
        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        private string _name = string.Empty;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string? _contactName;
        public string? ContactName { get => _contactName; set { _contactName = value; OnPropertyChanged(); } }

        private string? _phone;
        public string? Phone { get => _phone; set { _phone = value; OnPropertyChanged(); } }

        private string? _email;
        public string? Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private string? _address;
        public string? Address { get => _address; set { _address = value; OnPropertyChanged(); } }

        private string? _city;
        public string? City { get => _city; set { _city = value; OnPropertyChanged(); } }

        private string? _description;
        public string? Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private string _status = "Activo";
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
