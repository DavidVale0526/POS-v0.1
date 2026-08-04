using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using POS_v0_1.Models;
using POS_v0_1.Data;
using POS_v0_1.Services;
using Microsoft.EntityFrameworkCore;

namespace POS_v0_1.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        private bool _isAuthenticated;
        public bool IsAuthenticated
        {
            get => _isAuthenticated;
            set { _isAuthenticated = value; OnPropertyChanged(); }
        }

        private User _currentUser;
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                RefreshPermissions();
            }
        }

        private void RefreshPermissions()
        {
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(CanSeeSalesHistory));
            OnPropertyChanged(nameof(CanSeeInventory));
            OnPropertyChanged(nameof(CanSeePurchases));
            OnPropertyChanged(nameof(CanSeeRepairs));
            OnPropertyChanged(nameof(CanSeeTreasury));
            OnPropertyChanged(nameof(CanSeeClients));
            OnPropertyChanged(nameof(CanSeeReports));
            OnPropertyChanged(nameof(CanSeeAdmin));
        }

        public bool IsAdmin => CurrentUser?.Role == "Administrador";

        public bool CanSeeSalesHistory => IsAdmin || (BusinessConfig?.AllowUserSalesHistory ?? false);
        public bool CanSeeInventory => IsAdmin || (BusinessConfig?.AllowUserInventory ?? false);
        public bool CanSeePurchases => IsAdmin || (BusinessConfig?.AllowUserPurchases ?? false);
        public bool CanSeeRepairs => IsAdmin || (BusinessConfig?.AllowUserRepairs ?? false);
        public bool CanSeeTreasury => IsAdmin || (BusinessConfig?.AllowUserTreasury ?? false);
        public bool CanSeeClients => IsAdmin || (BusinessConfig?.AllowUserClients ?? false);
        public bool CanSeeReports => IsAdmin || (BusinessConfig?.AllowUserReports ?? false);
        public bool CanSeeAdmin => IsAdmin;

        private BusinessConfiguration _businessConfig;
        public BusinessConfiguration BusinessConfig
        {
            get => _businessConfig;
            set { _businessConfig = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();
        public ObservableCollection<Provider> Providers { get; set; } = new ObservableCollection<Provider>();
        public ObservableCollection<Purchase> Purchases { get; set; } = new ObservableCollection<Purchase>();
        public ObservableCollection<Sale> Sales { get; set; } = new ObservableCollection<Sale>();
        public ObservableCollection<Topup> Topups { get; set; } = new ObservableCollection<Topup>();
        public ObservableCollection<TopupCompany> TopupCompanies { get; set; } = new ObservableCollection<TopupCompany>();
        public ObservableCollection<TopupPreset> TopupPresets { get; set; } = new ObservableCollection<TopupPreset>();
        public ObservableCollection<RepairOrder> Repairs { get; set; } = new ObservableCollection<RepairOrder>();
        public ObservableCollection<RepairCategory> RepairCategories { get; set; } = new ObservableCollection<RepairCategory>();
        public ObservableCollection<RepairType> RepairTypes { get; set; } = new ObservableCollection<RepairType>();
        public ObservableCollection<FinancialMovement> FinancialMovements { get; set; } = new ObservableCollection<FinancialMovement>();
        public ObservableCollection<FinancialSource> FinancialSources { get; set; } = new ObservableCollection<FinancialSource>();
        public ObservableCollection<CashCount> CashCounts { get; set; } = new ObservableCollection<CashCount>();
        public ObservableCollection<BankTransaction> BankTransactions { get; set; } = new ObservableCollection<BankTransaction>();
        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
        public ObservableCollection<CashSession> CashSessions { get; set; } = new ObservableCollection<CashSession>();

        public AppDbContext Db { get; } = new AppDbContext();

        private CashSession _session;
        public CashSession Session
        {
            get => _session;
            set { _session = value; OnPropertyChanged(); }
        }

        public ICommand ShowProductManagementCommand { get; }
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
        public ICommand LogoutCommand { get; }
        public ICommand SwitchUserCommand { get; }

        public MainViewModel()
        {
            SwitchUserCommand = new RelayCommand(o => SwitchUser());
            ShowProductManagementCommand = new RelayCommand(o => CurrentView = new ProductsViewModel(this));
            ShowOpenCashCommand = new RelayCommand(o => CurrentView = new CashControlViewModel(this));
            ShowPosCommand = new RelayCommand(o =>
            {
                if (Session != null && Session.IsOpen)
                    CurrentView = new PosViewModel(this);
                else
                    CurrentView = new CashControlViewModel(this);
            });

            // New Commands
            ShowCategoriesCommand = new RelayCommand(o => CurrentView = new CategoriesViewModel(this));
            ShowLowStockCommand = new RelayCommand(o => CurrentView = new LowStockViewModel(this));
            ShowProvidersCommand = new RelayCommand(o => CurrentView = new ProvidersViewModel(this));
            ShowPurchasesCommand = new RelayCommand(o => CurrentView = new PurchasesViewModel(this));
            ShowSalesHistoryCommand = new RelayCommand(o => CurrentView = new SalesHistoryViewModel(this));
            ShowTopupsCommand = new RelayCommand(o => CurrentView = new TopupHistoryViewModel(this));
            ShowRepairsCommand = new RelayCommand(o => CurrentView = new RepairListViewModel(this));
            ShowClientsCommand = new RelayCommand(o => CurrentView = new ClientListViewModel(this));
            ShowRepairCategoriesCommand = new RelayCommand(o => CurrentView = new RepairCategoriesViewModel(this));
            ShowRepairTypesCommand = new RelayCommand(o => CurrentView = new RepairTypesViewModel(this));
            ShowCashCountCommand = new RelayCommand(o => CurrentView = new CashCountHistoryViewModel(this));
            ShowExpensesIncomesCommand = new RelayCommand(o => CurrentView = new FinancialMovementsViewModel(this));
            ShowBankCommand = new RelayCommand(o => CurrentView = new BankHistoryViewModel(this));
            ShowReportsCommand = new RelayCommand(o => CurrentView = new ReportsViewModel(this));
            ShowUsersRolesCommand = new RelayCommand(o => CurrentView = new UserManagementViewModel(this));
            ShowConfigurationCommand = new RelayCommand(o => CurrentView = new ConfigurationViewModel(this));

            // Initialize Database
            // Db.Database.EnsureDeleted(); // Comentado para evitar pérdida de datos en cada inicio
            Db.Database.EnsureCreated();
            ApplySchemaUpdates();

            LoadDataFromDatabase();

            LogoutCommand = new RelayCommand(o =>
            {
                IsAuthenticated = false;
                CurrentUser = null;
                CurrentView = new LoginViewModel(this);
            });

            // Initial view
            IsAuthenticated = true;
            CurrentUser = Users.FirstOrDefault();
            CurrentView = new RepairListViewModel(this);

            /* MODO PRODUCCIÓN:
            IsAuthenticated = false;
            CurrentView = new LoginViewModel(this);
            */
        }

        private void ApplySchemaUpdates()
        {
            // SQLite no soporta 'ADD COLUMN IF NOT EXISTS', así que verificamos la existencia manualmente.
            AddColumnIfMissing("Sales", "PaymentMethod", "TEXT DEFAULT 'Efectivo'");
            AddColumnIfMissing("Sales", "ReceivedAmount", "DECIMAL DEFAULT 0");
            AddColumnIfMissing("Sales", "ChangeAmount", "DECIMAL DEFAULT 0");
            AddColumnIfMissing("Sales", "IsCancelled", "INTEGER DEFAULT 0");
            AddColumnIfMissing("Sales", "CancellationReason", "TEXT");
            AddColumnIfMissing("SaleItems", "Discount", "DECIMAL DEFAULT 0");
            AddColumnIfMissing("SaleItems", "PurchasePrice", "DECIMAL DEFAULT 0");
            AddColumnIfMissing("Purchases", "Status", "TEXT DEFAULT 'Recibida'");

            // 4.1 Nuevas Columnas para Integridad de Datos (FKs)
            AddColumnIfMissing("Products", "CategoryId", "INTEGER DEFAULT 0");
            AddColumnIfMissing("Products", "ProviderId", "INTEGER");
            AddColumnIfMissing("Purchases", "ProviderId", "INTEGER");
            AddColumnIfMissing("FinancialMovements", "SourceId", "INTEGER DEFAULT 0");

            // Permisos de Rol Usuario
            AddColumnIfMissing("BusinessConfigs", "AllowUserSalesHistory", "INTEGER DEFAULT 0");
            AddColumnIfMissing("BusinessConfigs", "AllowUserInventory", "INTEGER DEFAULT 0");
            AddColumnIfMissing("BusinessConfigs", "AllowUserPurchases", "INTEGER DEFAULT 0");
            AddColumnIfMissing("BusinessConfigs", "AllowUserRepairs", "INTEGER DEFAULT 1");
            AddColumnIfMissing("BusinessConfigs", "AllowUserTreasury", "INTEGER DEFAULT 0");
            AddColumnIfMissing("BusinessConfigs", "AllowUserClients", "INTEGER DEFAULT 0");
            AddColumnIfMissing("BusinessConfigs", "AllowUserReports", "INTEGER DEFAULT 0");

            // Migración: Llenar PurchasePrice para ventas viejas que no lo tienen (usando el costo actual del producto)
            try
            {
                Db.Database.ExecuteSqlRaw(@"
                    UPDATE SaleItems
                    SET PurchasePrice = (SELECT PurchasePrice FROM Products WHERE Products.Id = SaleItems.ProductId)
                    WHERE PurchasePrice = 0 OR PurchasePrice IS NULL
                ");

                // Migración 4.1: Vincular Texto a ID para integridad de datos
                Db.Database.ExecuteSqlRaw(@"
                    -- Vincular Categorías de Productos
                    UPDATE Products
                    SET CategoryId = (SELECT Id FROM Categories WHERE Categories.Name = Products.Category)
                    WHERE CategoryId = 0 OR CategoryId IS NULL;

                    -- Vincular Proveedores de Productos
                    UPDATE Products
                    SET ProviderId = (SELECT Id FROM Providers WHERE Providers.Name = Products.Provider)
                    WHERE ProviderId IS NULL;

                    -- Vincular Proveedores de Compras
                    UPDATE Purchases
                    SET ProviderId = (SELECT Id FROM Providers WHERE Providers.Name = Purchases.ProviderName)
                    WHERE ProviderId IS NULL;

                    -- Vincular Fuentes Financieras
                    UPDATE FinancialMovements
                    SET SourceId = (SELECT Id FROM FinancialSources WHERE FinancialSources.Name = FinancialMovements.Source)
                    WHERE SourceId = 0 OR SourceId IS NULL;
                ");

                // Asegurar que las nuevas tablas de la auditoría existan
                Db.Database.ExecuteSqlRaw(@"
                    CREATE TABLE IF NOT EXISTS RepairParts (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        RepairOrderId INTEGER NOT NULL,
                        ProductId INTEGER NOT NULL,
                        ProductName TEXT,
                        Quantity INTEGER NOT NULL,
                        UnitPrice DECIMAL NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS CreditNotes (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SaleId INTEGER NOT NULL,
                        Date TEXT NOT NULL,
                        Amount DECIMAL NOT NULL,
                        Reason TEXT,
                        CustomerName TEXT
                    );

                    CREATE TABLE IF NOT EXISTS TopupCompanies (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Color TEXT
                    );

                    CREATE TABLE IF NOT EXISTS TopupPresets (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CompanyName TEXT NOT NULL,
                        Amount DECIMAL NOT NULL,
                        Cost DECIMAL NOT NULL
                    );
                ");
            }
            catch { }
        }

        private void AddColumnIfMissing(string tableName, string columnName, string columnDefinition)
        {
            try
            {
                // PRAGMA table_info devuelve información sobre las columnas de la tabla
                var columns = Db.Database.SqlQueryRaw<string>($"SELECT name FROM pragma_table_info('{tableName}')").ToList();
                if (!columns.Contains(columnName, StringComparer.OrdinalIgnoreCase))
                {
                    Db.Database.ExecuteSqlRaw($"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error actualizando esquema ({tableName}.{columnName}): {ex.Message}");
            }
        }

        private void LoadDataFromDatabase()
        {
            // Load Users
            var dbUsers = Db.Users.ToList();
            if (!dbUsers.Any())
            {
                // Seed default users if none exist
                var admin = new User { Name = "Administrador", Email = "admin@admin.com", Password = "admin", Role = "Administrador", Pin = "1234", RegistrationDate = DateTime.Now };
                var seller = new User { Name = "Vendedor Juan", Email = "vendedor@admin.com", Password = "123", Role = "Usuario", Pin = "4321", RegistrationDate = DateTime.Now };
                var miki = new User { Name = "Miki", Email = "miki@admin.com", Password = "miki", Role = "Usuario", Pin = "7845", RegistrationDate = DateTime.Now };
                Db.Users.AddRange(admin, seller, miki);
                Db.SaveChanges();
                dbUsers = Db.Users.ToList();
            }
            foreach (var user in dbUsers) Users.Add(user);

            // Load Categories
            var dbCategories = Db.Categories.Include(c => c.Products).ToList();
            if (!dbCategories.Any())
            {
                Db.Categories.Add(new Category { Name = "Celulares", Description = "Dispositivos móviles" });
                Db.Categories.Add(new Category { Name = "Accesorios", Description = "Cargadores, cables, etc." });
                Db.SaveChanges();
                dbCategories = Db.Categories.Include(c => c.Products).ToList();
            }
            foreach (var cat in dbCategories) Categories.Add(cat);

            // Load Products
            var dbProducts = Db.Products.Include(p => p.Category).Include(p => p.Provider).ToList();
            if (!dbProducts.Any())
            {
                var category = Db.Categories.FirstOrDefault();
                Db.Products.Add(new Product
                {
                    Name = "Producto de Prueba",
                    SellingPrice = 100,
                    PurchasePrice = 70,
                    Stock = 50,
                    CategoryId = category?.Id ?? 1,
                    Barcode = "123456789"
                });
                Db.SaveChanges();
                dbProducts = Db.Products.Include(p => p.Category).Include(p => p.Provider).ToList();
            }
            foreach (var prod in dbProducts) Products.Add(prod);

            // Load Providers
            foreach (var prov in Db.Providers.ToList()) Providers.Add(prov);

            // Load Financial Sources
            var dbSources = Db.FinancialSources.Include(s => s.Movements).ToList();
            if (!dbSources.Any())
            {
                Db.FinancialSources.Add(new FinancialSource { Name = "Caja", Description = "Dinero en caja" });
                Db.FinancialSources.Add(new FinancialSource { Name = "Banco", Description = "Cuenta bancaria" });
                Db.SaveChanges();
                dbSources = Db.FinancialSources.Include(s => s.Movements).ToList();
            }
            foreach (var source in dbSources) FinancialSources.Add(source);

            // Load Other Data
            foreach (var sale in Db.Sales.Include(s => s.Items).ToList()) Sales.Add(sale);
            foreach (var client in Db.Clients.ToList()) Clients.Add(client);
            foreach (var repair in Db.Repairs.ToList()) Repairs.Add(repair);
            foreach (var topup in Db.Topups.ToList()) Topups.Add(topup);

            // Load Topups Companies/Presets logic
            try
            {
                var dbTopupCos = Db.TopupCompanies.ToList();
                if (!dbTopupCos.Any())
                {
                    Db.TopupCompanies.AddRange(
                        new TopupCompany { Name = "TIGO", Color = "#0033A0" },
                        new TopupCompany { Name = "CLARO", Color = "#EF3340" }
                    );
                    Db.SaveChanges();
                    dbTopupCos = Db.TopupCompanies.ToList();
                }
                foreach (var co in dbTopupCos) TopupCompanies.Add(co);

                var dbPresets = Db.TopupPresets.ToList();
                if (!dbPresets.Any() && TopupCompanies.Any())
                {
                    Db.TopupPresets.AddRange(
                        new TopupPreset { CompanyName = "TIGO", Amount = 10, Cost = 9.5m },
                        new TopupPreset { CompanyName = "TIGO", Amount = 20, Cost = 19m },
                        new TopupPreset { CompanyName = "CLARO", Amount = 10, Cost = 9.5m }
                    );
                    Db.SaveChanges();
                    dbPresets = Db.TopupPresets.ToList();
                }
                foreach (var preset in dbPresets) TopupPresets.Add(preset);
            }
            catch { }

            foreach (var movement in Db.FinancialMovements.Include(m => m.FinancialSource).ToList()) FinancialMovements.Add(movement);
            foreach (var count in Db.CashCounts.Include(c => c.Details).ToList()) CashCounts.Add(count);
            foreach (var bank in Db.BankTransactions.ToList()) BankTransactions.Add(bank);
            foreach (var purchase in Db.Purchases.Include(p => p.Items).Include(p => p.Provider).ToList()) Purchases.Add(purchase);
            foreach (var sess in Db.CashSessions.ToList()) CashSessions.Add(sess);

            // Set current active session if any
            Session = CashSessions.FirstOrDefault(s => s.IsOpen);

            // Load Business Configuration
            BusinessConfig = Db.BusinessConfigs.FirstOrDefault() ?? new BusinessConfiguration();
        }

        public void SaveChanges()
        {
            Db.SaveChanges();
        }

        private void SwitchUser()
        {
            var pinDialog = new Views.PinValidationDialog();
            if (pinDialog.ShowDialog() == true)
            {
                var user = Users.FirstOrDefault(u => u.Pin == pinDialog.Pin);
                if (user != null)
                {
                    CurrentUser = user;

                    // SEGURIDAD: Validar si el nuevo usuario tiene acceso a la vista actual
                    ValidateCurrentViewAccess();

                    // Forzar actualización de comandos si es necesario
                    CommandManager.InvalidateRequerySuggested();
                }
                else
                {
                    System.Windows.MessageBox.Show("PIN no reconocido.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }

        private void ValidateCurrentViewAccess()
        {
            if (IsAdmin) return; // Si es Admin, tiene acceso total

            bool needsRedirect = false;

            // Bloqueo total de vistas Administrativas (Solo Admin)
            if (CurrentView is UserManagementViewModel || CurrentView is CreateUserViewModel ||
                CurrentView is ConfigurationViewModel || CurrentView is BackupViewModel)
            {
                needsRedirect = true;
            }
            // Bloqueos basados en la configuración dinámica de permisos
            else if (CurrentView is ReportsViewModel)
                needsRedirect = !CanSeeReports;
            else if (CurrentView is SalesHistoryViewModel)
                needsRedirect = !CanSeeSalesHistory;
            else if (CurrentView is ProductsViewModel || CurrentView is CategoriesViewModel ||
                     CurrentView is LowStockViewModel || CurrentView is ProductDetailViewModel ||
                     CurrentView is CreateProductViewModel || CurrentView is RestockViewModel)
                needsRedirect = !CanSeeInventory;
            else if (CurrentView is ProvidersViewModel || CurrentView is CreateProviderViewModel ||
                     CurrentView is PurchasesViewModel || CurrentView is CreatePurchaseViewModel ||
                     CurrentView is PurchaseDetailViewModel)
                needsRedirect = !CanSeePurchases;
            else if (CurrentView is RepairListViewModel || CurrentView is CreateRepairViewModel ||
                     CurrentView is RepairDetailViewModel || CurrentView is RepairCategoriesViewModel ||
                     CurrentView is RepairTypesViewModel)
                needsRedirect = !CanSeeRepairs;
            else if (CurrentView is CashControlViewModel || CurrentView is OpenCashViewModel ||
                     CurrentView is CashCountHistoryViewModel || CurrentView is NewCashCountViewModel ||
                     CurrentView is FinancialMovementsViewModel || CurrentView is CreateFinancialMovementViewModel ||
                     CurrentView is FinancialSourcesViewModel || CurrentView is BankHistoryViewModel ||
                     CurrentView is NewBankTransactionViewModel)
                needsRedirect = !CanSeeTreasury;
            else if (CurrentView is ClientListViewModel || CurrentView is CreateClientViewModel)
                needsRedirect = !CanSeeClients;

            if (needsRedirect)
            {
                // Redirigir a una vista siempre segura (POS si hay caja, si no a Reparaciones)
                if (Session != null && Session.IsOpen)
                    CurrentView = new PosViewModel(this);
                else
                    CurrentView = new RepairListViewModel(this);

                System.Windows.MessageBox.Show("Tu perfil no tiene acceso a la vista anterior. Has sido redirigido.", "Control de Acceso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        public bool RequestAdminValidation()
        {
            if (IsAdmin) return true;

            var pinDialog = new Views.PinValidationDialog();
            if (pinDialog.ShowDialog() == true)
            {
                var user = Users.FirstOrDefault(u => u.Pin == pinDialog.Pin);
                if (user != null && user.Role == "Administrador")
                {
                    return true;
                }
                else
                {
                    System.Windows.MessageBox.Show("Acceso denegado. Se requiere PIN de Administrador.", "Seguridad", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
                }
            }
            return false;
        }
    }

    public class PlaceholderViewModel : BaseViewModel
    {
        public string Title { get; set; }
        public PlaceholderViewModel(string title) { Title = title; }
    }

    public class ProductManagementViewModel : BaseViewModel
    {
        private MainViewModel _main;
        public ObservableCollection<Product> Products => _main.Products;

        private string _newName;
        public string NewName { get => _newName; set { _newName = value; OnPropertyChanged(); } }

        private decimal _newPrice;
        public decimal NewPrice { get => _newPrice; set { _newPrice = value; OnPropertyChanged(); } }

        public ICommand AddProductCommand { get; }

        public ProductManagementViewModel(MainViewModel main)
        {
            _main = main;
            AddProductCommand = new RelayCommand(o =>
            {
                if (!string.IsNullOrEmpty(NewName))
                {
                    Products.Add(new Product { Name = NewName, Price = NewPrice, Stock = 10 });
                    NewName = "";
                    NewPrice = 0;
                }
            });
        }
    }

    public class CashControlViewModel : BaseViewModel
    {
        private MainViewModel _main;
        public CashSession Session => _main.Session;
        public ObservableCollection<CashSession> History => _main.CashSessions;

        private decimal _openingAmount;
        public decimal OpeningAmount
        {
            get => _openingAmount;
            set { _openingAmount = value; OnPropertyChanged(); }
        }

        public ICommand OpenCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand ViewDetailCommand { get; }

        public CashControlViewModel(MainViewModel main)
        {
            _main = main;

            OpenCommand = new RelayCommand(o =>
            {
                if (!_main.RequestAdminValidation()) return;

                if (OpeningAmount < 0)
                {
                    System.Windows.MessageBox.Show("El monto de apertura no puede ser negativo.", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                if (OpeningAmount == 0)
                {
                    var res = System.Windows.MessageBox.Show("¿Está seguro de abrir la caja con saldo 0?", "Aviso", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
                    if (res == System.Windows.MessageBoxResult.No) return;
                }

                var newSession = new CashSession
                {
                    IsOpen = true,
                    OpenedBy = _main.CurrentUser?.Name ?? "Admin",
                    OpeningTime = DateTime.Now,
                    OpeningBalance = OpeningAmount
                };

                _main.Db.CashSessions.Add(newSession);
                _main.Db.SaveChanges();

                _main.CashSessions.Add(newSession);
                _main.Session = newSession;
                _main.CurrentView = new PosViewModel(_main);
            });

            CloseCommand = new RelayCommand(o =>
            {
                if (Session == null) return;
                if (!_main.RequestAdminValidation()) return;

                var result = System.Windows.MessageBox.Show("¿Está seguro de cerrar la caja?", "Confirmar Cierre", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
                if (result != System.Windows.MessageBoxResult.Yes) return;

                var currentSession = this.Session;
                if (currentSession == null) return;

                // Open the cash count view to perform a physical count before finalizing close.
                _main.CurrentView = new NewCashCountViewModel(_main, savedCount =>
                {
                    try
                    {
                        currentSession.ClosingTime = DateTime.Now;
                        currentSession.ClosingBalance = savedCount.TotalCalculated;
                        currentSession.IsOpen = false;
                        currentSession.ClosedBy = _main.CurrentUser?.Name ?? "Usuario"; // Registrar quién cerró

                        _main.Db.SaveChanges();
                        _main.Session = null;
                        OnPropertyChanged(nameof(Session));
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Error al cerrar sesión: {ex.Message}");
                    }
                });
            });

            ViewDetailCommand = new RelayCommand(o =>
            {
                if (o is CashSession sess)
                {
                    _main.CurrentView = new CashSessionDetailViewModel(_main, sess);
                }
            });

            RefreshTotals();
        }

        private void RefreshTotals()
        {
            if (Session == null || !Session.IsOpen) return;

            // Calculate totals from collections since opening time
            var startTime = Session.OpeningTime;

            Session.SalesTotal = _main.Sales.Where(s => s.Date >= startTime).Sum(s => s.Total);
            Session.TopupsTotal = _main.Topups.Where(t => t.DateTime >= startTime).Sum(t => t.Amount);
            Session.RepairsTotal = _main.Repairs.Where(r => r.EntryDate >= startTime).Sum(r => r.Total);

            var movements = _main.FinancialMovements.Where(m => m.Date >= startTime).ToList();
            Session.IncomesTotal = movements.Where(m => m.Type == "Ingreso").Sum(m => m.Amount);
            Session.ExpensesTotal = movements.Where(m => m.Type == "Gasto").Sum(m => m.Amount);

            OnPropertyChanged(nameof(Session));
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
            _main = main;
            Session = session;
            BackCommand = new RelayCommand(o => _main.CurrentView = new CashControlViewModel(_main));

            // Try to load a cash count performed during or closest to this session
            try
            {
                CashCount = _main.Db.CashCounts
                    .Include(c => c.Details)
                    .Where(c => c.DateTime >= Session.OpeningTime && (Session.ClosingTime == null || c.DateTime <= Session.ClosingTime.Value))
                    .OrderByDescending(c => c.DateTime)
                    .FirstOrDefault();

                // If none found in the session range, take the most recent overall
                if (CashCount == null)
                {
                    CashCount = _main.Db.CashCounts
                        .Include(c => c.Details)
                        .OrderByDescending(c => c.DateTime)
                        .FirstOrDefault();
                }
            }
            catch
            {
                // ignore DB errors for display purposes
                CashCount = null;
            }
        }
    }

    public class OpenCashViewModel : BaseViewModel
    {
        private MainViewModel _main;
        public decimal Amount { get; set; }
        public ICommand OpenCommand { get; }

        public OpenCashViewModel(MainViewModel main)
        {
            _main = main;
            OpenCommand = new RelayCommand(o =>
            {
                _main.Session = new CashSession
                {
                    IsOpen = true,
                    OpeningBalance = Amount,
                    OpeningTime = DateTime.Now
                };
                _main.CurrentView = new PosViewModel(_main);
            });
        }
    }

    public class PosViewModel : BaseViewModel
    {
        private MainViewModel _main;
        private decimal _taxRate;
        public ObservableCollection<Product> Products => _main.Products;
        public ObservableCollection<CartItem> Cart { get; } = new ObservableCollection<CartItem>();

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private ObservableCollection<Product> _filteredProducts;
        public ObservableCollection<Product> FilteredProducts
        {
            get => _filteredProducts;
            set { _filteredProducts = value; OnPropertyChanged(); }
        }

        public decimal Subtotal => Cart.Sum(i => i.Subtotal);

        private decimal _discount;
        public decimal Discount
        {
            get => _discount;
            set
            {
                // Validación para evitar total negativo
                decimal tempDiscountValue = IsPercentageDiscount ? (Subtotal * (value / 100m)) : value;
                if (Subtotal - tempDiscountValue < 0)
                {
                    System.Windows.MessageBox.Show("El descuento no puede ser mayor al subtotal.", "Validación", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                _discount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Iva));
                OnPropertyChanged(nameof(Total));
            }
        }

        private bool _isPercentageDiscount;
        public bool IsPercentageDiscount
        {
            get => _isPercentageDiscount;
            set
            {
                _isPercentageDiscount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Iva));
                OnPropertyChanged(nameof(Total));
            }
        }

        public decimal DiscountValue => IsPercentageDiscount ? (Subtotal * (Discount / 100m)) : Discount;

        public decimal Iva => Math.Round((Subtotal - DiscountValue) * (_taxRate / 100m), 2);
        public decimal Total => Subtotal - DiscountValue + Iva;

        public ObservableCollection<Client> Clients => _main.Clients;

        private Client? _selectedClient;
        public Client? _previousSelectedClient;
        public Client? SelectedClient
        {
            get => _selectedClient;
            set
            {
                _selectedClient = value;
                OnPropertyChanged();
                if (value != null) CustomerNameText = value.Name;
            }
        }

        private string _customerNameText = "Consumidor Final";
        public string CustomerNameText
        {
            get => _customerNameText;
            set { _customerNameText = value; OnPropertyChanged(); }
        }

        private string _paymentMethod = "Efectivo";
        public string PaymentMethod
        {
            get => _paymentMethod;
            set { _paymentMethod = value; OnPropertyChanged(); }
        }

        private decimal _receivedAmount;
        public decimal ReceivedAmount
        {
            get => _receivedAmount;
            set
            {
                _receivedAmount = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ChangeAmount));
            }
        }

        public decimal ChangeAmount => Math.Max(0, ReceivedAmount - Total);

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

                var seller = _main.Users.FirstOrDefault(u => u.Pin?.Trim() == SellerPin.Trim());
                return seller?.Name ?? _main.CurrentUser?.Name ?? "Usuario";
            }
        }

        public CashSession Session => _main.Session;

        public ICommand AddToCartCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand HandleScannedBarcodeCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand SellCommand { get; }
        public ICommand CloseCashCommand { get; }
        public ICommand QuickAddClientCommand { get; }

        public PosViewModel(MainViewModel main)
        {
            _main = main;

            // Cargar tasa de impuesto desde configuración
            var config = _main.Db.BusinessConfigs.FirstOrDefault();
            _taxRate = config?.TaxRate ?? 0m;

            FilteredProducts = new ObservableCollection<Product>(_main.Products);
            AddToCartCommand = new RelayCommand(p =>
            {
                if (p is Product prod) AddProductToCart(prod);
            });

            RemoveItemCommand = new RelayCommand(o =>
            {
                if (o is CartItem item)
                {
                    Cart.Remove(item);
                    OnPropertyChanged(nameof(Subtotal));
                    OnPropertyChanged(nameof(Total));
                }
            });



            IncreaseQuantityCommand = new RelayCommand(o =>
            {
                if (o is CartItem item)
                {
                    if (item.Product.Stock > item.Quantity)
                    {
                        item.Quantity++;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show($"Stock insuficiente. Solo quedan {item.Product.Stock} unidades.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    }
                }
            });

            // Expose a command that can be used by the view to process a scanned barcode string
            HandleScannedBarcodeCommand = new RelayCommand(o =>
            {
                if (o is string code)
                {
                    HandleBarcodeScan(code.Trim());
                }
            });

            DecreaseQuantityCommand = new RelayCommand(o =>
            {
                if (o is CartItem item && item.Quantity > 1) item.Quantity--;
            });

            ClearCartCommand = new RelayCommand(o =>
            {
                Cart.Clear();
                Discount = 0;
                CustomerNameText = "Consumidor Final";
                SelectedClient = null;
                OnPropertyChanged(nameof(Subtotal));
                OnPropertyChanged(nameof(Total));
            });

            QuickAddClientCommand = new RelayCommand(o =>
            {
                var dialog = new Views.QuickClientDialog();
                if (dialog.ShowDialog() == true)
                {
                    var client = dialog.Client;
                    _main.Db.Clients.Add(client);
                    _main.Db.SaveChanges();
                    _main.Clients.Add(client);
                    SelectedClient = client;
                }
            });

            SellCommand = new RelayCommand(o =>
            {
                if (!Cart.Any()) return;

                // Validación de Descuento Grande (> 15%)
                decimal discountThreshold = Subtotal * 0.15m;
                if (DiscountValue > discountThreshold)
                {
                    if (!_main.RequestAdminValidation()) return;
                }

                if (PaymentMethod == "Efectivo" && ReceivedAmount < Total)
                {
                    System.Windows.MessageBox.Show("El monto recibido es menor al total.", "Error de Pago", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                // Volver a la búsqueda por el PIN del cuadro de texto (sin diálogo)
                var pinActual = SellerPin?.Trim();
                var seller = _main.Users.FirstOrDefault(u => u.Pin == pinActual);

                string nombreFinal = seller?.Name ?? _main.CurrentUser?.Name ?? "Usuario";
                string pinFinal = seller?.Pin ?? _main.CurrentUser?.Pin ?? "0000";

                // Save sale record to DB
                var newSale = new Sale
                {
                    Date = DateTime.Now,
                    SellerPin = pinFinal,
                    SellerName = nombreFinal,
                    CustomerName = string.IsNullOrWhiteSpace(CustomerNameText) ? "Consumidor Final" : CustomerNameText,
                    Subtotal = Subtotal,
                    Discount = DiscountValue,
                    Iva = Iva,
                    Total = Total,
                    PaymentMethod = PaymentMethod,
                    ReceivedAmount = ReceivedAmount,
                    ChangeAmount = ChangeAmount,
                    Items = new ObservableCollection<SaleItem>(Cart.Select(i => new SaleItem
                    {
                        ProductId = i.Product.Id,
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.Product.SellingPrice,
                        PurchasePrice = i.Product.PurchasePrice, // Guardar costo histórico
                        Discount = i.Discount
                    }))
                };

                // Update stock in DB and memory
                foreach (var item in Cart)
                {
                    var dbProd = _main.Db.Products.Find(item.Product.Id);
                    if (dbProd != null)
                    {
                        dbProd.Stock -= item.Quantity;

                        // Sincronización 4.5: Actualizar instancia en memoria global
                        var memProd = _main.Products.FirstOrDefault(p => p.Id == item.Product.Id);
                        if (memProd != null) memProd.Stock = dbProd.Stock;

                        // Asegurar que la instancia del carrito también esté al día (por si acaso)
                        if (item.Product != memProd) item.Product.Stock = dbProd.Stock;
                    }
                }

                _main.Db.Sales.Add(newSale);
                _main.Db.SaveChanges();

                // Imprimir Ticket automáticamente después de guardar
                try
                {
                    var printer = new TicketPrinter();
                    printer.PrintSaleTicket(newSale);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error de impresión: " + ex.Message);
                }

                _main.Sales.Add(newSale);
                _main.Session.SalesTotal += Total;
                SellerPin = string.Empty;
                ReceivedAmount = 0;
                Discount = 0;
                IsPercentageDiscount = false;
                SelectedClient = null;
                CustomerNameText = "Consumidor Final";
                ClearCartCommand.Execute(null);
            });

            CloseCashCommand = new RelayCommand(o =>
            {
                _main.Session.IsOpen = false;
                _main.Session.ClosingTime = DateTime.Now;
                _main.Db.SaveChanges(); // Ensure session state is saved if it's tracked
                // After closing cash, go to the dedicated CashControlView which persists sessions and is the
                // canonical place for opening/closing cash sessions and viewing history.
                _main.CurrentView = new CashControlViewModel(_main);
            });
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                FilteredProducts = new ObservableCollection<Product>(_main.Products);
            else
                FilteredProducts = new ObservableCollection<Product>(_main.Products.Where(p =>
                    p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (p.Barcode != null && p.Barcode.Contains(SearchText))));
        }

        // Add product to cart (used by click and barcode)
        public void AddProductToCart(Product prod, int quantity = 1)
        {
            if (prod == null) return;

            // Validación de Estado
            if (prod.Status != "Activo")
            {
                System.Windows.MessageBox.Show("No se puede vender un producto inactivo.", "Estado no permitido", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // Validación de stock
            if (prod.Stock <= 0)
            {
                System.Windows.MessageBox.Show("Producto sin existencias en inventario.", "Sin Stock", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            var existing = Cart.FirstOrDefault(i => i.Product.Id == prod.Id);
            if (existing != null)
            {
                if (existing.Quantity + quantity > prod.Stock)
                {
                    System.Windows.MessageBox.Show("No hay suficiente stock para aumentar la cantidad.", "Límite alcanzado", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }
                existing.Quantity += quantity;
            }
            else
            {
                Cart.Add(new CartItem(prod) { Quantity = quantity });
            }

            OnPropertyChanged(nameof(Subtotal));
            OnPropertyChanged(nameof(Total));
        }

        // Handle a scanned barcode string
        public void HandleBarcodeScan(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return;
            // Try exact barcode match first
            var prod = _main.Products.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Barcode) && p.Barcode.Trim() == code);
            // Fallback: try contains or by name
            if (prod == null)
            {
                prod = _main.Products.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Barcode) && p.Barcode.Trim().Contains(code));
            }
            if (prod == null)
            {
                // Optionally, try by numeric code as barcode stored without leading zeros
                prod = _main.Products.FirstOrDefault(p => p.Barcode != null && p.Barcode.Trim().Equals(code, StringComparison.OrdinalIgnoreCase));
            }

            if (prod != null)
            {
                AddProductToCart(prod, 1);
            }
            else
            {
                // No product found: optionally notify user -- for now do nothing
            }
        }
    }
}
