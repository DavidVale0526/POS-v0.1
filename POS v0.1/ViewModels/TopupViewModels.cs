using System;
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
        }

        private void ApplyFilter()
        {
            var filtered = Topups.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(t =>
                    t.PhoneNumber.Contains(SearchText) ||
                    t.Company.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
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

        private string _selectedCompany = "TIGO";
        public string SelectedCompany
        {
            get => _selectedCompany;
            set { _selectedCompany = value; OnPropertyChanged(); }
        }

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

        public ICommand SelectTigoCommand { get; }
        public ICommand SelectClaroCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public CreateTopupViewModel(MainViewModel main)
        {
            _main = main;

            SelectTigoCommand = new RelayCommand(o => SelectedCompany = "TIGO");
            SelectClaroCommand = new RelayCommand(o => SelectedCompany = "CLARO");

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(PhoneNumber) || Amount <= 0) return;

                var topup = new Topup
                {
                    DateTime = DateTime.Now,
                    Company = SelectedCompany,
                    PhoneNumber = PhoneNumber,
                    Amount = Amount,
                    Cost = Cost,
                    Notes = Notes
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
