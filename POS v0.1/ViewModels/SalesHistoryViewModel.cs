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
            set { _searchText = value; OnPropertyChanged(); }
        }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); }
        }

        public decimal TotalSold => Sales.Sum(s => s.Total);
        public int TransactionCount => Sales.Count;

        public ICommand FilterCommand { get; }
        public ICommand ShowPosCommand { get; }
        public ICommand ReprintCommand { get; }
        public ICommand CancelSaleCommand { get; }

        public SalesHistoryViewModel(MainViewModel main)
        {
            _main = main;
            FilteredSales = new ObservableCollection<Sale>(Sales);

            // Update summary when sales change
            Sales.CollectionChanged += (s, e) => {
                OnPropertyChanged(nameof(TotalSold));
                OnPropertyChanged(nameof(TransactionCount));
                ApplyFilter();
            };

            FilterCommand = new RelayCommand(o => ApplyFilter());
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
                                }
                            }

                            _main.Db.SaveChanges();
                            OnPropertyChanged(nameof(TotalSold));
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

        private void ApplyFilter()
        {
            var filtered = Sales.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(s =>
                    s.Id.ToString().Contains(SearchText) ||
                    (s.CustomerName != null && s.CustomerName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
            }

            if (SelectedDate.HasValue)
            {
                filtered = filtered.Where(s => s.Date.Date == SelectedDate.Value.Date);
            }

            FilteredSales = new ObservableCollection<Sale>(filtered.OrderByDescending(s => s.Date));
        }
    }
}
