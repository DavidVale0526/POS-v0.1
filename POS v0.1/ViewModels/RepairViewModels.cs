using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
                    if (repair.Status == "Entregado")
                    {
                        var confirm = MessageBox.Show($"La orden #{repair.Id} ya está entregada. ¿Desea reabrirla para edición?", "Reabrir Orden", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (confirm == MessageBoxResult.Yes)
                        {
                            var user = _main.Users.FirstOrDefault(u => u.Id == _main.CurrentUser?.Id);
                            if (user == null || user.Role != "Administrador")
                            {
                                var pinDialog = new Views.PinValidationDialog();
                                if (pinDialog.ShowDialog() != true) return;
                                user = _main.Users.FirstOrDefault(u => u.Pin == pinDialog.Pin && u.Role == "Administrador");
                                if (user == null)
                                {
                                    MessageBox.Show("Se requiere PIN de Administrador para reabrir órdenes.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }

                            string oldStatus = repair.Status;
                            repair.Status = "Recibido";

                            var history = new RepairStatusHistory
                            {
                                RepairOrderId = repair.Id,
                                Status = repair.Status,
                                ChangeDate = DateTime.Now,
                                UserId = user.Id,
                                UserName = user.Name,
                                Notes = $"Orden reabierta por {user.Name}. Estado cambió de {oldStatus} a {repair.Status}."
                            };

                            _main.Db.RepairStatusHistories.Add(history);
                            _main.Db.SaveChanges();
                            ApplyFilter();
                            _main.ShowToast("Orden reabierta.");
                        }
                        return;
                    }

                    // Flujo de Entrega (solo si está Recibido)
                    var deliverPinDialog = new Views.PinValidationDialog(true, repair.Total);
                    if (deliverPinDialog.ShowDialog() == true)
                    {
                        bool isSessionOpen = _main.Session != null && _main.Session.IsOpen;
                        decimal finalAmount = deliverPinDialog.Amount ?? repair.Total;

                        if (finalAmount > 0 && !isSessionOpen)
                        {
                            MessageBox.Show("No se puede cobrar sin caja abierta.", "Caja Cerrada", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        var user = _main.Users.FirstOrDefault(u => u.Pin == deliverPinDialog.Pin);
                        if (user == null)
                        {
                            MessageBox.Show("PIN incorrecto.", "Seguridad", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Guardar datos de entrega y trazabilidad
                        repair.Status = "Entregado";
                        repair.Total = finalAmount;
                        repair.DeliveredById = user.Id;
                        repair.DeliveredByName = user.Name;
                        repair.DeliveryDate = DateTime.Now;

                        var history = new RepairStatusHistory
                        {
                            RepairOrderId = repair.Id,
                            Status = repair.Status,
                            ChangeDate = DateTime.Now,
                            UserId = user.Id,
                            UserName = user.Name,
                            Notes = $"Entregado y cobrado por {user.Name}."
                        };

                        _main.Db.RepairStatusHistories.Add(history);

                        // Actualizar sesión de caja (Solo en el campo de reparaciones)
                        if (isSessionOpen)
                        {
                            _main.Session.RepairsTotal += finalAmount;
                        }

                        _main.Db.SaveChanges();
                        ApplyFilter();
                        _main.ShowToast("Reparación entregada con éxito.");
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
                    r.Id.ToString().Contains(q) ||
                    (r.ClientName != null && terms.All(t => r.ClientName.Contains(t, StringComparison.OrdinalIgnoreCase))) ||
                    (r.ClientPhone != null && terms.All(t => r.ClientPhone.Contains(t, StringComparison.OrdinalIgnoreCase))) ||
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

        private string _clientName;
        public string ClientName
        {
            get => _clientName;
            set { _clientName = value; OnPropertyChanged(); }
        }

        private string _clientPhone;
        public string ClientPhone
        {
            get => _clientPhone;
            set { _clientPhone = value; OnPropertyChanged(); }
        }

        private string _deviceBrand;
        public string DeviceBrand
        {
            get => _deviceBrand;
            set { _deviceBrand = value; OnPropertyChanged(); }
        }

        private string _deviceModel;
        public string DeviceModel
        {
            get => _deviceModel;
            set { _deviceModel = value; OnPropertyChanged(); }
        }

        private string _imei;
        public string IMEI
        {
            get => _imei;
            set { _imei = value; OnPropertyChanged(); }
        }

        private User _selectedTechnician;
        public User SelectedTechnician
        {
            get => _selectedTechnician;
            set { _selectedTechnician = value; OnPropertyChanged(); }
        }

        public ObservableCollection<User> Technicians => new ObservableCollection<User>(_main.Users);
        public ObservableCollection<RepairPart> Parts { get; }

        public ICommand BackCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AddPartCommand { get; }
        public ICommand RemovePartCommand { get; }

        public RepairDetailViewModel(MainViewModel main, RepairOrder repair)
        {
            _main = main;
            Repair = repair;

            // Cargar repuestos explícitamente desde la DB para evitar desincronización
            var dbParts = _main.Db.RepairParts.Where(p => p.RepairOrderId == repair.Id).ToList();
            Parts = new ObservableCollection<RepairPart>(dbParts);

            Status = repair.Status;
            Total = repair.Total;
            ReportedFailure = repair.ReportedFailure;
            ClientName = repair.ClientName;
            ClientPhone = repair.ClientPhone;
            DeviceBrand = repair.DeviceBrand;
            DeviceModel = repair.DeviceModel;
            IMEI = repair.IMEI;
            SelectedTechnician = _main.Users.FirstOrDefault(u => u.Id == repair.TechnicianId);

            BackCommand = new RelayCommand(o => _main.CurrentView = new RepairListViewModel(_main));

            PrintCommand = new RelayCommand(o =>
            {
                var printer = new Services.TicketPrinter();
                printer.PrintRepairTicket(Repair);
            });

            AddPartCommand = new RelayCommand(o =>
            {
                var searchDialog = new Views.ProductSearchDialog(_main.Products.ToList());
                searchDialog.Owner = Application.Current.MainWindow;
                if (searchDialog.ShowDialog() == true)
                {
                    var prod = searchDialog.SelectedProduct;
                    if (prod.Stock <= 0)
                    {
                        MessageBox.Show("No hay stock disponible para este producto.", "Sin Stock", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var part = new RepairPart
                    {
                        RepairOrderId = Repair.Id,
                        ProductId = prod.Id,
                        ProductName = prod.Name,
                        Quantity = 1,
                        UnitPrice = prod.SellingPrice
                    };
                    Parts.Add(part);
                    Total += part.Subtotal;
                }
            });

            RemovePartCommand = new RelayCommand(o =>
            {
                if (o is RepairPart part)
                {
                    Parts.Remove(part);
                    Total -= part.Subtotal;
                }
            });

            SaveCommand = new RelayCommand(o =>
            {
                bool isSessionOpen = _main.Session != null && _main.Session.IsOpen;
                if (Total > 0 && !isSessionOpen)
                {
                    System.Windows.MessageBox.Show("No se puede guardar una reparación con monto mayor a 0 sin una caja abierta.", "Caja Cerrada", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Sincronizar Repuestos (Eliminados)
                var currentDbParts = _main.Db.RepairParts.Where(p => p.RepairOrderId == Repair.Id).ToList();
                foreach (var oldPart in currentDbParts)
                {
                    if (!Parts.Any(p => p.Id == oldPart.Id))
                    {
                        var prod = _main.Db.Products.Find(oldPart.ProductId);
                        if (prod != null)
                        {
                            prod.Stock += oldPart.Quantity;
                            var memProd = _main.Products.FirstOrDefault(p => p.Id == prod.Id);
                            if (memProd != null) memProd.Stock = prod.Stock;
                            _main.RecordStockMovement(prod.Id, oldPart.Quantity, "Reparación", $"Retorno por eliminación de repuesto en Orden #{Repair.Id}");
                        }
                        _main.Db.RepairParts.Remove(oldPart);
                    }
                }

                // Sincronizar Repuestos (Nuevos o Actualizados)
                foreach (var part in Parts)
                {
                    if (part.Id == 0) // Nuevo
                    {
                        var prod = _main.Db.Products.Find(part.ProductId);
                        if (prod != null)
                        {
                            prod.Stock -= part.Quantity;
                            var memProd = _main.Products.FirstOrDefault(p => p.Id == prod.Id);
                            if (memProd != null) memProd.Stock = prod.Stock;
                            _main.RecordStockMovement(prod.Id, -part.Quantity, "Reparación", $"Consumo de repuesto en Orden #{Repair.Id}");
                        }
                        _main.Db.RepairParts.Add(part);
                        Repair.Parts.Add(part);
                    }
                }

                string oldStatus = Repair.Status;
                decimal oldTotal = Repair.Total;

                Repair.Status = Status;
                Repair.Total = Total;
                Repair.ReportedFailure = ReportedFailure;
                Repair.ClientName = ClientName;
                Repair.ClientPhone = ClientPhone;
                Repair.DeviceBrand = DeviceBrand;
                Repair.DeviceModel = DeviceModel;
                Repair.IMEI = IMEI;
                Repair.TechnicianId = SelectedTechnician?.Id;
                Repair.TechnicianName = SelectedTechnician?.Name;

                if (isSessionOpen && Repair.Status == "Entregado" && Repair.Total != oldTotal)
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
                    _main.Db.RepairStatusHistories.Add(history);
                }

                _main.Db.SaveChanges();
                _main.ShowToast("Reparación guardada correctamente.");
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
            set { _sellerPin = value; OnPropertyChanged(); OnPropertyChanged(nameof(SellerNamePreview)); }
        }

        public string SellerNamePreview
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SellerPin)) return _main.CurrentUser?.Name ?? "Usuario";
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

        public ObservableCollection<string> Categories { get; } = new ObservableCollection<string> { "Ninguna / No especificada", "Pantalla", "Batería", "Centro de Carga", "Software", "Otros" };

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public CreateRepairViewModel(MainViewModel main)
        {
            _main = main;
            FailureCategory = "Ninguna / No especificada";

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(ClientName) || string.IsNullOrWhiteSpace(ClientPhone)) return;

                bool isSessionOpen = _main.Session != null && _main.Session.IsOpen;
                if (Total > 0 && !isSessionOpen)
                {
                    System.Windows.MessageBox.Show("No se puede registrar una reparación con monto mayor a 0 sin una caja abierta.\n\nPor favor, abra caja primero o deje el total en 0.00 (solo diagnóstico).", "Caja Cerrada", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                var seller = _main.Users.FirstOrDefault(u => u.Pin == SellerPin?.Trim());
                int userId = seller?.Id ?? _main.CurrentUser?.Id ?? 1;
                string userName = seller?.Name ?? _main.CurrentUser?.Name ?? "Usuario";

                var repair = new RepairOrder { EntryDate = DateTime.Now, ClientName = ClientName, ClientPhone = ClientPhone ?? "", DeviceBrand = DeviceBrand ?? "", DeviceModel = DeviceModel ?? "", IMEI = IMEI ?? "", FailureCategory = FailureCategory ?? "Ninguna", ReportedFailure = ReportedFailure ?? "", Total = Total, Status = "Recibido", ReceivedById = userId, ReceivedByName = userName, TechnicianId = SelectedTechnician?.Id, TechnicianName = SelectedTechnician?.Name };
                _main.Db.Repairs.Add(repair);
                var history = new RepairStatusHistory { Status = "Recibido", ChangeDate = DateTime.Now, UserId = userId, UserName = userName, Notes = $"Equipo recibido por {userName}" };
                repair.StatusHistory.Add(history);
                _main.Db.SaveChanges();
                _main.Repairs.Add(repair);
                _main.CurrentView = new RepairDetailViewModel(_main, repair);
            });

            BackCommand = new RelayCommand(o =>
            {
                if (!string.IsNullOrWhiteSpace(ClientName) || !string.IsNullOrWhiteSpace(DeviceModel)) {
                    var res = System.Windows.MessageBox.Show("Hay una reparación en progreso. ¿Desea salir sin guardar?", "Confirmar", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
                    if (res == System.Windows.MessageBoxResult.No) return;
                }
                _main.CurrentView = new RepairListViewModel(_main);
            });
        }
    }

    public class RepairCategoriesViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private RepairCategory _editingCategory;

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
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand BackCommand { get; }

        public RepairCategoriesViewModel(MainViewModel main)
        {
            _main = main;

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(NewName)) return;

                if (_editingCategory == null)
                {
                    var category = new RepairCategory { Name = NewName, Description = NewDescription ?? "" };
                    _main.Db.RepairCategories.Add(category);
                    _main.Db.SaveChanges();
                    Categories.Add(category);
                }
                else
                {
                    _editingCategory.Name = NewName;
                    _editingCategory.Description = NewDescription;
                    _main.Db.SaveChanges();
                    _editingCategory = null;
                }

                NewName = string.Empty;
                NewDescription = string.Empty;
                _main.ShowToast("Categoría guardada.");
            });

            EditCommand = new RelayCommand(o =>
            {
                if (o is RepairCategory cat)
                {
                    _editingCategory = cat;
                    NewName = cat.Name;
                    NewDescription = cat.Description;
                }
            });

            DeleteCommand = new RelayCommand(o =>
            {
                if (o is RepairCategory cat)
                {
                    if (MessageBox.Show($"¿Eliminar categoría {cat.Name}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        _main.Db.RepairCategories.Remove(cat);
                        _main.Db.SaveChanges();
                        Categories.Remove(cat);
                    }
                }
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new RepairListViewModel(_main));
        }
    }

    public class RepairTypesViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private RepairType _editingType;

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
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand BackCommand { get; }

        public RepairTypesViewModel(MainViewModel main)
        {
            _main = main;

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(NewName) || SelectedCategory == null) return;

                if (_editingType == null)
                {
                    var type = new RepairType { CategoryId = SelectedCategory.Id, CategoryName = SelectedCategory.Name, Name = NewName, BasePrice = NewPrice, Description = NewDescription };
                    _main.Db.RepairTypes.Add(type);
                    _main.Db.SaveChanges();
                    Types.Add(type);
                }
                else
                {
                    _editingType.Name = NewName;
                    _editingType.BasePrice = NewPrice;
                    _editingType.Description = NewDescription;
                    _editingType.CategoryId = SelectedCategory.Id;
                    _editingType.CategoryName = SelectedCategory.Name;
                    _main.Db.SaveChanges();
                    _editingType = null;
                }

                NewName = string.Empty;
                NewPrice = 0;
                NewDescription = string.Empty;
                SelectedCategory = null;
                _main.ShowToast("Tipo de reparación guardado.");
            });

            EditCommand = new RelayCommand(o =>
            {
                if (o is RepairType rt)
                {
                    _editingType = rt;
                    NewName = rt.Name;
                    NewPrice = rt.BasePrice;
                    NewDescription = rt.Description;
                    SelectedCategory = Categories.FirstOrDefault(c => c.Id == rt.CategoryId);
                }
            });

            DeleteCommand = new RelayCommand(o =>
            {
                if (o is RepairType rt)
                {
                    if (MessageBox.Show($"¿Eliminar tipo {rt.Name}?", "Confirmar", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        _main.Db.RepairTypes.Remove(rt);
                        _main.Db.SaveChanges();
                        Types.Remove(rt);
                    }
                }
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new RepairListViewModel(_main));
        }
    }
}
