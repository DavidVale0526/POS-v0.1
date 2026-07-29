using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;

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

        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();
        public ObservableCollection<Provider> Providers { get; set; } = new ObservableCollection<Provider>();
        public ObservableCollection<Purchase> Purchases { get; set; } = new ObservableCollection<Purchase>();
        public ObservableCollection<Sale> Sales { get; set; } = new ObservableCollection<Sale>();
        public ObservableCollection<Topup> Topups { get; set; } = new ObservableCollection<Topup>();
        public ObservableCollection<RepairOrder> Repairs { get; set; } = new ObservableCollection<RepairOrder>();
        public ObservableCollection<RepairCategory> RepairCategories { get; set; } = new ObservableCollection<RepairCategory>();
        public ObservableCollection<RepairType> RepairTypes { get; set; } = new ObservableCollection<RepairType>();

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
            ShowClientsCommand = new RelayCommand(o => CurrentView = new PlaceholderViewModel("Clientes"));
            ShowRepairCategoriesCommand = new RelayCommand(o => CurrentView = new RepairCategoriesViewModel(this));
            ShowRepairTypesCommand = new RelayCommand(o => CurrentView = new RepairTypesViewModel(this));
            ShowCashCountCommand = new RelayCommand(o => CurrentView = new PlaceholderViewModel("Conteo de Billetes"));
            ShowExpensesIncomesCommand = new RelayCommand(o => CurrentView = new PlaceholderViewModel("Gastos / Ingresos"));
            ShowBankCommand = new RelayCommand(o => CurrentView = new PlaceholderViewModel("Banco"));
            ShowReportsCommand = new RelayCommand(o => CurrentView = new PlaceholderViewModel("Reportes"));
            ShowUsersRolesCommand = new RelayCommand(o => CurrentView = new PlaceholderViewModel("Usuarios / Roles"));
            ShowConfigurationCommand = new RelayCommand(o => CurrentView = new PlaceholderViewModel("Configuración"));

            // Initial view
            CurrentView = new OpenCashViewModel(this);

            // Sample data
            Categories.Add(new Category { Id = 1, Name = "Celulares", Description = "Dispositivos móviles", ProductCount = 0 });
            Categories.Add(new Category { Id = 2, Name = "Accesorios", Description = "Cargadores, cables, fundas, etc.", ProductCount = 0 });
            Categories.Add(new Category { Id = 3, Name = "Repuestos", Description = "Pantallas, baterías, etc.", ProductCount = 0 });

            Products.Add(new Product { Id = 1, Name = "Samsung Galaxy S21", Barcode = "123456789", SKU = "SAM-S21", SellingPrice = 800, PurchasePrice = 600, Stock = 10, MinStock = 2, Category = "Celulares", Status = "Activo" });
            Products.Add(new Product { Id = 2, Name = "Cargador Carga Rápida", Barcode = "987654321", SKU = "ACC-CHRG", SellingPrice = 25, PurchasePrice = 10, Stock = 50, MinStock = 10, Category = "Accesorios", Status = "Activo" });

            Providers.Add(new Provider { Id = 1, Name = "TecnoDistribuidora", ContactName = "Juan Pérez", Phone = "555-0199", City = "CDMX" });
            Providers.Add(new Provider { Id = 2, Name = "Global Phones", ContactName = "Maria García", Phone = "555-0288", City = "Guadalajara" });

            foreach (var prod in Products)
            {
                var cat = Categories.FirstOrDefault(c => c.Name == prod.Category);
                if (cat != null) cat.ProductCount++;
            }
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

                // Save sale record
                var newSale = new Sale
                {
                    Id = _main.Sales.Count + 1,
                    Date = DateTime.Now,
                    Subtotal = Subtotal,
                    Discount = Discount,
                    Iva = Iva,
                    Total = Total
                };

                foreach (var item in Cart)
                {
                    newSale.Items.Add(new SaleItem
                    {
                        ProductId = item.Product.Id,
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.SellingPrice
                    });

                    var prod = _main.Products.FirstOrDefault(p => p.Id == item.Product.Id);
                    if (prod != null) prod.Stock -= item.Quantity;
                }

                _main.Sales.Add(newSale);
                _main.Session.CurrentBalance += Total;
                ClearCartCommand.Execute(null);
            });

            CloseCashCommand = new RelayCommand(o =>
            {
                _main.Session.IsOpen = false;
                _main.Session.ClosingTime = DateTime.Now;
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
