using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
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
        public ICommand ManualAdjustmentCommand { get; }

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

            ManualAdjustmentCommand = new RelayCommand(o =>
            {
                if (o is Product prod)
                {
                    if (!_main.RequestAdminValidation()) return;

                    var dialog = new Views.StockAdjustmentDialog(prod);
                    dialog.Owner = Application.Current.MainWindow;
                    if (dialog.ShowDialog() == true)
                    {
                        prod.Stock += dialog.Adjustment;
                        var dbProd = _main.Db.Products.Find(prod.Id);
                        if (dbProd != null)
                        {
                            dbProd.Stock = prod.Stock;
                            _main.Db.SaveChanges();
                        }
                        _main.RecordStockMovement(prod.Id, dialog.Adjustment, "Ajuste Manual", dialog.Reason);
                        _main.ShowToast("Stock ajustado correctamente.");
                    }
                }
            });

            ShowCreateProductCommand = new RelayCommand(o => _main.CurrentView = new CreateProductViewModel(_main));

            DeleteProductCommand = new RelayCommand(o =>
            {
                if (o is Product prod)
                {
                    var result = System.Windows.MessageBox.Show($"¿Desea eliminar {prod.Name}?", "Confirmar", System.Windows.MessageBoxButton.YesNo);
                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        _main.Products.Remove(prod);
                        var dbProd = _main.Db.Products.Find(prod.Id);
                        if (dbProd != null) _main.Db.Products.Remove(dbProd);
                        _main.Db.SaveChanges();
                        ApplyFilters();
                    }
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
                query = query.Where(p => p.CategoryId == SelectedCategoryFilter.Id);
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

                    // Sincronización 4.5: Guardar en DB inmediatamente
                    var dbProd = _main.Db.Products.Find(Product.Id);
                    if (dbProd != null)
                    {
                        dbProd.Stock = Product.Stock;
                        _main.Db.SaveChanges();
                    }

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
        public ObservableCollection<Provider> Providers => _main.Providers;
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

        private Provider _selectedProvider;
        public Provider SelectedProvider { get => _selectedProvider; set { _selectedProvider = value; OnPropertyChanged(); } }

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

        private string _imagePath;
        public string ImagePath { get => _imagePath; set { _imagePath = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ChooseImageCommand { get; }

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
                SelectedStatus = _editingProduct.Status;
                ImagePath = _editingProduct.ImagePath;

                SelectedCategory = _main.Categories.FirstOrDefault(c => c.Id == _editingProduct.CategoryId);
                SelectedProvider = _main.Providers.FirstOrDefault(p => p.Id == _editingProduct.ProviderId);
            }

            ChooseImageCommand = new RelayCommand(o =>
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"
                };
                if (dialog.ShowDialog() == true)
                {
                    ImagePath = dialog.FileName;
                }
            });

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(Name) || SelectedCategory == null) return;

                // Validación de Precios
                if (SellingPrice <= 0)
                {
                    MessageBox.Show("El precio de venta debe ser mayor a 0.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (SellingPrice < PurchasePrice)
                {
                    var res = MessageBox.Show("El precio de venta es menor al costo de compra. ¿Desea continuar con pérdida?", "Aviso de Rentabilidad", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (res == MessageBoxResult.No) return;
                }

                // Normalización para evitar errores de duplicidad en campos únicos
                string finalBarcode = string.IsNullOrWhiteSpace(Barcode) ? null : Barcode.Trim();
                string finalSKU = string.IsNullOrWhiteSpace(SKU) ? null : SKU.Trim();

                // Validación de Unicidad (Nombre, Barcode y SKU)
                bool nameExists = _main.Products.Any(p => p.Name.Equals(Name, StringComparison.OrdinalIgnoreCase) && p.Id != (_editingProduct?.Id ?? -1));
                if (nameExists)
                {
                    MessageBox.Show("Ya existe un producto con este nombre.", "Duplicado", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (finalBarcode != null)
                {
                    bool barcodeExists = _main.Products.Any(p => p.Barcode == finalBarcode && p.Id != (_editingProduct?.Id ?? -1));
                    if (barcodeExists)
                    {
                        MessageBox.Show("El código de barras ya está registrado en otro producto.", "Duplicado", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (finalSKU != null)
                {
                    bool skuExists = _main.Products.Any(p => p.SKU == finalSKU && p.Id != (_editingProduct?.Id ?? -1));
                    if (skuExists)
                    {
                        MessageBox.Show("El SKU ya está registrado en otro producto.", "Duplicado", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                try
                {
                    if (_editingProduct == null)
                    {
                        var newProd = new Product
                        {
                            Name = Name,
                            Barcode = finalBarcode,
                            SKU = finalSKU,
                            Description = Description,
                            PurchasePrice = PurchasePrice,
                            SellingPrice = SellingPrice,
                            Stock = Stock,
                            MinStock = MinStock,
                            Location = Location,
                            Status = SelectedStatus,
                            ImagePath = string.IsNullOrWhiteSpace(ImagePath) ? null : ImagePath,
                            CategoryId = SelectedCategory.Id,
                            ProviderId = SelectedProvider?.Id,
                            CategoryLegacy = SelectedCategory.Name // Satisfacer NOT NULL legacy
                        };

                        // Asegurar que CategoryLegacy nunca sea null para evitar Error 19
                        if (string.IsNullOrEmpty(newProd.CategoryLegacy)) newProd.CategoryLegacy = "General";

                        // Limpiar referencias para evitar re-inserción
                        newProd.Category = null;
                        newProd.Provider = null;

                        _main.Db.Products.Add(newProd);
                        _main.Db.SaveChanges();
                        _main.Products.Add(newProd);
                    }
                    else
                    {
                        _editingProduct.Name = Name;
                        _editingProduct.Barcode = finalBarcode;
                        _editingProduct.SKU = finalSKU;
                        _editingProduct.Description = Description;
                        _editingProduct.PurchasePrice = PurchasePrice;
                        _editingProduct.SellingPrice = SellingPrice;
                        _editingProduct.Stock = Stock;
                        _editingProduct.MinStock = MinStock;
                        _editingProduct.Location = Location;
                        _editingProduct.Status = SelectedStatus;
                        _editingProduct.ImagePath = string.IsNullOrWhiteSpace(ImagePath) ? null : ImagePath;
                        _editingProduct.CategoryId = SelectedCategory.Id;
                        _editingProduct.ProviderId = SelectedProvider?.Id;
                        _editingProduct.CategoryLegacy = SelectedCategory.Name;

                        _main.Db.SaveChanges();
                    }
                    _main.ShowToast("Producto guardado con éxito.");
                    _main.CurrentView = new ProductsViewModel(_main);
                }
                catch (Exception ex)
                {
                    string msg = "Error al guardar: " + ex.Message;
                    if (ex.InnerException != null) msg += "\nDetalle: " + ex.InnerException.Message;
                    MessageBox.Show(msg, "Fallo de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            BackCommand = new RelayCommand(o =>
            {
                if (IsDirty())
                {
                    var res = MessageBox.Show("Hay cambios sin guardar. ¿Está seguro de que desea salir?", "Confirmar salida", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (res == MessageBoxResult.No) return;
                }
                _main.CurrentView = new ProductsViewModel(_main);
            });
        }

        private bool IsDirty()
        {
            if (_editingProduct == null)
            {
                return !string.IsNullOrWhiteSpace(Name) || !string.IsNullOrWhiteSpace(Barcode) || SellingPrice > 0;
            }
            return Name != _editingProduct.Name || Barcode != _editingProduct.Barcode || SellingPrice != _editingProduct.SellingPrice;
        }
    }
}
