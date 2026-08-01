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
            set
            {
                _filteredRepairs = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalRepairsAmount));
                OnPropertyChanged(nameof(RepairsCount));
            }
        }

        public decimal TotalRepairsAmount => FilteredRepairs?.Sum(r => r.Total) ?? 0;
        public int RepairsCount => FilteredRepairs?.Count ?? 0;

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
        public ICommand ShowDetailCommand { get; }
        public ICommand ChangeStatusCommand { get; }

        public RepairListViewModel(MainViewModel main)
        {
            _main = main;
            FilteredRepairs = new ObservableCollection<RepairOrder>(Repairs);

            Repairs.CollectionChanged += (s, e) => ApplyFilter();

            FilterCommand = new RelayCommand(o => ApplyFilter());
            ShowCreateRepairCommand = new RelayCommand(o => _main.CurrentView = new CreateRepairViewModel(_main));

            ShowDetailCommand = new RelayCommand(o =>
            {
                if (o is RepairOrder r)
                {
                    _main.CurrentView = new RepairDetailViewModel(_main, r);
                }
            });

            ChangeStatusCommand = new RelayCommand(o =>
            {
                if (o is RepairOrder repair)
                {
                    string targetStatus = repair.Status == "Recibido" ? "Entregado" : "Recibido";

                    // Si vamos a entregar, mostramos el campo de monto opcional
                    bool isDelivering = targetStatus == "Entregado";
                    var pinDialog = new Views.PinValidationDialog(isDelivering, isDelivering ? repair.Total : null);

                    if (pinDialog.ShowDialog() == true)
                    {
                        // VALIDACIÓN DE CAJA (Bloqueo si hay cobro sin caja abierta)
                        bool isSessionOpen = _main.Session != null && _main.Session.IsOpen;
                        decimal finalAmount = pinDialog.Amount ?? (isDelivering ? repair.Total : 0);

                        if (finalAmount > 0 && !isSessionOpen)
                        {
                            System.Windows.MessageBox.Show(
                                "No se puede registrar un cobro sin una caja abierta.\nPor favor, abra caja primero.",
                                "Caja Cerrada",
                                System.Windows.MessageBoxButton.OK,
                                System.Windows.MessageBoxImage.Warning);
                            return;
                        }

                        var user = _main.Users.FirstOrDefault(u => u.Pin == pinDialog.Pin);
                        if (user == null)
                        {
                            System.Windows.MessageBox.Show("PIN incorrecto.", "Seguridad", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                            return;
                        }

                        string oldStatus = repair.Status;
                        decimal oldTotal = repair.Total;
                        repair.Status = targetStatus;

                        if (isDelivering && pinDialog.Amount.HasValue)
                        {
                            repair.Total = pinDialog.Amount.Value;
                        }

                        // Actualizar sesión de caja si hay una abierta y el total cambió
                        if (isSessionOpen && repair.Total != oldTotal)
                        {
                            _main.Session.RepairsTotal += (repair.Total - oldTotal);
                        }
                        else if (isSessionOpen && isDelivering && oldTotal == 0 && repair.Total == 0)
                        {
                            // Si se entrega con 0 no hace falta sumar, pero si se entrega con algo que ya estaba,
                            // se asume que se cobra en ese momento si no se cobró antes.
                            // En este sistema, RepairsTotal en sesión parece sumar el total de la orden al crearla.
                        }

                        var history = new RepairStatusHistory
                        {
                            RepairOrderId = repair.Id,
                            Status = repair.Status,
                            ChangeDate = DateTime.Now,
                            UserId = user.Id,
                            UserName = user.Name,
                            Notes = $"Cambio de estado por {user.Name}. De {oldStatus} a {repair.Status}. Monto: {repair.Total:C$ 0.00}"
                        };

                        _main.Db.RepairStatusHistories.Add(history);
                        _main.Db.SaveChanges();
                        ApplyFilter();
                        System.Windows.MessageBox.Show($"Estado actualizado a {repair.Status} por {user.Name}. Total: {repair.Total:C$ 0.00}", "Éxito");
                    }
                }
            });
        }

        private void ApplyFilter()
        {
            var filtered = Repairs.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var q = SearchText.Trim();
                var terms = q.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                filtered = filtered.Where(r =>
                    // Allow search by Id as fallback
                    r.Id.ToString().Contains(q) ||
                    // Search by client name (all terms must match)
                    (r.ClientName != null && terms.All(t => r.ClientName.Contains(t, StringComparison.OrdinalIgnoreCase))) ||
                    // Search by phone
                    (r.ClientPhone != null && terms.All(t => r.ClientPhone.Contains(t, StringComparison.OrdinalIgnoreCase))) ||
                    // Search by received model (brand + model combined)
                    (((r.DeviceBrand ?? string.Empty) + " " + (r.DeviceModel ?? string.Empty)).Trim() != string.Empty && terms.All(t => ((r.DeviceBrand ?? string.Empty) + " " + (r.DeviceModel ?? string.Empty)).Contains(t, StringComparison.OrdinalIgnoreCase))));
            }

            if (SelectedStatusFilter != "Todos los Estados")
            {
                filtered = filtered.Where(r => r.Status == SelectedStatusFilter);
            }

            FilteredRepairs = new ObservableCollection<RepairOrder>(filtered.OrderByDescending(r => r.EntryDate));
        }
    }

    public class RepairDetailViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        public RepairOrder Repair { get; }

        private string _status;
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        private decimal _total;
        public decimal Total
        {
            get => _total;
            set { _total = value; OnPropertyChanged(); }
        }

        private string _reportedFailure;
        public string ReportedFailure
        {
            get => _reportedFailure;
            set { _reportedFailure = value; OnPropertyChanged(); }
        }

        private User _selectedTechnician;
        public User SelectedTechnician
        {
            get => _selectedTechnician;
            set { _selectedTechnician = value; OnPropertyChanged(); }
        }

        public ObservableCollection<User> Technicians => new ObservableCollection<User>(_main.Users);

        public ICommand BackCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand SaveCommand { get; }

        public RepairDetailViewModel(MainViewModel main, RepairOrder repair)
        {
            _main = main;
            Repair = repair;

            // Cargar valores actuales a las propiedades de edición
            Status = repair.Status;
            Total = repair.Total;
            ReportedFailure = repair.ReportedFailure;
            SelectedTechnician = _main.Users.FirstOrDefault(u => u.Id == repair.TechnicianId);

            BackCommand = new RelayCommand(o => _main.CurrentView = new RepairListViewModel(_main));
            PrintCommand = new RelayCommand(o => { /* printing not implemented */ });

            SaveCommand = new RelayCommand(o =>
            {
                // VALIDACIÓN DE CAJA (Bloqueo si hay monto y caja cerrada)
                bool isSessionOpen = _main.Session != null && _main.Session.IsOpen;
                if (Total > 0 && !isSessionOpen)
                {
                    System.Windows.MessageBox.Show(
                        "No se puede guardar una reparación con monto mayor a 0 sin una caja abierta.",
                        "Caja Cerrada",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Solo guardamos si el usuario presiona el botón
                string oldStatus = Repair.Status;
                decimal oldTotal = Repair.Total;

                Repair.Status = Status;
                Repair.Total = Total;
                Repair.ReportedFailure = ReportedFailure;
                Repair.TechnicianId = SelectedTechnician?.Id;
                Repair.TechnicianName = SelectedTechnician?.Name;

                // Actualizar sesión de caja si el total cambió
                if (isSessionOpen && Repair.Total != oldTotal)
                {
                    _main.Session.RepairsTotal += (Repair.Total - oldTotal);
                }

                if (oldStatus != Status)
                {
                    var history = new RepairStatusHistory
                    {
                        RepairOrderId = Repair.Id,
                        Status = Status,
                        ChangeDate = DateTime.Now,
                        UserId = _main.CurrentUser?.Id ?? 1,
                        UserName = _main.CurrentUser?.Name ?? "Admin",
                        Notes = $"Estado cambiado de {oldStatus} a {Status} desde el detalle."
                    };
                    Repair.StatusHistory.Add(history);
                    _main.Db.RepairStatusHistories.Add(history);
                }

                _main.Db.SaveChanges();
                System.Windows.MessageBox.Show("Cambios guardados correctamente.", "Éxito");
                _main.CurrentView = new RepairListViewModel(_main);
            });
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

        private string _sellerPin;
        public string SellerPin
        {
            get => _sellerPin;
            set
            {
                _sellerPin = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SellerNamePreview));
            }
        }

        public string SellerNamePreview
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SellerPin))
                    return _main.CurrentUser?.Name ?? "Usuario";
                var user = _main.Users.FirstOrDefault(u => u.Pin == SellerPin.Trim());
                return user?.Name ?? "PIN no válido";
            }
        }

        private User _selectedTechnician;
        public User SelectedTechnician
        {
            get => _selectedTechnician;
            set { _selectedTechnician = value; OnPropertyChanged(); }
        }

        public ObservableCollection<User> Technicians => new ObservableCollection<User>(_main.Users);

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
                // Only ClientName and ClientPhone are required
                if (string.IsNullOrWhiteSpace(ClientName) || string.IsNullOrWhiteSpace(ClientPhone)) return;

                // VALIDACIÓN DE CAJA (Opción 1: Bloqueo si hay cobro sin caja abierta)
                bool isSessionOpen = _main.Session != null && _main.Session.IsOpen;
                if (Total > 0 && !isSessionOpen)
                {
                    System.Windows.MessageBox.Show(
                        "No se puede registrar una reparación con monto mayor a 0 sin una caja abierta.\n\n" +
                        "Por favor, abra caja primero o deje el total en 0.00 (solo diagnóstico).",
                        "Caja Cerrada",
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Buscar usuario por PIN para auditoría
                var seller = _main.Users.FirstOrDefault(u => u.Pin == SellerPin?.Trim());
                int userId = seller?.Id ?? _main.CurrentUser?.Id ?? 1;
                string userName = seller?.Name ?? _main.CurrentUser?.Name ?? "Usuario";

                var repair = new RepairOrder
                {
                    EntryDate = DateTime.Now,
                    ClientName = ClientName,
                    ClientPhone = ClientPhone ?? "",
                    DeviceBrand = DeviceBrand ?? "",
                    DeviceModel = DeviceModel ?? "",
                    IMEI = IMEI ?? "",
                    FailureCategory = FailureCategory ?? "Ninguna",
                    ReportedFailure = ReportedFailure ?? "",
                    Total = Total,
                    Status = "Recibido",
                    ReceivedById = userId,
                    ReceivedByName = userName,
                    TechnicianId = SelectedTechnician?.Id,
                    TechnicianName = SelectedTechnician?.Name
                };

                _main.Db.Repairs.Add(repair);

                // Agregar historial inicial
                var history = new RepairStatusHistory
                {
                    Status = "Recibido",
                    ChangeDate = DateTime.Now,
                    UserId = userId,
                    UserName = userName,
                    Notes = $"Equipo recibido por {userName}"
                };
                repair.StatusHistory.Add(history);

                _main.Db.SaveChanges();
                _main.Repairs.Add(repair);

                // Actualizar sesión de caja si hay una abierta
                if (_main.Session != null && _main.Session.IsOpen)
                {
                    _main.Session.RepairsTotal += repair.Total;
                }

                // Show the detail view for the newly created repair
                _main.CurrentView = new RepairDetailViewModel(_main, repair);
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
