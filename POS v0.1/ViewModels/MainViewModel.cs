using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;
using POS_v0_1.Data;
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
            set { _currentUser = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();
        public ObservableCollection<Provider> Providers { get; set; } = new ObservableCollection<Provider>();
        public ObservableCollection<Purchase> Purchases { get; set; } = new ObservableCollection<Purchase>();
        public ObservableCollection<Sale> Sales { get; set; } = new ObservableCollection<Sale>();
        public ObservableCollection<Topup> Topups { get; set; } = new ObservableCollection<Topup>();
        public ObservableCollection<RepairOrder> Repairs { get; set; } = new ObservableCollection<RepairOrder>();
        public ObservableCollection<RepairCategory> RepairCategories { get; set; } = new ObservableCollection<RepairCategory>();
        public ObservableCollection<RepairType> RepairTypes { get; set; } = new ObservableCollection<RepairType>();
        public ObservableCollection<FinancialMovement> FinancialMovements { get; set; } = new ObservableCollection<FinancialMovement>();
        public ObservableCollection<FinancialSource> FinancialSources { get; set; } = new ObservableCollection<FinancialSource>();
        public ObservableCollection<CashCount> CashCounts { get; set; } = new ObservableCollection<CashCount>();
        public ObservableCollection<BankTransaction> BankTransactions { get; set; } = new ObservableCollection<BankTransaction>();
        public ObservableCollection<Client> Clients { get; set; } = new ObservableCollection<Client>();
        public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

        public AppDbContext Db { get; } = new AppDbContext();

        private CashSession _session = new CashSession();
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

        public MainViewModel()
        {
            ShowProductManagementCommand = new RelayCommand(o => CurrentView = new ProductsViewModel(this));
            ShowOpenCashCommand = new RelayCommand(o => CurrentView = new OpenCashViewModel(this));
            ShowPosCommand = new RelayCommand(o =>
            {
                if (Session.IsOpen)
                    CurrentView = new PosViewModel(this);
                else
                    CurrentView = new OpenCashViewModel(this);
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
            ShowReportsCommand = new RelayCommand(o =>
            {
                if (CurrentUser?.Role == "Administrador")
                    CurrentView = new ReportsViewModel(this);
                else
                    System.Windows.MessageBox.Show("Acceso denegado. Se requiere rol de Administrador.", "Permisos", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
            });
            ShowUsersRolesCommand = new RelayCommand(o =>
            {
                if (CurrentUser?.Role == "Administrador")
                    CurrentView = new UserManagementViewModel(this);
                else
                    System.Windows.MessageBox.Show("Acceso denegado. Se requiere rol de Administrador.", "Permisos", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Stop);
            });
            ShowConfigurationCommand = new RelayCommand(o => CurrentView = new PlaceholderViewModel("Configuración"));

            // Initialize Database
            Db.Database.EnsureCreated();
            LoadDataFromDatabase();

            LogoutCommand = new RelayCommand(o =>
            {
                IsAuthenticated = false;
                CurrentUser = null;
                CurrentView = new LoginViewModel(this);
            });

            // Initial view
            IsAuthenticated = false;
            CurrentView = new LoginViewModel(this);
        }

        private void LoadDataFromDatabase()
        {
            // Load Users
            var dbUsers = Db.Users.ToList();
            if (!dbUsers.Any())
            {
                // Seed default users if none exist
                var admin = new User { Name = "Administrador", Email = "admin@admin.com", Password = "admin", Role = "Administrador", Pin = "1234" };
                var seller = new User { Name = "Vendedor Juan", Email = "vendedor@admin.com", Password = "123", Role = "Vendedor", Pin = "4321" };
                Db.Users.AddRange(admin, seller);
                Db.SaveChanges();
                dbUsers = Db.Users.ToList();
            }
            foreach (var user in dbUsers) Users.Add(user);

            // Load Categories
            var dbCategories = Db.Categories.ToList();
            if (!dbCategories.Any())
            {
                Db.Categories.Add(new Category { Name = "Celulares", Description = "Dispositivos móviles" });
                Db.Categories.Add(new Category { Name = "Accesorios", Description = "Cargadores, cables, etc." });
                Db.SaveChanges();
                dbCategories = Db.Categories.ToList();
            }
            foreach (var cat in dbCategories) Categories.Add(cat);

            // Load Products
            foreach (var prod in Db.Products.ToList()) Products.Add(prod);

            // Load Providers
            foreach (var prov in Db.Providers.ToList()) Providers.Add(prov);

            // Load Financial Sources
            var dbSources = Db.FinancialSources.ToList();
            if (!dbSources.Any())
            {
                Db.FinancialSources.Add(new FinancialSource { Name = "Caja", Description = "Dinero en caja" });
                Db.FinancialSources.Add(new FinancialSource { Name = "Banco", Description = "Cuenta bancaria" });
                Db.SaveChanges();
                dbSources = Db.FinancialSources.ToList();
            }
            foreach (var source in dbSources) FinancialSources.Add(source);

            // Load Other Data
            foreach (var sale in Db.Sales.Include(s => s.Items).ToList()) Sales.Add(sale);
            foreach (var client in Db.Clients.ToList()) Clients.Add(client);
            foreach (var repair in Db.Repairs.ToList()) Repairs.Add(repair);
            foreach (var topup in Db.Topups.ToList()) Topups.Add(topup);
            foreach (var movement in Db.FinancialMovements.ToList()) FinancialMovements.Add(movement);
            foreach (var count in Db.CashCounts.Include(c => c.Details).ToList()) CashCounts.Add(count);
            foreach (var bank in Db.BankTransactions.ToList()) BankTransactions.Add(bank);
            foreach (var purchase in Db.Purchases.Include(p => p.Items).ToList()) Purchases.Add(purchase);
        }

        public void SaveChanges()
        {
            Db.SaveChanges();
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
                    CurrentBalance = Amount,
                    OpeningTime = DateTime.Now
                };
                _main.CurrentView = new PosViewModel(_main);
            });
        }
    }

    public class PosViewModel : BaseViewModel
    {
        private MainViewModel _main;
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
            set { _discount = value; OnPropertyChanged(); OnPropertyChanged(nameof(Total)); }
        }

        public decimal Iva => 0; // Assuming 0% as per image for now
        public decimal Total => Subtotal - Discount + Iva;

        public CashSession Session => _main.Session;

        public ICommand AddToCartCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand ClearCartCommand { get; }
        public ICommand SellCommand { get; }
        public ICommand CloseCashCommand { get; }

        public PosViewModel(MainViewModel main)
        {
            _main = main;
            FilteredProducts = new ObservableCollection<Product>(_main.Products);

            AddToCartCommand = new RelayCommand(p =>
            {
                if (p is Product prod)
                {
                    var existing = Cart.FirstOrDefault(i => i.Product.Id == prod.Id);
                    if (existing != null)
                    {
                        existing.Quantity++;
                    }
                    else
                    {
                        var newItem = new CartItem { Product = prod, Quantity = 1 };
                        newItem.PropertyChanged += (s, e) => {
                            if (e.PropertyName == nameof(CartItem.Subtotal))
                            {
                                OnPropertyChanged(nameof(Subtotal));
                                OnPropertyChanged(nameof(Total));
                            }
                        };
                        Cart.Add(newItem);
                    }
                    OnPropertyChanged(nameof(Subtotal));
                    OnPropertyChanged(nameof(Total));
                }
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
                if (o is CartItem item) item.Quantity++;
            });

            DecreaseQuantityCommand = new RelayCommand(o =>
            {
                if (o is CartItem item && item.Quantity > 1) item.Quantity--;
            });

            ClearCartCommand = new RelayCommand(o =>
            {
                Cart.Clear();
                Discount = 0;
                OnPropertyChanged(nameof(Subtotal));
                OnPropertyChanged(nameof(Total));
            });

            SellCommand = new RelayCommand(o =>
            {
                if (!Cart.Any()) return;

                // Save sale record to DB
                var newSale = new Sale
                {
                    Date = DateTime.Now,
                    Subtotal = Subtotal,
                    Discount = Discount,
                    Iva = Iva,
                    Total = Total,
                    Items = new ObservableCollection<SaleItem>(Cart.Select(i => new SaleItem
                    {
                        ProductId = i.Product.Id,
                        ProductName = i.Product.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.Product.SellingPrice
                    }))
                };

                // Update stock in DB and memory
                foreach (var item in Cart)
                {
                    var dbProd = _main.Db.Products.Find(item.Product.Id);
                    if (dbProd != null) dbProd.Stock -= item.Quantity;
                }

                _main.Db.Sales.Add(newSale);
                _main.Db.SaveChanges();

                _main.Sales.Add(newSale);
                _main.Session.CurrentBalance += Total;
                ClearCartCommand.Execute(null);
            });

            CloseCashCommand = new RelayCommand(o =>
            {
                _main.Session.IsOpen = false;
                _main.Session.ClosingTime = DateTime.Now;
                _main.Db.SaveChanges(); // Ensure session state is saved if it's tracked
                _main.CurrentView = new OpenCashViewModel(_main);
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
    }
}
