using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class ProductsViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        public ObservableCollection<Category> Categories => _main.Categories;

        private ObservableCollection<Product> _filteredProducts;
        public ObservableCollection<Product> FilteredProducts
        {
            get => _filteredProducts;
            set { _filteredProducts = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; ApplyFilters(); OnPropertyChanged(); }
        }

        private Category _selectedCategoryFilter;
        public Category SelectedCategoryFilter
        {
            get => _selectedCategoryFilter;
            set { _selectedCategoryFilter = value; ApplyFilters(); OnPropertyChanged(); }
        }

        private string _locationFilter;
        public string LocationFilter
        {
            get => _locationFilter;
            set { _locationFilter = value; ApplyFilters(); OnPropertyChanged(); }
        }

        private string _statusFilter = "Estados";
        public string StatusFilter
        {
            get => _statusFilter;
            set { _statusFilter = value; ApplyFilters(); OnPropertyChanged(); }
        }

        public ICommand ShowCreateProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand EditProductCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand StockAlertCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand ShowProductDetailCommand { get; }

        public ProductsViewModel(MainViewModel main, bool lowStockOnly = false)
        {
            _main = main;

            if (lowStockOnly)
            {
                FilteredProducts = new ObservableCollection<Product>(_main.Products.Where(p => p.Stock <= p.MinStock));
            }
            else
            {
                FilteredProducts = new ObservableCollection<Product>(_main.Products);
            }

            ShowCreateProductCommand = new RelayCommand(o => _main.CurrentView = new CreateProductViewModel(_main));

            DeleteProductCommand = new RelayCommand(o =>
            {
                if (o is Product prod)
                {
                    var cat = _main.Categories.FirstOrDefault(c => c.Name == prod.Category);
                    if (cat != null) cat.ProductCount--;

                    _main.Products.Remove(prod);
                    ApplyFilters();
                }
            });

            EditProductCommand = new RelayCommand(o =>
            {
                if (o is Product prod) _main.CurrentView = new CreateProductViewModel(_main, prod);
            });

            ShowProductDetailCommand = new RelayCommand(o =>
            {
                if (o is Product prod) _main.CurrentView = new ProductDetailViewModel(_main, prod);
            });

            FilterCommand = new RelayCommand(o => ApplyFilters());

            StockAlertCommand = new RelayCommand(o =>
            {
                _main.CurrentView = new LowStockViewModel(_main);
            });

            ClearFiltersCommand = new RelayCommand(o =>
            {
                SearchText = string.Empty;
                SelectedCategoryFilter = null;
                LocationFilter = string.Empty;
                StatusFilter = "Estados";
                ApplyFilters();
            });
        }

        private void ApplyFilters()
        {
            var query = _main.Products.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                query = query.Where(p =>
                    (p.Name != null && p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (p.Barcode != null && p.Barcode.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (p.SKU != null && p.SKU.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                );
            }

            if (SelectedCategoryFilter != null)
            {
                query = query.Where(p => p.Category == SelectedCategoryFilter.Name);
            }

            if (!string.IsNullOrWhiteSpace(LocationFilter))
            {
                query = query.Where(p => p.Location != null && p.Location.Contains(LocationFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (StatusFilter != "Estados" && !string.IsNullOrWhiteSpace(StatusFilter))
            {
                query = query.Where(p => p.Status == StatusFilter);
            }

            FilteredProducts = new ObservableCollection<Product>(query.ToList());
        }
    }

    public class LowStockViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        public ObservableCollection<Product> LowStockProducts { get; }

        public ICommand BackCommand { get; }
        public ICommand RestockCommand { get; }
        public ICommand ViewDetailCommand { get; }

        public LowStockViewModel(MainViewModel main)
        {
            _main = main;
            LowStockProducts = new ObservableCollection<Product>(_main.Products.Where(p => p.Stock <= p.MinStock));

            BackCommand = new RelayCommand(o => _main.CurrentView = new ProductsViewModel(_main));

            RestockCommand = new RelayCommand(o =>
            {
                if (o is Product prod) _main.CurrentView = new RestockViewModel(_main, prod);
            });

            ViewDetailCommand = new RelayCommand(o =>
            {
                if (o is Product prod) _main.CurrentView = new ProductDetailViewModel(_main, prod);
            });
        }
    }

    public class ProductDetailViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        public Product Product { get; }

        public ICommand BackCommand { get; }
        public ICommand EditCommand { get; }

        public ProductDetailViewModel(MainViewModel main, Product product)
        {
            _main = main;
            Product = product;

            BackCommand = new RelayCommand(o => _main.CurrentView = new ProductsViewModel(_main));
            EditCommand = new RelayCommand(o => _main.CurrentView = new CreateProductViewModel(_main, Product));
        }
    }

    public class RestockViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        public Product Product { get; }

        private int _quantityToAdd;
        public int QuantityToAdd
        {
            get => _quantityToAdd;
            set { _quantityToAdd = value; OnPropertyChanged(); }
        }

        public ICommand BackCommand { get; }
        public ICommand SaveCommand { get; }

        public RestockViewModel(MainViewModel main, Product product)
        {
            _main = main;
            Product = product;

            BackCommand = new RelayCommand(o => _main.CurrentView = new LowStockViewModel(_main));

            SaveCommand = new RelayCommand(o =>
            {
                if (QuantityToAdd > 0)
                {
                    Product.Stock += QuantityToAdd;
                    _main.CurrentView = new LowStockViewModel(_main);
                }
            });
        }
    }

    public class CreateProductViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private Product _editingProduct;

        public string Title => _editingProduct == null ? "Nuevo Producto" : "Editar Producto";
        public ObservableCollection<Category> Categories => _main.Categories;
        public ObservableCollection<string> Statuses { get; } = new ObservableCollection<string> { "Activo", "Inactivo" };

        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _barcode;
        public string Barcode { get => _barcode; set { _barcode = value; OnPropertyChanged(); } }

        private string _sku;
        public string SKU { get => _sku; set { _sku = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        private Category _selectedCategory;
        public Category SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); } }

        private string _provider;
        public string Provider { get => _provider; set { _provider = value; OnPropertyChanged(); } }

        private string _location;
        public string Location { get => _location; set { _location = value; OnPropertyChanged(); } }

        private decimal _purchasePrice;
        public decimal PurchasePrice { get => _purchasePrice; set { _purchasePrice = value; OnPropertyChanged(); } }

        private decimal _sellingPrice;
        public decimal SellingPrice { get => _sellingPrice; set { _sellingPrice = value; OnPropertyChanged(); } }

        private int _stock;
        public int Stock { get => _stock; set { _stock = value; OnPropertyChanged(); } }

        private int _minStock = 5;
        public int MinStock { get => _minStock; set { _minStock = value; OnPropertyChanged(); } }

        private string _selectedStatus = "Activo";
        public string SelectedStatus { get => _selectedStatus; set { _selectedStatus = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public CreateProductViewModel(MainViewModel main, Product product = null)
        {
            _main = main;
            _editingProduct = product;

            if (_editingProduct != null)
            {
                Name = _editingProduct.Name;
                Barcode = _editingProduct.Barcode;
                SKU = _editingProduct.SKU;
                Description = _editingProduct.Description;
                PurchasePrice = _editingProduct.PurchasePrice;
                SellingPrice = _editingProduct.SellingPrice;
                Stock = _editingProduct.Stock;
                MinStock = _editingProduct.MinStock;
                Location = _editingProduct.Location;
                Provider = _editingProduct.Provider;
                SelectedStatus = _editingProduct.Status;
                SelectedCategory = _main.Categories.FirstOrDefault(c => c.Name == _editingProduct.Category);
            }

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(Name) || SelectedCategory == null) return;

                if (_editingProduct == null)
                {
                    int nextId = _main.Products.Any() ? _main.Products.Max(p => p.Id) + 1 : 1;
                    _main.Products.Add(new Product
                    {
                        Id = nextId,
                        Name = Name,
                        Barcode = Barcode,
                        SKU = SKU,
                        Description = Description,
                        PurchasePrice = PurchasePrice,
                        SellingPrice = SellingPrice,
                        Stock = Stock,
                        MinStock = MinStock,
                        Location = Location,
                        Provider = Provider,
                        Status = SelectedStatus,
                        Category = SelectedCategory.Name
                    });
                    SelectedCategory.ProductCount++;
                }
                else
                {
                    if (_editingProduct.Category != SelectedCategory.Name)
                    {
                        var oldCat = _main.Categories.FirstOrDefault(c => c.Name == _editingProduct.Category);
                        if (oldCat != null) oldCat.ProductCount--;
                        SelectedCategory.ProductCount++;
                    }

                    _editingProduct.Name = Name;
                    _editingProduct.Barcode = Barcode;
                    _editingProduct.SKU = SKU;
                    _editingProduct.Description = Description;
                    _editingProduct.PurchasePrice = PurchasePrice;
                    _editingProduct.SellingPrice = SellingPrice;
                    _editingProduct.Stock = Stock;
                    _editingProduct.MinStock = MinStock;
                    _editingProduct.Location = Location;
                    _editingProduct.Provider = Provider;
                    _editingProduct.Status = SelectedStatus;
                    _editingProduct.Category = SelectedCategory.Name;
                }
                _main.CurrentView = new ProductsViewModel(_main);
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new ProductsViewModel(_main));
        }
    }
}
