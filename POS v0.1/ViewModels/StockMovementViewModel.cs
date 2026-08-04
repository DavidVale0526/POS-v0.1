using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class StockHistoryViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public ObservableCollection<StockMovement> Movements { get; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); ApplyFilter(); }
        }

        private ObservableCollection<StockMovement> _filteredMovements;
        public ObservableCollection<StockMovement> FilteredMovements
        {
            get => _filteredMovements;
            set { _filteredMovements = value; OnPropertyChanged(); }
        }

        public ICommand BackCommand { get; }

        public StockHistoryViewModel(MainViewModel main, int? productId = null)
        {
            _main = main;

            var query = _main.StockMovements.AsEnumerable();
            if (productId.HasValue)
            {
                query = query.Where(m => m.ProductId == productId.Value);
            }

            Movements = new ObservableCollection<StockMovement>(query);
            FilteredMovements = new ObservableCollection<StockMovement>(Movements);

            BackCommand = new RelayCommand(o => _main.CurrentView = new ProductsViewModel(_main));
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredMovements = new ObservableCollection<StockMovement>(Movements);
            }
            else
            {
                var filtered = Movements.Where(m =>
                    (m.MovementType != null && m.MovementType.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (m.Reason != null && m.Reason.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (m.UserName != null && m.UserName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (m.Product != null && m.Product.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                );
                FilteredMovements = new ObservableCollection<StockMovement>(filtered);
            }
        }
    }
}
