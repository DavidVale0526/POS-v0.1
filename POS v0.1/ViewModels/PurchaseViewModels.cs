using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class PurchasesViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        public ObservableCollection<Purchase> Purchases => _main.Purchases;

        public ICommand ShowCreatePurchaseCommand { get; }

        public PurchasesViewModel(MainViewModel main)
        {
            _main = main;
            ShowCreatePurchaseCommand = new RelayCommand(o => _main.CurrentView = new CreatePurchaseViewModel(_main));
        }
    }

    public class CreatePurchaseViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public ObservableCollection<Provider> Providers => _main.Providers;
        public ObservableCollection<Product> Products => _main.Products;

        private Provider _selectedProvider;
        public Provider SelectedProvider
        {
            get => _selectedProvider;
            set { _selectedProvider = value; OnPropertyChanged(); }
        }

        private string _invoiceNumber;
        public string InvoiceNumber
        {
            get => _invoiceNumber;
            set { _invoiceNumber = value; OnPropertyChanged(); }
        }

        private DateTime _purchaseDate = DateTime.Now;
        public DateTime PurchaseDate
        {
            get => _purchaseDate;
            set { _purchaseDate = value; OnPropertyChanged(); }
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        private Product _selectedProductToSearch;
        public Product SelectedProductToSearch
        {
            get => _selectedProductToSearch;
            set { _selectedProductToSearch = value; OnPropertyChanged(); }
        }

        public ObservableCollection<PurchaseDetail> Items { get; } = new ObservableCollection<PurchaseDetail>();

        public decimal Total => Items.Sum(i => i.Subtotal);

        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand SavePurchaseCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ShowQuickCreateProductCommand { get; }

        public CreatePurchaseViewModel(MainViewModel main)
        {
            _main = main;

            AddItemCommand = new RelayCommand(o =>
            {
                if (SelectedProductToSearch == null) return;

                var existing = Items.FirstOrDefault(i => i.ProductId == SelectedProductToSearch.Id);
                if (existing != null)
                {
                    existing.Quantity++;
                    OnPropertyChanged(nameof(Total));
                    // Trigger refresh for DataGrid if needed, though simple props don't always notify in sub-items without INPC on detail
                }
                else
                {
                    Items.Add(new PurchaseDetail
                    {
                        ProductId = SelectedProductToSearch.Id,
                        ProductName = SelectedProductToSearch.Name,
                        Quantity = 1,
                        UnitPrice = SelectedProductToSearch.PurchasePrice
                    });
                }
                OnPropertyChanged(nameof(Total));
                SelectedProductToSearch = null;
            });

            RemoveItemCommand = new RelayCommand(o =>
            {
                if (o is PurchaseDetail item)
                {
                    Items.Remove(item);
                    OnPropertyChanged(nameof(Total));
                }
            });

            ShowQuickCreateProductCommand = new RelayCommand(o =>
            {
                // This would ideally be a modal, but since we use dynamic navigation:
                _main.CurrentView = new QuickCreateProductViewModel(_main, this);
            });

            SavePurchaseCommand = new RelayCommand(o =>
            {
                if (SelectedProvider == null || !Items.Any()) return;

                var purchase = new Purchase
                {
                    Id = _main.Purchases.Any() ? _main.Purchases.Max(p => p.Id) + 1 : 1,
                    Date = PurchaseDate,
                    ProviderName = SelectedProvider.Name,
                    InvoiceNumber = InvoiceNumber,
                    Notes = Notes,
                    Total = Total
                };

                foreach (var item in Items)
                {
                    purchase.Items.Add(item);
                    var product = _main.Products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                    }
                }

                _main.Purchases.Add(purchase);
                _main.CurrentView = new PurchasesViewModel(_main);
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new PurchasesViewModel(_main));
        }

        // Helper to notify total change from UI if quantity/price edits are allowed in grid
        public void RefreshTotal() => OnPropertyChanged(nameof(Total));
    }

    public class QuickCreateProductViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private readonly CreatePurchaseViewModel _purchaseVM;

        public ObservableCollection<Category> Categories => _main.Categories;

        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private Category _selectedCategory;
        public Category SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); } }

        private decimal _purchasePrice;
        public decimal PurchasePrice { get => _purchasePrice; set { _purchasePrice = value; OnPropertyChanged(); } }

        private decimal _sellingPrice;
        public decimal SellingPrice { get => _sellingPrice; set { _sellingPrice = value; OnPropertyChanged(); } }

        private int _stock;
        public int Stock { get => _stock; set { _stock = value; OnPropertyChanged(); } }

        private int _minStock = 5;
        public int MinStock { get => _minStock; set { _minStock = value; OnPropertyChanged(); } }

        private string _location;
        public string Location { get => _location; set { _location = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand CloseCommand { get; }

        public QuickCreateProductViewModel(MainViewModel main, CreatePurchaseViewModel purchaseVM)
        {
            _main = main;
            _purchaseVM = purchaseVM;

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(Name) || SelectedCategory == null) return;

                var newProduct = new Product
                {
                    Id = _main.Products.Any() ? _main.Products.Max(p => p.Id) + 1 : 1,
                    Name = Name,
                    Category = SelectedCategory.Name,
                    PurchasePrice = PurchasePrice,
                    SellingPrice = SellingPrice,
                    Stock = Stock,
                    MinStock = MinStock,
                    Location = Location,
                    Status = "Activo"
                };

                _main.Products.Add(newProduct);
                SelectedCategory.ProductCount++;

                // Automatically add to current purchase
                _purchaseVM.Items.Add(new PurchaseDetail
                {
                    ProductId = newProduct.Id,
                    ProductName = newProduct.Name,
                    Quantity = Stock > 0 ? Stock : 1,
                    UnitPrice = PurchasePrice
                });
                _purchaseVM.RefreshTotal();

                // Go back to purchase view
                _main.CurrentView = _purchaseVM;
            });

            CloseCommand = new RelayCommand(o => _main.CurrentView = _purchaseVM);
        }
    }
}
