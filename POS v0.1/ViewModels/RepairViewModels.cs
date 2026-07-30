using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class RepairListViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public ObservableCollection<RepairOrder> Repairs => _main.Repairs;

        private ObservableCollection<RepairOrder> _filteredRepairs;
        public ObservableCollection<RepairOrder> FilteredRepairs
        {
            get => _filteredRepairs;
            set { _filteredRepairs = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private string _selectedStatusFilter = "Todos los Estados";
        public string SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set { _selectedStatusFilter = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public ObservableCollection<string> StatusFilters { get; } = new ObservableCollection<string>
        {
            "Todos los Estados",
            "Recibido",
            "Entregado"
        };

        public ICommand FilterCommand { get; }
        public ICommand ShowCreateRepairCommand { get; }
        public ICommand ChangeStatusCommand { get; }

        public RepairListViewModel(MainViewModel main)
        {
            _main = main;
            FilteredRepairs = new ObservableCollection<RepairOrder>(Repairs);

            Repairs.CollectionChanged += (s, e) => ApplyFilter();

            FilterCommand = new RelayCommand(o => ApplyFilter());
            ShowCreateRepairCommand = new RelayCommand(o => _main.CurrentView = new CreateRepairViewModel(_main));

            ChangeStatusCommand = new RelayCommand(o =>
            {
                if (o is RepairOrder repair)
                {
                    repair.Status = repair.Status == "Recibido" ? "Entregado" : "Recibido";
                    _main.Db.SaveChanges();
                    ApplyFilter();
                }
            });
        }

        private void ApplyFilter()
        {
            var filtered = Repairs.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(r =>
                    r.Id.ToString().Contains(SearchText) ||
                    (r.ClientName != null && r.ClientName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (r.ClientPhone != null && r.ClientPhone.Contains(SearchText)) ||
                    (r.DeviceModel != null && r.DeviceModel.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            if (SelectedStatusFilter != "Todos los Estados")
            {
                filtered = filtered.Where(r => r.Status == SelectedStatusFilter);
            }

            FilteredRepairs = new ObservableCollection<RepairOrder>(filtered.OrderByDescending(r => r.EntryDate));
        }
    }

    public class CreateRepairViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceModel { get; set; }
        public string IMEI { get; set; }
        public string FailureCategory { get; set; }
        public string ReportedFailure { get; set; }
        public decimal Total { get; set; }

        public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>
        {
            "Ninguna / No especificada",
            "Pantalla",
            "Batería",
            "Centro de Carga",
            "Software",
            "Otros"
        };

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public CreateRepairViewModel(MainViewModel main)
        {
            _main = main;
            FailureCategory = "Ninguna / No especificada";

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(DeviceBrand) || string.IsNullOrWhiteSpace(DeviceModel)) return;

                var repair = new RepairOrder
                {
                    EntryDate = DateTime.Now,
                    ClientName = string.IsNullOrWhiteSpace(ClientName) ? "Consumidor Final" : ClientName,
                    ClientPhone = ClientPhone,
                    DeviceBrand = DeviceBrand,
                    DeviceModel = DeviceModel,
                    IMEI = IMEI,
                    FailureCategory = FailureCategory,
                    ReportedFailure = ReportedFailure,
                    Total = Total,
                    Status = "Recibido"
                };

                _main.Db.Repairs.Add(repair);
                _main.Db.SaveChanges();
                _main.Repairs.Add(repair);
                _main.CurrentView = new RepairListViewModel(_main);
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new RepairListViewModel(_main));
        }
    }

    public class RepairCategoriesViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public ObservableCollection<RepairCategory> Categories => _main.RepairCategories;

        private string _newName;
        public string NewName
        {
            get => _newName;
            set { _newName = value; OnPropertyChanged(); }
        }

        private string _newDescription;
        public string NewDescription
        {
            get => _newDescription;
            set { _newDescription = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public RepairCategoriesViewModel(MainViewModel main)
        {
            _main = main;

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(NewName)) return;

                var category = new RepairCategory
                {
                    Name = NewName,
                    Description = NewDescription
                };

                _main.Db.RepairCategories.Add(category);
                _main.Db.SaveChanges();
                Categories.Add(category);
                NewName = string.Empty;
                NewDescription = string.Empty;
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new RepairListViewModel(_main));
        }
    }

    public class RepairTypesViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public ObservableCollection<RepairType> Types => _main.RepairTypes;
        public ObservableCollection<RepairCategory> Categories => _main.RepairCategories;

        private string _newName;
        public string NewName
        {
            get => _newName;
            set { _newName = value; OnPropertyChanged(); }
        }

        private decimal _newPrice;
        public decimal NewPrice
        {
            get => _newPrice;
            set { _newPrice = value; OnPropertyChanged(); }
        }

        private string _newDescription;
        public string NewDescription
        {
            get => _newDescription;
            set { _newDescription = value; OnPropertyChanged(); }
        }

        private RepairCategory _selectedCategory;
        public RepairCategory SelectedCategory
        {
            get => _selectedCategory;
            set { _selectedCategory = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public RepairTypesViewModel(MainViewModel main)
        {
            _main = main;

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(NewName) || SelectedCategory == null) return;

                var type = new RepairType
                {
                    CategoryId = SelectedCategory.Id,
                    CategoryName = SelectedCategory.Name,
                    Name = NewName,
                    BasePrice = NewPrice,
                    Description = NewDescription
                };

                _main.Db.RepairTypes.Add(type);
                _main.Db.SaveChanges();
                Types.Add(type);
                NewName = string.Empty;
                NewPrice = 0;
                NewDescription = string.Empty;
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new RepairListViewModel(_main));
        }
    }
}
