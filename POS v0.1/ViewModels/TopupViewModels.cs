using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class TopupHistoryViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public ObservableCollection<Topup> Topups => _main.Topups;

        private ObservableCollection<Topup> _filteredTopups;
        public ObservableCollection<Topup> FilteredTopups
        {
            get => _filteredTopups;
            set { _filteredTopups = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); }
        }

        public decimal TotalSoldToday => Topups.Where(t => t.DateTime.Date == DateTime.Today).Sum(t => t.Amount);

        public ICommand FilterCommand { get; }
        public ICommand ShowCreateTopupCommand { get; }
        public ICommand ManageCompaniesCommand { get; }

        public TopupHistoryViewModel(MainViewModel main)
        {
            _main = main;
            FilteredTopups = new ObservableCollection<Topup>(Topups);

            Topups.CollectionChanged += (s, e) => {
                OnPropertyChanged(nameof(TotalSoldToday));
                ApplyFilter();
            };

            FilterCommand = new RelayCommand(o => ApplyFilter());
            ShowCreateTopupCommand = new RelayCommand(o => _main.CurrentView = new CreateTopupViewModel(_main));
            ManageCompaniesCommand = new RelayCommand(o =>
            {
                var dialog = new Views.ManageTopupCompaniesDialog(_main);
                dialog.ShowDialog();
            });
        }

        private void ApplyFilter()
        {
            var filtered = Topups.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(t =>
                    t.PhoneNumber.Contains(SearchText) ||
                    (t.Company != null && t.Company.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            if (SelectedDate.HasValue)
            {
                filtered = filtered.Where(t => t.DateTime.Date == SelectedDate.Value.Date);
            }

            FilteredTopups = new ObservableCollection<Topup>(filtered.OrderByDescending(t => t.DateTime));
        }
    }

    public class CreateTopupViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public ObservableCollection<TopupCompany> Companies => _main.TopupCompanies;
        public ObservableCollection<TopupPreset> Presets => _main.TopupPresets;

        private TopupCompany _selectedCompany;
        public TopupCompany SelectedCompany
        {
            get => _selectedCompany;
            set
            {
                _selectedCompany = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FilteredPresets));
            }
        }

        public IEnumerable<TopupPreset> FilteredPresets =>
            Presets.Where(p => p.CompanyName == SelectedCompany?.Name);

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set { _phoneNumber = value; OnPropertyChanged(); }
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); }
        }

        private decimal _cost;
        public decimal Cost
        {
            get => _cost;
            set { _cost = value; OnPropertyChanged(); }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ApplyPresetCommand { get; }

        public CreateTopupViewModel(MainViewModel main)
        {
            _main = main;

            SelectedCompany = _main.TopupCompanies.FirstOrDefault();

            ApplyPresetCommand = new RelayCommand(o =>
            {
                if (o is TopupPreset preset)
                {
                    Amount = preset.Amount;
                    Cost = preset.Cost;
                }
            });

            SaveCommand = new RelayCommand(o =>
            {
                if (SelectedCompany == null || string.IsNullOrWhiteSpace(PhoneNumber) || Amount <= 0) return;

                var topup = new Topup
                {
                    DateTime = DateTime.Now,
                    Company = SelectedCompany.Name,
                    PhoneNumber = PhoneNumber,
                    Amount = Amount,
                    Cost = Cost,
                    Notes = Notes,
                    Seller = _main.CurrentUser?.Name ?? "Usuario"
                };

                _main.Db.Topups.Add(topup);
                _main.Db.SaveChanges();
                _main.Topups.Add(topup);

                // Actualizar sesión de caja si hay una abierta
                if (_main.Session != null && _main.Session.IsOpen)
                {
                    _main.Session.TopupsTotal += topup.Amount;
                }

                _main.CurrentView = new TopupHistoryViewModel(_main);
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new TopupHistoryViewModel(_main));
        }
    }
}
