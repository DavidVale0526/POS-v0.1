using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using POS_v0_1.Models;
using System.Collections.Specialized;
using System.ComponentModel;

namespace POS_v0_1.ViewModels
{
    public class PurchasesViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private ObservableCollection<Purchase> _purchases;
        public ObservableCollection<Purchase> Purchases
        {
            get => _purchases;
            set { _purchases = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilter();
            }
        }

        public ICommand ShowCreatePurchaseCommand { get; }
        public ICommand ShowPurchaseDetailCommand { get; }
        public ICommand ShowEditPurchaseCommand { get; }

        public PurchasesViewModel(MainViewModel main)
        {
            _main = main;
            Purchases = new ObservableCollection<Purchase>(_main.Purchases);

            ShowCreatePurchaseCommand = new RelayCommand(o => _main.CurrentView = new CreatePurchaseViewModel(_main));
            ShowEditPurchaseCommand = new RelayCommand(o =>
            {
                if (o is Purchase p)
                {
                    _main.CurrentView = new CreatePurchaseViewModel(_main, p);
                }
            });
            ShowPurchaseDetailCommand = new RelayCommand(o =>
            {
                if (o is Purchase p)
                {
                    _main.CurrentView = new PurchaseDetailViewModel(_main, p);
                }
            });
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Purchases = new ObservableCollection<Purchase>(_main.Purchases);
            }
            else
            {
                var filtered = _main.Purchases.Where(p =>
                    (p.Provider?.Name != null && p.Provider.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (p.InvoiceNumber != null && p.InvoiceNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
                Purchases = new ObservableCollection<Purchase>(filtered);
            }
        }
    }

    public class PurchaseDetailViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        public Purchase Purchase { get; }
        public Provider Provider => Purchase?.Provider ?? new Provider { Name = "N/A" };

        public ICommand BackCommand { get; }

        public PurchaseDetailViewModel(MainViewModel main, Purchase purchase)
        {
            _main = main;
            Purchase = purchase;
            BackCommand = new RelayCommand(o => _main.CurrentView = new PurchasesViewModel(_main));
        }
    }

    public class CreatePurchaseViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private Purchase? _editingPurchase;

        public ObservableCollection<Provider> Providers => _main.Providers;
        public ObservableCollection<Product> Products => _main.Products;
        public ObservableCollection<FinancialSource> FinancialSources => _main.FinancialSources;

        public ObservableCollection<string> Statuses { get; } = new ObservableCollection<string> { "Pendiente", "Recibida", "Parcial" };

        private string _selectedStatus = "Recibida";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { _selectedStatus = value; OnPropertyChanged(); }
        }

        private FinancialSource? _selectedFinancialSource;
        public FinancialSource? SelectedFinancialSource
        {
            get => _selectedFinancialSource;
            set { _selectedFinancialSource = value; OnPropertyChanged(); }
        }

        private Provider _selectedProvider;
        public Provider SelectedProvider
        {
            get => _selectedProvider;
            set
            {
                _selectedProvider = value;
                OnPropertyChanged();
                if (value != null) ProviderNameText = value.Name;
            }
        }

        private string _providerNameText;
        public string ProviderNameText
        {
            get => _providerNameText;
            set { _providerNameText = value; OnPropertyChanged(); }
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

        private void SubscribeToItem(PurchaseDetail item)
        {
            if (item == null) return;
            item.PropertyChanged += Item_PropertyChanged;
        }

        private void UnsubscribeFromItem(PurchaseDetail item)
        {
            if (item == null) return;
            item.PropertyChanged -= Item_PropertyChanged;
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PurchaseDetail.Subtotal) || e.PropertyName == nameof(PurchaseDetail.Quantity) || e.PropertyName == nameof(PurchaseDetail.UnitPrice))
            {
                OnPropertyChanged(nameof(Total));
            }
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var ni in e.NewItems)
                {
                    if (ni is PurchaseDetail pd) SubscribeToItem(pd);
                }
            }
            if (e.OldItems != null)
            {
                foreach (var oi in e.OldItems)
                {
                    if (oi is PurchaseDetail pd) UnsubscribeFromItem(pd);
                }
            }
            OnPropertyChanged(nameof(Total));
        }

        public decimal Total => Items.Sum(i => i.Subtotal);

        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand SavePurchaseCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand ShowQuickCreateProductCommand { get; }

        public CreatePurchaseViewModel(MainViewModel main, Purchase? purchaseToEdit = null)
        {
            _main = main;
            _editingPurchase = purchaseToEdit;

            SelectedFinancialSource = _main.FinancialSources.FirstOrDefault();

            if (_editingPurchase != null)
            {
                SelectedProvider = _main.Providers.FirstOrDefault(p => p.Id == _editingPurchase.ProviderId);
                ProviderNameText = SelectedProvider?.Name ?? "";
                InvoiceNumber = _editingPurchase.InvoiceNumber;
                PurchaseDate = _editingPurchase.Date;
                Notes = _editingPurchase.Notes;
                SelectedStatus = _editingPurchase.Status;
                foreach (var item in _editingPurchase.Items)
                {
                    Items.Add(new PurchaseDetail
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice
                    });
                }
            }

            Items.CollectionChanged += Items_CollectionChanged;

            AddItemCommand = new RelayCommand(o =>
            {
                if (SelectedProductToSearch == null) return;

                var existing = Items.FirstOrDefault(i => i.ProductId == SelectedProductToSearch.Id);
                if (existing != null)
                {
                    existing.Quantity = existing.Quantity + 1;
                }
                else
                {
                    var newItem = new PurchaseDetail
                    {
                        ProductId = SelectedProductToSearch.Id,
                        ProductName = SelectedProductToSearch.Name,
                        Quantity = 1,
                        UnitPrice = SelectedProductToSearch.PurchasePrice
                    };
                    Items.Add(newItem);
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
                _main.CurrentView = new QuickCreateProductViewModel(_main, this);
            });

            SavePurchaseCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(ProviderNameText) || !Items.Any()) return;

                // Intentar buscar proveedor o crear uno nuevo
                if (SelectedProvider == null || SelectedProvider.Name != ProviderNameText)
                {
                    var existing = _main.Providers.FirstOrDefault(p => p.Name.Equals(ProviderNameText, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        SelectedProvider = existing;
                    }
                    else
                    {
                        var newProv = new Provider { Name = ProviderNameText, Description = "Proveedor registrado durante compra" };
                        _main.Db.Providers.Add(newProv);
                        _main.Db.SaveChanges();
                        _main.Providers.Add(newProv);
                        SelectedProvider = newProv;
                    }
                }

                if (SelectedProvider == null) return;

                if (_editingPurchase != null)
                {
                    // Deshacer stock previo si estamos editando
                    foreach (var oldItem in _editingPurchase.Items)
                    {
                        var prod = _main.Db.Products.Find(oldItem.ProductId);
                        if (prod != null) prod.Stock -= oldItem.Quantity;
                    }

                    _editingPurchase.Date = PurchaseDate;
                    _editingPurchase.ProviderId = SelectedProvider.Id;
                    _editingPurchase.Provider = SelectedProvider;
                    _editingPurchase.InvoiceNumber = InvoiceNumber;
                    _editingPurchase.Notes = Notes;
                    _editingPurchase.Total = Total;
                    _editingPurchase.Status = SelectedStatus;

                    _main.Db.RemoveRange(_editingPurchase.Items);
                    _editingPurchase.Items = new ObservableCollection<PurchaseDetail>(Items);
                }
                else
                {
                    var purchase = new Purchase
                    {
                        Date = PurchaseDate,
                        ProviderId = SelectedProvider.Id,
                        Provider = SelectedProvider,
                        InvoiceNumber = InvoiceNumber,
                        Notes = Notes,
                        Total = Total,
                        Status = SelectedStatus,
                        Items = new ObservableCollection<PurchaseDetail>(Items),
                        RegisteredBy = _main.CurrentUser?.Name ?? "Usuario"
                    };
                    _main.Db.Purchases.Add(purchase);
                    _main.Purchases.Add(purchase);
                }

                // Actualizar stock de los productos comprados
                foreach (var item in Items)
                {
                    var product = _main.Db.Products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        product.Stock += item.Quantity;
                        product.PurchasePrice = item.UnitPrice;

                        var memProd = _main.Products.FirstOrDefault(p => p.Id == item.ProductId);
                        if (memProd != null)
                        {
                            memProd.Stock = product.Stock;
                            memProd.PurchasePrice = product.PurchasePrice;
                        }

                        _main.RecordStockMovement(item.ProductId, item.Quantity, "Compra", $"Entrada por compra de {SelectedProvider.Name}");
                    }
                }

                // Registrar movimiento financiero si se seleccionó una fuente
                if (SelectedFinancialSource != null)
                {
                    var movement = new FinancialMovement
                    {
                        Date = DateTime.Now,
                        Type = "Gasto",
                        Concept = $"Compra a proveedor: {SelectedProvider.Name} (Fact: {InvoiceNumber})",
                        SourceId = SelectedFinancialSource.Id,
                        FinancialSource = SelectedFinancialSource,
                        Source = SelectedFinancialSource.Name, // Populate legacy Source column

                        Amount = Total,
                        RegisteredBy = _main.CurrentUser?.Name ?? "Usuario",
                        Reference = InvoiceNumber
                    };
                    _main.Db.FinancialMovements.Add(movement);
                    _main.FinancialMovements.Add(movement);
                }

                _main.Db.SaveChanges();
                _main.ShowToast("Compra registrada correctamente.");
                _main.CurrentView = new PurchasesViewModel(_main);
            });

            BackCommand = new RelayCommand(o =>
            {
                if (Items.Any() || !string.IsNullOrWhiteSpace(InvoiceNumber))
                {
                    var res = System.Windows.MessageBox.Show("Hay una compra en progreso. ¿Desea cancelar y salir?", "Confirmar", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
                    if (res == System.Windows.MessageBoxResult.No) return;
                }
                _main.CurrentView = new PurchasesViewModel(_main);
            });
        }

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
                    Name = Name,
                    CategoryId = SelectedCategory.Id,
                    Category = SelectedCategory,
                    PurchasePrice = PurchasePrice,
                    SellingPrice = SellingPrice,
                    Stock = Stock,
                    MinStock = MinStock,
                    Location = Location,
                    Status = "Activo"
                };

                _main.Db.Products.Add(newProduct);
                _main.Db.SaveChanges();
                _main.Products.Add(newProduct);

                _purchaseVM.Items.Add(new PurchaseDetail
                {
                    ProductId = newProduct.Id,
                    ProductName = newProduct.Name,
                    Quantity = Stock > 0 ? Stock : 1,
                    UnitPrice = PurchasePrice
                });
                _purchaseVM.RefreshTotal();

                _main.CurrentView = _purchaseVM;
            });

            CloseCommand = new RelayCommand(o => _main.CurrentView = _purchaseVM);
        }
    }
}
