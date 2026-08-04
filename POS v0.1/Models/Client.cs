using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS_v0_1.Models
{
    public class Client : INotifyPropertyChanged
    {
        private int _id;
        public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }

        private string _name = string.Empty;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string? _phone;
        public string? Phone { get => _phone; set { _phone = value; OnPropertyChanged(); OnPropertyChanged(nameof(ContactSummary)); } }

        private string? _email;
        public string? Email { get => _email; set { _email = value; OnPropertyChanged(); OnPropertyChanged(nameof(ContactSummary)); } }

        private string? _address;
        public string? Address { get => _address; set { _address = value; OnPropertyChanged(); } }

        private string? _notes;
        public string? Notes { get => _notes; set { _notes = value; OnPropertyChanged(); } }

        private DateTime _createdAt = DateTime.Now;
        public DateTime CreatedAt { get => _createdAt; set { _createdAt = value; OnPropertyChanged(); } }

        public string ContactSummary => $"{Phone} {(string.IsNullOrEmpty(Email) ? "" : "| " + Email)}";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
