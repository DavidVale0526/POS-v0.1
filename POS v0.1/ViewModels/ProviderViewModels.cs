using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class ProvidersViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        public ObservableCollection<Provider> Providers => _main.Providers;

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private ObservableCollection<Provider> _filteredProviders;
        public ObservableCollection<Provider> FilteredProviders
        {
            get => _filteredProviders;
            set { _filteredProviders = value; OnPropertyChanged(); }
        }

        public ICommand ShowCreateProviderCommand { get; }
        public ICommand EditProviderCommand { get; }
        public ICommand DeleteProviderCommand { get; }

        public ProvidersViewModel(MainViewModel main)
        {
            _main = main;
            FilteredProviders = new ObservableCollection<Provider>(_main.Providers);

            ShowCreateProviderCommand = new RelayCommand(o => _main.CurrentView = new CreateProviderViewModel(_main));

            EditProviderCommand = new RelayCommand(o =>
            {
                if (o is Provider provider) _main.CurrentView = new CreateProviderViewModel(_main, provider);
            });

            DeleteProviderCommand = new RelayCommand(o =>
            {
                if (o is Provider provider)
                {
                    _main.Providers.Remove(provider);
                    var dbProvider = _main.Db.Providers.Find(provider.Id);
                    if (dbProvider != null) _main.Db.Providers.Remove(dbProvider);
                    _main.Db.SaveChanges();
                    ApplyFilter();
                }
            });
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredProviders = new ObservableCollection<Provider>(_main.Providers);
            }
            else
            {
                var filtered = _main.Providers.Where(p =>
                    (p.Name != null && p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (p.ContactName != null && p.ContactName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Phone != null && p.Phone.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                ).ToList();
                FilteredProviders = new ObservableCollection<Provider>(filtered);
            }
        }
    }

    public class CreateProviderViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private Provider _editingProvider;

        public string Title => _editingProvider == null ? "Nuevo Proveedor" : "Editar Proveedor";

        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _contactName;
        public string ContactName { get => _contactName; set { _contactName = value; OnPropertyChanged(); } }

        private string _phone;
        public string Phone { get => _phone; set { _phone = value; OnPropertyChanged(); } }

        private string _email;
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }

        private string _address;
        public string Address { get => _address; set { _address = value; OnPropertyChanged(); } }

        private string _city;
        public string City { get => _city; set { _city = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public CreateProviderViewModel(MainViewModel main, Provider provider = null)
        {
            _main = main;
            _editingProvider = provider;

            if (_editingProvider != null)
            {
                Name = _editingProvider.Name;
                ContactName = _editingProvider.ContactName;
                Phone = _editingProvider.Phone;
                Email = _editingProvider.Email;
                Address = _editingProvider.Address;
                City = _editingProvider.City;
                Description = _editingProvider.Description;
            }

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(Name)) return;

                if (_editingProvider == null)
                {
                    var newProvider = new Provider
                    {
                        Name = Name,
                        ContactName = ContactName,
                        Phone = Phone,
                        Email = Email,
                        Address = Address,
                        City = City,
                        Description = Description
                    };
                    _main.Db.Providers.Add(newProvider);
                    _main.Db.SaveChanges();
                    _main.Providers.Add(newProvider);
                }
                else
                {
                    _editingProvider.Name = Name;
                    _editingProvider.ContactName = ContactName;
                    _editingProvider.Phone = Phone;
                    _editingProvider.Email = Email;
                    _editingProvider.Address = Address;
                    _editingProvider.City = City;
                    _editingProvider.Description = Description;
                    _main.Db.SaveChanges();
                }
                _main.CurrentView = new ProvidersViewModel(_main);
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new ProvidersViewModel(_main));
        }
    }
}
