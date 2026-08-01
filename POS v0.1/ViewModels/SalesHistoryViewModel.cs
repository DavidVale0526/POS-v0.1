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
