using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class ClientListViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        private ObservableCollection<Client> _filteredClients;
        public ObservableCollection<Client> FilteredClients
        {
            get => _filteredClients;
            set { _filteredClients = value; OnPropertyChanged(); }
        }

        public ICommand ShowCreateClientCommand { get; }
        public ICommand SearchCommand { get; }

        public ClientListViewModel(MainViewModel main)
        {
            _main = main;
            FilteredClients = new ObservableCollection<Client>(_main.Clients);

            ShowCreateClientCommand = new RelayCommand(o => _main.CurrentView = new CreateClientViewModel(_main));
            SearchCommand = new RelayCommand(o => ApplyFilter());
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredClients = new ObservableCollection<Client>(_main.Clients);
            }
            else
            {
                var search = SearchText.ToLower();
                var filtered = _main.Clients.Where(c =>
                    (c.Name?.ToLower().Contains(search) ?? false) ||
                    (c.Phone?.ToLower().Contains(search) ?? false) ||
                    (c.Email?.ToLower().Contains(search) ?? false)
                );
                FilteredClients = new ObservableCollection<Client>(filtered);
            }
        }
    }

    public class CreateClientViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        private string _address;
        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public CreateClientViewModel(MainViewModel main)
        {
            _main = main;

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(Name)) return;

                var client = new Client
                {
                    Id = _main.Clients.Any() ? _main.Clients.Max(c => c.Id) + 1 : 1,
                    Name = this.Name,
                    Phone = this.Phone,
                    Email = this.Email,
                    Address = this.Address,
                    Notes = this.Notes
                };

                _main.Clients.Add(client);
                _main.CurrentView = new ClientListViewModel(_main);
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new ClientListViewModel(_main));
        }
    }
}
