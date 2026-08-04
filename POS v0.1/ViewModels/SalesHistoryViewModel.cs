using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;
using POS_v0_1.Services;

namespace POS_v0_1.ViewModels
{
    public class SalesHistoryViewModel : BaseViewModel
    {
        private MainViewModel _main;

        public ObservableCollection<Sale> Sales => _main.Sales;

        private ObservableCollection<Sale> _filteredSales;
        public ObservableCollection<Sale> FilteredSales
        {
            get => _filteredSales;
            set { _filteredSales = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private string _selectedSeller;
        public string SelectedSeller
        {
            get => _selectedSeller;
            set { _selectedSeller = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private string _selectedPaymentMethod;
        public string SelectedPaymentMethod
        {
            get => _selectedPaymentMethod;
            set { _selectedPaymentMethod = value; OnPropertyChanged(); ApplyFilter(); }
        }

        public ObservableCollection<string> Sellers { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> PaymentMethods { get; } = new ObservableCollection<string>
        {
            "Efectivo", "Tarjeta", "Transferencia", "Mixto"
        };

        private bool _showOnlyCancelled;
        public bool ShowOnlyCancelled
        {
            get => _showOnlyCancelled;
            set { _showOnlyCancelled = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private decimal _totalSold;
        public decimal TotalSold
        {
            get => _totalSold;
            set { _totalSold = value; OnPropertyChanged(); }
        }

        private int _transactionCount;
        public int TransactionCount
        {
            get => _transactionCount;
            set { _transactionCount = value; OnPropertyChanged(); }
        }

        private int _pageSize = 50;
        public int PageSize
        {
            get => _pageSize;
            set { _pageSize = value; OnPropertyChanged(); }
        }

        public bool HasMoreItems => FilteredSales?.Count < TransactionCount;

        public ICommand FilterCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand ShowPosCommand { get; }
        public ICommand ReprintCommand { get; }
        public ICommand CancelSaleCommand { get; }
        public ICommand LoadMoreCommand { get; }

        public SalesHistoryViewModel(MainViewModel main)
        {
            _main = main;

            // Cargar lista de vendedores únicos
            LoadSellers();

            ApplyFilter(); // Inicializar con filtros aplicados (y totales calculados)

            // Update summary when sales change
            Sales.CollectionChanged += (s, e) => {
                ApplyFilter();
            };

            _main.Users.CollectionChanged += (s, e) => {
                LoadSellers();
            };

            FilterCommand = new RelayCommand(o => ApplyFilter());
            ClearFiltersCommand = new RelayCommand(o =>
            {
                SearchText = string.Empty;
                SelectedDate = null;
                SelectedSeller = null;
                SelectedPaymentMethod = null;
                ShowOnlyCancelled = false;
                PageSize = 50;
                ApplyFilter();
            });
            LoadMoreCommand = new RelayCommand(o =>
            {
                PageSize += 50;
                ApplyFilter();
            });
            ShowPosCommand = _main.ShowPosCommand;

            ReprintCommand = new RelayCommand(o =>
            {
                if (o is Sale sale)
                {
                    try
                    {
                        var printer = new TicketPrinter();
                        printer.PrintSaleTicket(sale, isReprint: true);
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Error al reimprimir: " + ex.Message);
                    }
                }
            });

            CancelSaleCommand = new RelayCommand(o =>
            {
                if (o is Sale sale)
                {
                    if (sale.IsCancelled)
                    {
                        System.Windows.MessageBox.Show("Esta venta ya ha sido anulada.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        return;
                    }

                    // Requerir validación de Administrador para anular ventas
                    if (!_main.RequestAdminValidation()) return;

                    var result = System.Windows.MessageBox.Show($"¿Está seguro que desea anular la venta #{sale.Id}?\nEl stock de los productos será devuelto.", "Confirmar Anulación", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);

                    if (result == System.Windows.MessageBoxResult.Yes)
                    {
                        try
                        {
                            sale.IsCancelled = true;
                            sale.CancellationReason = "Anulación manual por usuario";

                            // Crear Nota de Crédito
                            var creditNote = new CreditNote
                            {
                                SaleId = sale.Id,
                                Date = DateTime.Now,
                                Amount = sale.Total,
                                Reason = "Anulación de venta",
                                CustomerName = sale.CustomerName
                            };
                            _main.Db.CreditNotes.Add(creditNote);

                            // Devolver stock
                            foreach (var item in sale.Items)
                            {
                                var dbProd = _main.Db.Products.Find(item.ProductId);
                                if (dbProd != null)
                                {
                                    dbProd.Stock += item.Quantity;

                                    // Sincronización 4.5: Actualizar en memoria global para UI instantánea
                                    var memProd = _main.Products.FirstOrDefault(p => p.Id == item.ProductId);
                                    if (memProd != null) memProd.Stock = dbProd.Stock;

                                    // REGISTRO EN KARDEX
                                    _main.RecordStockMovement(item.ProductId, item.Quantity, "Anulación", $"Retorno por anulación de venta #{sale.Id}");
                                }
                            }

                            _main.Db.SaveChanges();
                            ApplyFilter();
                            System.Windows.MessageBox.Show("Venta anulada correctamente.", "Éxito", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            System.Windows.MessageBox.Show("Error al anular venta: " + ex.Message);
                        }
                    }
                }
            });
        }

        private void LoadSellers()
        {
            Sellers.Clear();
            var names = _main.Users.Select(u => u.Name).Distinct().OrderBy(n => n);
            foreach (var name in names) Sellers.Add(name);
        }

        private void ApplyFilter()
        {
            var filtered = Sales.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var search = SearchText.Trim().ToLower();
                filtered = filtered.Where(s =>
                    s.Id.ToString().Contains(search) ||
                    (s.CustomerName != null && s.CustomerName.ToLower().Contains(search)));
            }

            if (SelectedDate.HasValue)
            {
                filtered = filtered.Where(s => s.Date.Date == SelectedDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(SelectedSeller))
            {
                filtered = filtered.Where(s => s.SellerName == SelectedSeller);
            }

            if (!string.IsNullOrEmpty(SelectedPaymentMethod))
            {
                filtered = filtered.Where(s => s.PaymentMethod == SelectedPaymentMethod);
            }

            if (ShowOnlyCancelled)
            {
                filtered = filtered.Where(s => s.IsCancelled);
            }

            // Calculamos totales sobre el filtro COMPLETO (antes de paginar)
            var fullFilteredList = filtered.ToList();
            TotalSold = fullFilteredList.Where(s => !s.IsCancelled).Sum(s => s.Total);
            TransactionCount = fullFilteredList.Count;

            var result = fullFilteredList.OrderByDescending(s => s.Date).Take(PageSize).ToList();
            FilteredSales = new ObservableCollection<Sale>(result);
            OnPropertyChanged(nameof(HasMoreItems));
        }
    }
}
