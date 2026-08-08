using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using POS_v0_1.Models;
using POS_v0_1.Data;
using POS_v0_1.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace POS_v0_1.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
                CheckModuleAccess();
            }
        }

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsAdmin));
                NotifyPermissions();
            }
        }

        private bool _isAuthenticated;
        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set { _isAuthenticated = value; OnPropertyChanged(); }
        }

        public bool IsAdmin => CurrentUser?.Role == "Administrador";

        // Dynamic Permissions for Sidebar
        public bool CanSeeSalesHistory => IsAdmin || (BusinessConfig?.AllowUserSalesHistory ?? false);
        public bool CanSeeInventory => IsAdmin || (BusinessConfig?.AllowUserInventory ?? false);
        public bool CanSeePurchases => IsAdmin || (BusinessConfig?.AllowUserPurchases ?? false);
        public bool CanSeeRepairs => IsAdmin || (BusinessConfig?.AllowUserRepairs ?? false);
        public bool CanSeeTreasury => IsAdmin || (BusinessConfig?.AllowUserTreasury ?? false);
        public bool CanSeeClients => IsAdmin || (BusinessConfig?.AllowUserClients ?? false);
        public bool CanSeeReports => IsAdmin || (BusinessConfig?.AllowUserReports ?? false);

        public void NotifyPermissions()
        {
            OnPropertyChanged(nameof(CanSeeSalesHistory));
            OnPropertyChanged(nameof(CanSeeInventory));
            OnPropertyChanged(nameof(CanSeePurchases));
            OnPropertyChanged(nameof(CanSeeRepairs));
            OnPropertyChanged(nameof(CanSeeTreasury));
            OnPropertyChanged(nameof(CanSeeClients));
            OnPropertyChanged(nameof(CanSeeReports));
        }

        public AppDbContext Db { get; } = new AppDbContext();

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();
        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();
        public ObservableCollection<Provider> Providers { get; } = new ObservableCollection<Provider>();
        public ObservableCollection<Sale> Sales { get; } = new ObservableCollection<Sale>();
        public ObservableCollection<Purchase> Purchases { get; } = new ObservableCollection<Purchase>();
        public ObservableCollection<Topup> Topups { get; } = new ObservableCollection<Topup>();
        public ObservableCollection<TopupCompany> TopupCompanies { get; } = new ObservableCollection<TopupCompany>();
        public ObservableCollection<RepairOrder> Repairs { get; } = new ObservableCollection<RepairOrder>();
        public ObservableCollection<Client> Clients { get; } = new ObservableCollection<Client>();
        public ObservableCollection<FinancialMovement> FinancialMovements { get; } = new ObservableCollection<FinancialMovement>();
        public ObservableCollection<FinancialSource> FinancialSources { get; } = new ObservableCollection<FinancialSource>();
        public ObservableCollection<User> Users { get; } = new ObservableCollection<User>();
        public ObservableCollection<StockMovement> StockMovements { get; } = new ObservableCollection<StockMovement>();
        public ObservableCollection<CashSession> CashSessions { get; } = new ObservableCollection<CashSession>();
        public ObservableCollection<BankTransaction> BankTransactions { get; } = new ObservableCollection<BankTransaction>();
        public ObservableCollection<CashCount> CashCounts { get; } = new ObservableCollection<CashCount>();
        public ObservableCollection<RepairCategory> RepairCategories { get; } = new ObservableCollection<RepairCategory>();
        public ObservableCollection<RepairType> RepairTypes { get; } = new ObservableCollection<RepairType>();
        public ObservableCollection<TopupPreset> TopupPresets { get; } = new ObservableCollection<TopupPreset>();

        private BusinessConfiguration _businessConfig;
        public BusinessConfiguration BusinessConfig
        {
            get => _businessConfig;
            set { _businessConfig = value; OnPropertyChanged(); NotifyPermissions(); }
        }

        private CashSession _session;
        public CashSession Session
        {
            get => _session;
            set { _session = value; OnPropertyChanged(); }
        }

        private PosViewModel _posViewModel;
        public PosViewModel PosViewModel
        {
            get
            {
                if (_posViewModel == null) _posViewModel = new PosViewModel(this);
                return _posViewModel;
            }
        }

        public void ResetPosViewModel()
        {
            _posViewModel = new PosViewModel(this);
            OnPropertyChanged(nameof(PosViewModel));
        }

        private bool _isSidebarCollapsed;
        public bool IsSidebarCollapsed
        {
            get => _isSidebarCollapsed;
            set { _isSidebarCollapsed = value; OnPropertyChanged(); }
        }

        private ToastMessage _currentToast;
        public ToastMessage CurrentToast
        {
            get => _currentToast;
            set { _currentToast = value; OnPropertyChanged(); }
        }

        public void ShowToast(string message, string type = "Success")
        {
            CurrentToast = new ToastMessage { Message = message, Type = type };
            var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            timer.Tick += (s, e) =>
            {
                CurrentToast = null;
                timer.Stop();
            };
            timer.Start();
        }

        public ICommand ToggleSidebarCommand { get; }
        public ICommand ShowProductManagementCommand { get; }
        public ICommand ShowDashboardCommand { get; }
        public ICommand ShowOpenCashCommand { get; }
        public ICommand ShowPosCommand { get; }
        public ICommand ShowCategoriesCommand { get; }
        public ICommand ShowLowStockCommand { get; }
        public ICommand ShowProvidersCommand { get; }
        public ICommand ShowPurchasesCommand { get; }
        public ICommand ShowSalesHistoryCommand { get; }
        public ICommand ShowTopupsCommand { get; }
        public ICommand ShowRepairsCommand { get; }
        public ICommand ShowClientsCommand { get; }
        public ICommand ShowRepairCategoriesCommand { get; }
        public ICommand ShowRepairTypesCommand { get; }
        public ICommand ShowCashCountCommand { get; }
        public ICommand ShowExpensesIncomesCommand { get; }
        public ICommand ShowBankCommand { get; }
        public ICommand ShowReportsCommand { get; }
        public ICommand ShowUsersRolesCommand { get; }
        public ICommand ShowConfigurationCommand { get; }
        public ICommand ShowStockHistoryCommand { get; }
        public ICommand OpenFolderCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand SwitchUserCommand { get; }

        public MainViewModel()
        {
            ToggleSidebarCommand = new RelayCommand(o => IsSidebarCollapsed = !IsSidebarCollapsed);
            SwitchUserCommand = new RelayCommand(o => SwitchUser());
            ShowDashboardCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new DashboardViewModel(this)));
            ShowProductManagementCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new ProductsViewModel(this)));
            ShowOpenCashCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new CashControlViewModel(this)));
            ShowPosCommand = new RelayCommand(o => ExecuteIfAuthenticated(() =>
            {
                if (Session != null && Session.IsOpen)
                {
                    CurrentView = PosViewModel;
                }
                else
                {
                    CurrentView = new CashControlViewModel(this);
                }
            }));

            // New Commands
            ShowCategoriesCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new CategoriesViewModel(this)));
            ShowLowStockCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new LowStockViewModel(this)));
            ShowProvidersCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new ProvidersViewModel(this)));
            ShowPurchasesCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new PurchasesViewModel(this)));
            ShowSalesHistoryCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new SalesHistoryViewModel(this)));
            ShowTopupsCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new TopupHistoryViewModel(this)));
            ShowRepairsCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new RepairListViewModel(this)));
            ShowClientsCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new ClientListViewModel(this)));
            ShowRepairCategoriesCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new RepairCategoriesViewModel(this)));
            ShowRepairTypesCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new RepairTypesViewModel(this)));
            ShowCashCountCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new CashCountHistoryViewModel(this)));
            ShowExpensesIncomesCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new FinancialMovementsViewModel(this)));
            ShowBankCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new BankHistoryViewModel(this)));
            ShowReportsCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new ReportsViewModel(this)));
            ShowUsersRolesCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new UserManagementViewModel(this)));
            ShowConfigurationCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => CurrentView = new ConfigurationViewModel(this)));
            OpenFolderCommand = new RelayCommand(o => ExecuteIfAuthenticated(() => {
                try {
                    string folder = o?.ToString() == "Logs" ? "Logs" : "Backups";
                    string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folder);
                    if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
                    System.Diagnostics.Process.Start("explorer.exe", path);
                } catch (Exception ex) { MessageBox.Show("Error al abrir carpeta: " + ex.Message); }
            }));
            ShowStockHistoryCommand = new RelayCommand(o => ExecuteIfAuthenticated(() =>
            {
                if (o is int prodId) CurrentView = new StockHistoryViewModel(this, prodId);
                else CurrentView = new StockHistoryViewModel(this);
            }));

            // Initialize Database
            try
            {
                Db.Database.EnsureCreated();
                ApplySchemaUpdates();
                LoadDataFromDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error crítico al iniciar la base de datos: {ex.Message}", "Fallo de Inicio");
            }

            LogoutCommand = new RelayCommand(o =>
            {
                IsAuthenticated = false;
                CurrentUser = null;
                CurrentView = new LoginViewModel(this);
            });

            // Initial view
            try
            {
                if (!Users.Any())
                {
                    IsAuthenticated = false;
                    _currentView = new SetupWizardViewModel(this);
                }
                else
                {
                    IsAuthenticated = false;
                    _currentView = new LoginViewModel(this);
                }
                OnPropertyChanged(nameof(CurrentView));
            }
            catch
            {
                IsAuthenticated = false;
                _currentView = new LoginViewModel(this);
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        private void ApplySchemaUpdates()
        {
            AddColumnSafe("Sales", "PaymentMethod", "TEXT DEFAULT 'Efectivo'");
            AddColumnSafe("Sales", "ReceivedAmount", "DECIMAL DEFAULT 0");
            AddColumnSafe("Sales", "ChangeAmount", "DECIMAL DEFAULT 0");
            AddColumnSafe("Sales", "AmountCash", "DECIMAL DEFAULT 0");
            AddColumnSafe("Sales", "AmountCard", "DECIMAL DEFAULT 0");
            AddColumnSafe("Sales", "AmountTransfer", "DECIMAL DEFAULT 0");
            AddColumnSafe("Sales", "IsCancelled", "INTEGER DEFAULT 0");
            AddColumnSafe("Sales", "CancellationReason", "TEXT");
            AddColumnSafe("Sales", "GrossAmount", "DECIMAL DEFAULT 0");
            AddColumnSafe("Sales", "TotalDiscount", "DECIMAL DEFAULT 0");

            AddColumnSafe("SaleItems", "Discount", "DECIMAL DEFAULT 0");
            AddColumnSafe("SaleItems", "PurchasePrice", "DECIMAL DEFAULT 0");

            AddColumnSafe("Purchases", "Status", "TEXT DEFAULT 'Recibida'");
            AddColumnSafe("Purchases", "ProviderId", "INTEGER");

            AddColumnSafe("CashSessions", "CashTotal", "DECIMAL DEFAULT 0");
            AddColumnSafe("CashSessions", "CardTotal", "DECIMAL DEFAULT 0");
            AddColumnSafe("CashSessions", "TransferTotal", "DECIMAL DEFAULT 0");

            AddColumnSafe("Products", "CategoryId", "INTEGER DEFAULT 0");
            AddColumnSafe("Products", "ProviderId", "INTEGER");
            AddColumnSafe("Products", "SKU", "TEXT");
            AddColumnSafe("Products", "ImagePath", "TEXT");

            AddColumnSafe("FinancialMovements", "SourceId", "INTEGER DEFAULT 0");

            AddColumnSafe("BusinessConfigs", "AllowUserSalesHistory", "INTEGER DEFAULT 0");
            AddColumnSafe("BusinessConfigs", "AllowUserInventory", "INTEGER DEFAULT 0");
            AddColumnSafe("BusinessConfigs", "AllowUserPurchases", "INTEGER DEFAULT 0");
            AddColumnSafe("BusinessConfigs", "AllowUserRepairs", "INTEGER DEFAULT 1");
            AddColumnSafe("BusinessConfigs", "AllowUserTreasury", "INTEGER DEFAULT 0");
            AddColumnSafe("BusinessConfigs", "AllowUserClients", "INTEGER DEFAULT 0");
            AddColumnSafe("BusinessConfigs", "AllowUserReports", "INTEGER DEFAULT 0");
            AddColumnSafe("BusinessConfigs", "AutoBackupEnabled", "INTEGER DEFAULT 1");
            AddColumnSafe("BusinessConfigs", "LastBackupDate", "TEXT");
            AddColumnSafe("BusinessConfigs", "MaxDiscountWithoutPin", "DECIMAL DEFAULT 15");

            AddColumnSafe("Users", "SecurityQuestion1", "TEXT");
            AddColumnSafe("Users", "SecurityAnswer1", "TEXT");
            AddColumnSafe("Users", "SecurityQuestion2", "TEXT");
            AddColumnSafe("Users", "SecurityAnswer2", "TEXT");

            try
            {
                Db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS RepairParts (Id INTEGER PRIMARY KEY AUTOINCREMENT, RepairOrderId INTEGER NOT NULL, ProductId INTEGER NOT NULL, ProductName TEXT, Quantity INTEGER NOT NULL, UnitPrice DECIMAL NOT NULL);");
                Db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS StockMovements (Id INTEGER PRIMARY KEY AUTOINCREMENT, ProductId INTEGER NOT NULL, QuantityChange INTEGER NOT NULL, NewStock INTEGER NOT NULL, MovementType TEXT NOT NULL, Reason TEXT, Date TEXT NOT NULL, UserId INTEGER, UserName TEXT);");
                Db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS CreditNotes (Id INTEGER PRIMARY KEY AUTOINCREMENT, SaleId INTEGER NOT NULL, Date TEXT NOT NULL, Amount DECIMAL NOT NULL, Reason TEXT, CustomerName TEXT);");
                Db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS TopupCompanies (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT NOT NULL, Color TEXT);");
                Db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS TopupPresets (Id INTEGER PRIMARY KEY AUTOINCREMENT, CompanyName TEXT NOT NULL, Amount DECIMAL NOT NULL, Cost DECIMAL NOT NULL);");
            }
            catch { }
        }

        private void AddColumnSafe(string table, string column, string definition)
        {
            try
            {
                using (var conn = Db.Database.GetDbConnection())
                {
                    if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"PRAGMA table_info({table})";
                        bool exists = false;
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader["name"].ToString().Equals(column, StringComparison.OrdinalIgnoreCase))
                                {
                                    exists = true; break;
                                }
                            }
                        }
                        if (!exists)
                        {
                            cmd.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {definition}";
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch { }
        }

        // Compatibilidad
        private void AddColumnDirect(string t, string d) { /* Obsoleto */ }

        private void LoadDataFromDatabase()
        {
            try
            {
                var usersFromDb = Db.Users.ToList();
                foreach (var u in usersFromDb) Users.Add(u);
            }
            catch { }

            try { foreach (var m in Db.StockMovements.Include(x => x.Product).OrderByDescending(x => x.Date).Take(500)) StockMovements.Add(m); } catch { }
            try { foreach (var c in Db.Categories.Include(x => x.Products)) Categories.Add(c); } catch { }
            try { foreach (var p in Db.Products.Include(x => x.Category).Include(x => x.Provider)) Products.Add(p); } catch { }
            try { foreach (var pr in Db.Providers) Providers.Add(pr); } catch { }
            try { foreach (var s in Db.Sales.Include(x => x.Items)) Sales.Add(s); } catch { }

            try {
                var sources = Db.FinancialSources.ToList();
                if (!sources.Any()) {
                    sources.Add(new FinancialSource { Name = "Caja", Description = "Efectivo físico" });
                    sources.Add(new FinancialSource { Name = "Banco", Description = "Transferencias/Tarjetas" });
                    Db.FinancialSources.AddRange(sources); Db.SaveChanges();
                }
                foreach (var src in sources) FinancialSources.Add(src);
            } catch { }

            try { foreach (var fm in Db.FinancialMovements) FinancialMovements.Add(fm); } catch { }
            try { foreach (var cl in Db.Clients) Clients.Add(cl); } catch { }
            try { foreach (var r in Db.Repairs.Include(x => x.Parts)) Repairs.Add(r); } catch { }
            try { foreach (var t in Db.Topups) Topups.Add(t); } catch { }
            try { foreach (var bt in Db.BankTransactions) BankTransactions.Add(bt); } catch { }
            try { foreach (var cc in Db.CashCounts.Include(x => x.Details)) CashCounts.Add(cc); } catch { }
            try { foreach (var rc in Db.RepairCategories) RepairCategories.Add(rc); } catch { }
            try { foreach (var rt in Db.RepairTypes) RepairTypes.Add(rt); } catch { }
            try { foreach (var tp in Db.TopupPresets) TopupPresets.Add(tp); } catch { }
            try { foreach (var co in Db.TopupCompanies) TopupCompanies.Add(co); } catch { }

            try {
                BusinessConfig = Db.BusinessConfigs.FirstOrDefault();
                if (BusinessConfig == null) {
                    BusinessConfig = new BusinessConfiguration { BusinessName = "Mi Tienda", TaxRate = 0, TicketFormat = "58mm" };
                    Db.BusinessConfigs.Add(BusinessConfig); Db.SaveChanges();
                }
            } catch { }

            try {
                var dbSessions = Db.CashSessions.ToList();
                foreach (var s in dbSessions) CashSessions.Add(s);
                Session = CashSessions.FirstOrDefault(s => s.IsOpen);
            } catch { }
        }

        public void RecordStockMovement(int productId, int change, string type, string reason, string forcedUser = null)
        {
            var product = Products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return;
            var movement = new StockMovement { ProductId = productId, QuantityChange = change, NewStock = product.Stock, MovementType = type, Reason = reason, Date = DateTime.Now, UserId = CurrentUser?.Id, UserName = forcedUser ?? CurrentUser?.Name };
            Db.StockMovements.Add(movement); Db.SaveChanges();
            StockMovements.Insert(0, movement);
        }

        public bool RequestAdminValidation()
        {
            if (CurrentUser?.Role == "Administrador") return true;
            var dialog = new Views.PinValidationDialog();
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true) {
                var admin = Users.FirstOrDefault(u => u.Role == "Administrador" && u.Pin == dialog.Pin);
                if (admin != null) return true;
                else { MessageBox.Show("PIN incorrecto.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); return false; }
            }
            return false;
        }

        private void ExecuteIfAuthenticated(Action action)
        {
            if (IsAuthenticated)
            {
                action();
            }
        }

        private void SwitchUser()
        {
            var dialog = new Views.PinValidationDialog();
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.ShowDialog() == true) {
                var user = Users.FirstOrDefault(u => u.Pin == dialog.Pin);
                if (user != null)
                {
                    CurrentUser = user;
                    CurrentView = new DashboardViewModel(this); // Redirigir a Dashboard al cambiar usuario
                    CheckModuleAccess();
                    ShowToast($"Sesión: {user.Name}");
                }
                else { MessageBox.Show("PIN no válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            }
        }

        private void CheckModuleAccess()
        {
            if (CurrentUser == null) return;

            bool hasAccess = true;

            // 1. Bloqueo estricto de Vistas Administrativas
            if (CurrentUser.Role != "Administrador")
            {
                if (CurrentView is ConfigurationViewModel || CurrentView is UserManagementViewModel)
                {
                    hasAccess = false;
                }
            }

            // 2. Bloqueo por Permisos Dinámicos
            if (hasAccess && CurrentUser.Role != "Administrador")
            {
                var config = BusinessConfig;
                if (config != null)
                {
                    if (CurrentView is SalesHistoryViewModel && !config.AllowUserSalesHistory) hasAccess = false;
                    if (CurrentView is ProductsViewModel && !config.AllowUserInventory) hasAccess = false;
                    if (CurrentView is PurchasesViewModel && !config.AllowUserPurchases) hasAccess = false;
                    if (CurrentView is RepairListViewModel && !config.AllowUserRepairs) hasAccess = false;
                    if (CurrentView is CashControlViewModel && !config.AllowUserTreasury) hasAccess = false;
                    if (CurrentView is ClientListViewModel && !config.AllowUserClients) hasAccess = false;
                    if (CurrentView is ReportsViewModel && !config.AllowUserReports) hasAccess = false;
                }
            }

            if (!hasAccess)
            {
                // Usamos el campo privado para evitar recursión en el setter de CurrentView
                _currentView = (Session != null && Session.IsOpen) ? PosViewModel : (object)new RepairListViewModel(this);
                OnPropertyChanged(nameof(CurrentView));

                MessageBox.Show("Su rol actual no tiene permiso para acceder a esta vista. Redirigiendo...",
                                "Acceso Restringido", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    public class CashControlViewModel : BaseViewModel
    {
        private MainViewModel _main;
        public CashSession Session => _main.Session;
        public ObservableCollection<CashSession> History => _main.CashSessions;
        private decimal _openingAmount;
        public decimal OpeningAmount { get => _openingAmount; set { _openingAmount = value; OnPropertyChanged(); } }
        public ICommand OpenCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ViewDetailCommand { get; }
        public CashControlViewModel(MainViewModel main)
        {
            _main = main;
            OpenCommand = new RelayCommand(o => {
                if (!_main.RequestAdminValidation()) return;
                var newSession = new CashSession { IsOpen = true, OpenedBy = _main.CurrentUser?.Name ?? "Admin", OpeningTime = DateTime.Now, OpeningBalance = OpeningAmount };
                _main.Db.CashSessions.Add(newSession); _main.Db.SaveChanges();
                _main.CashSessions.Add(newSession); _main.Session = newSession; _main.ResetPosViewModel(); _main.CurrentView = _main.PosViewModel;
            });
            CloseCommand = new RelayCommand(o => {
                if (Session == null || !_main.RequestAdminValidation()) return;
                var currentSession = this.Session;
                _main.CurrentView = new NewCashCountViewModel(_main, savedCount => {
                    try {
                        currentSession.ClosingTime = DateTime.Now; currentSession.ClosingBalance = savedCount.TotalCalculated; currentSession.IsOpen = false; currentSession.ClosedBy = _main.CurrentUser?.Name ?? "Usuario";
                        _main.Db.SaveChanges(); _main.Session = null; OnPropertyChanged(nameof(Session));
                    } catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
                });
            });
            ViewDetailCommand = new RelayCommand(o => { if (o is CashSession sess) _main.CurrentView = new CashSessionDetailViewModel(_main, sess); });
        }
    }

    public class CashSessionDetailViewModel : BaseViewModel
    {
        private MainViewModel _main;
        public CashSession Session { get; }
        public Models.CashCount CashCount { get; private set; }
        public decimal TotalPhysical => CashCount?.TotalCalculated ?? 0m;
        public ICommand BackCommand { get; }
        public CashSessionDetailViewModel(MainViewModel main, CashSession session)
        {
            _main = main; Session = session; BackCommand = new RelayCommand(o => _main.CurrentView = new CashControlViewModel(_main));
            try { CashCount = _main.Db.CashCounts.Include(c => c.Details).Where(c => c.DateTime >= Session.OpeningTime && (Session.ClosingTime == null || c.DateTime <= Session.ClosingTime.Value)).OrderByDescending(c => c.DateTime).FirstOrDefault(); } catch { }
        }
    }

    public class OpenCashViewModel : BaseViewModel
    {
        private MainViewModel _main;
        public decimal Amount { get; set; }
        public ICommand OpenCommand { get; }
        public OpenCashViewModel(MainViewModel main) { _main = main; OpenCommand = new RelayCommand(o => { _main.Session = new CashSession { IsOpen = true, OpeningBalance = Amount, OpeningTime = DateTime.Now }; _main.ResetPosViewModel(); _main.CurrentView = _main.PosViewModel; }); }
    }

    public class PosViewModel : BaseViewModel
    {
        private MainViewModel _main;
        private decimal _taxRate;
        public ObservableCollection<Product> Products => _main.Products;
        public ObservableCollection<CartItem> Cart { get; } = new ObservableCollection<CartItem>();
        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); ApplyFilter(); } }
        private ObservableCollection<Product> _filteredProducts;
        public ObservableCollection<Product> FilteredProducts { get => _filteredProducts; set { _filteredProducts = value; OnPropertyChanged(); } }
        public decimal Subtotal => Cart.Sum(i => i.Subtotal);
        private decimal _discount;
        public decimal Discount { get => _discount; set { decimal temp = IsPercentageDiscount ? (Subtotal * (value / 100m)) : value; if (Subtotal - temp < 0) return; _discount = value; OnPropertyChanged(); OnPropertyChanged(nameof(Iva)); OnPropertyChanged(nameof(Total)); OnPropertyChanged(nameof(ChangeAmount)); } }
        private bool _isPercentageDiscount;
        public bool IsPercentageDiscount { get => _isPercentageDiscount; set { _isPercentageDiscount = value; OnPropertyChanged(); OnPropertyChanged(nameof(Iva)); OnPropertyChanged(nameof(Total)); OnPropertyChanged(nameof(ChangeAmount)); } }
        public decimal DiscountValue => IsPercentageDiscount ? (Subtotal * (Discount / 100m)) : Discount;
        public decimal Iva => Math.Round((Subtotal - DiscountValue) * (_taxRate / 100m), 2);
        public decimal Total => Subtotal - DiscountValue + Iva;
        public ObservableCollection<Client> Clients => _main.Clients;
        private Client? _selectedClient;
        public Client? SelectedClient { get => _selectedClient; set { _selectedClient = value; OnPropertyChanged(); if (value != null) CustomerNameText = value.Name; } }
        private string _customerNameText = "Consumidor Final";
        public string CustomerNameText { get => _customerNameText; set { _customerNameText = value; OnPropertyChanged(); } }
        private string _paymentMethod = "Efectivo";
        public string PaymentMethod { get => _paymentMethod; set { _paymentMethod = value; OnPropertyChanged(); } }
        private decimal _receivedAmount;
        public decimal ReceivedAmount { get => _receivedAmount; set { _receivedAmount = value; OnPropertyChanged(); OnPropertyChanged(nameof(ChangeAmount)); } }
        public decimal ChangeAmount => Math.Max(0, ReceivedAmount - Total);
        private string _sellerPin;
        public string SellerPin { get => _sellerPin; set { _sellerPin = value; OnPropertyChanged(); OnPropertyChanged(nameof(SellerNamePreview)); } }
        public string SellerNamePreview { get { if (string.IsNullOrWhiteSpace(SellerPin)) return _main.CurrentUser?.Name ?? "Usuario"; var seller = _main.Users.FirstOrDefault(u => u.Pin?.Trim() == SellerPin.Trim()); return seller?.Name ?? _main.CurrentUser?.Name ?? "Usuario"; } }
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand HandleScannedBarcodeCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand SellCommand { get; }
        public ICommand QuickAddClientCommand { get; }

        public PosViewModel(MainViewModel main)
        {
            _main = main; _main.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(_main.CurrentUser)) OnPropertyChanged(nameof(SellerNamePreview)); };
            var config = _main.Db.BusinessConfigs.FirstOrDefault(); _taxRate = config?.TaxRate ?? 0m;
            FilteredProducts = new ObservableCollection<Product>(_main.Products);

            Cart.CollectionChanged += (s, e) => {
                if (e.NewItems != null) foreach (CartItem item in e.NewItems) item.PropertyChanged += CartItem_PropertyChanged;
                if (e.OldItems != null) foreach (CartItem item in e.OldItems) item.PropertyChanged -= CartItem_PropertyChanged;
                OnPropertyChanged(nameof(Subtotal)); OnPropertyChanged(nameof(Iva)); OnPropertyChanged(nameof(Total)); OnPropertyChanged(nameof(ChangeAmount));
            };

            AddToCartCommand = new RelayCommand(p => { if (p is Product prod) AddProductToCart(prod); });
            RemoveItemCommand = new RelayCommand(o => { if (o is CartItem item) { Cart.Remove(item); } });
            IncreaseQuantityCommand = new RelayCommand(o => { if (o is CartItem item) { if (item.Product.Stock > item.Quantity) item.Quantity++; else MessageBox.Show("Sin stock."); } });
            HandleScannedBarcodeCommand = new RelayCommand(o => { if (o is string code) HandleBarcodeScan(code.Trim()); });
            DecreaseQuantityCommand = new RelayCommand(o => { if (o is CartItem item && item.Quantity > 1) item.Quantity--; });
            ClearCartCommand = new RelayCommand(o => { Cart.Clear(); Discount = 0; CustomerNameText = "Consumidor Final"; SelectedClient = null; });
            QuickAddClientCommand = new RelayCommand(o => { var dialog = new Views.QuickClientDialog(); if (dialog.ShowDialog() == true) { var client = dialog.Client; _main.Db.Clients.Add(client); _main.Db.SaveChanges(); _main.Clients.Add(client); SelectedClient = client; } });
            SellCommand = new RelayCommand(o => {
                if (!Cart.Any()) return;

                // Validación de Descuento Máximo
                decimal maxDiscountPct = _main.BusinessConfig?.MaxDiscountWithoutPin ?? 15m;
                bool needsAdmin = false;
                if (Subtotal > 0 && (DiscountValue / Subtotal) * 100 > maxDiscountPct) needsAdmin = true;
                foreach (var item in Cart) {
                    decimal itemBase = item.Product.SellingPrice * item.Quantity;
                    if (itemBase > 0 && (item.Discount / itemBase) * 100 > maxDiscountPct) { needsAdmin = true; break; }
                }
                if (needsAdmin && !_main.RequestAdminValidation()) return;

                decimal cash = 0, card = 0, transfer = 0;
                if (PaymentMethod == "Mixto") {
                    var mixed = new Views.MixedPaymentDialog(Total); mixed.Owner = Application.Current.MainWindow;
                    if (mixed.ShowDialog() == true) { cash = mixed.CashAmount; card = mixed.CardAmount; transfer = mixed.TransferAmount; } else return;
                } else {
                    if (PaymentMethod == "Efectivo") { cash = Total; if (ReceivedAmount < Total) { MessageBox.Show("Monto insuficiente."); return; } }
                    else if (PaymentMethod == "Tarjeta") card = Total;
                    else if (PaymentMethod == "Transferencia") transfer = Total;
                }
                var pinActual = SellerPin?.Trim(); var seller = _main.Users.FirstOrDefault(u => u.Pin == pinActual);
                string nFinal = seller?.Name ?? _main.CurrentUser?.Name ?? "Usuario"; string pFinal = seller?.Pin ?? _main.CurrentUser?.Pin ?? "0000";

                decimal grossAmount = Cart.Sum(i => i.Product.SellingPrice * i.Quantity);
                decimal itemDiscounts = Cart.Sum(i => i.Discount);
                decimal totalDiscount = itemDiscounts + DiscountValue;

                var newSale = new Sale {
                    Date = DateTime.Now,
                    SellerPin = pFinal,
                    SellerName = nFinal,
                    CustomerName = string.IsNullOrWhiteSpace(CustomerNameText) ? "Consumidor Final" : CustomerNameText,
                    GrossAmount = grossAmount,
                    Subtotal = Subtotal,
                    Discount = DiscountValue,
                    TotalDiscount = totalDiscount,
                    Iva = Iva,
                    Total = Total,
                    PaymentMethod = PaymentMethod,
                    ReceivedAmount = ReceivedAmount,
                    ChangeAmount = ChangeAmount,
                    AmountCash = cash,
                    AmountCard = card,
                    AmountTransfer = transfer,
                    Items = new ObservableCollection<SaleItem>(Cart.Select(i => new SaleItem { ProductId = i.Product.Id, ProductName = i.Product.Name, Quantity = i.Quantity, UnitPrice = i.Product.SellingPrice, PurchasePrice = i.Product.PurchasePrice, Discount = i.Discount }))
                };
                foreach (var item in Cart) {
                    var dbProd = _main.Db.Products.Find(item.Product.Id);
                    if (dbProd != null) { dbProd.Stock -= item.Quantity; var memProd = _main.Products.FirstOrDefault(p => p.Id == item.Product.Id); if (memProd != null) memProd.Stock = dbProd.Stock; _main.RecordStockMovement(item.Product.Id, -item.Quantity, "Venta", $"Venta a {newSale.CustomerName}", nFinal); }
                }
                _main.Db.Sales.Add(newSale);

                // Actualizar sesión ANTES de guardar para que persista
                if (_main.Session != null && _main.Session.IsOpen) {
                    _main.Session.SalesTotal += Total;
                    _main.Session.CashTotal += cash;
                    _main.Session.CardTotal += card;
                    _main.Session.TransferTotal += transfer;
                }

                _main.Db.SaveChanges();

                try { var printer = new TicketPrinter(); printer.PrintSaleTicket(newSale); } catch { }
                _main.Sales.Add(newSale);

                SellerPin = string.Empty; ReceivedAmount = 0; Discount = 0; IsPercentageDiscount = false; SelectedClient = null; CustomerNameText = "Consumidor Final";
                _main.ShowToast("Venta realizada."); ClearCartCommand.Execute(null);
            });
        }
        private void ApplyFilter() { if (string.IsNullOrWhiteSpace(SearchText)) FilteredProducts = new ObservableCollection<Product>(_main.Products); else FilteredProducts = new ObservableCollection<Product>(_main.Products.Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || (p.Barcode != null && p.Barcode.Contains(SearchText)))); }
        public void AddProductToCart(Product prod, int quantity = 1) {
            if (prod == null || prod.Status != "Activo" || prod.Stock <= 0) return;
            var existing = Cart.FirstOrDefault(i => i.Product.Id == prod.Id);
            if (existing != null) { if (existing.Quantity + quantity > prod.Stock) return; existing.Quantity += quantity; }
            else Cart.Add(new CartItem(prod) { Quantity = quantity });
        }
        public void HandleBarcodeScan(string code) { var prod = _main.Products.FirstOrDefault(p => p.Barcode != null && p.Barcode.Trim() == code); if (prod != null) AddProductToCart(prod, 1); }

        private void CartItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CartItem.Subtotal))
            {
                OnPropertyChanged(nameof(Subtotal));
                OnPropertyChanged(nameof(Iva));
                OnPropertyChanged(nameof(Total));
                OnPropertyChanged(nameof(ChangeAmount));
            }
        }
    }

    public class PlaceholderViewModel : BaseViewModel { public string Title { get; set; } public PlaceholderViewModel(string title) { Title = title; } }
}
