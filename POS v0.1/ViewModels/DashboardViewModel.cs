using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private MainViewModel _main;

        private decimal _todaySales;
        public decimal TodaySales
        {
            get => _todaySales;
            private set { _todaySales = value; OnPropertyChanged(); }
        }

        private decimal _todayTopups;
        public decimal TodayTopups
        {
            get => _todayTopups;
            private set { _todayTopups = value; OnPropertyChanged(); }
        }

        private int _pendingRepairs;
        public int PendingRepairs
        {
            get => _pendingRepairs;
            private set { _pendingRepairs = value; OnPropertyChanged(); }
        }

        private int _lowStockProducts;
        public int LowStockProducts
        {
            get => _lowStockProducts;
            private set { _lowStockProducts = value; OnPropertyChanged(); }
        }
        public string FormattedDate => DateTime.Now.ToString("dd MMM, yyyy");

        public ObservableCollection<Sale> RecentSales { get; } = new ObservableCollection<Sale>();

        public CashSession? ActiveSession => _main.Session;

        public ICommand NewQuickSaleCommand { get; }
        public ICommand RegisterProductCommand { get; }
        public ICommand NewTopupCommand { get; }
        public ICommand NewRepairCommand { get; }
        public ICommand CashControlCommand { get; }
        public ICommand ViewAllSalesCommand { get; }

        public DashboardViewModel(MainViewModel main)
        {
            _main = main;
            LoadStats();

            // Suscribirse a cambios en las colecciones para actualizar estadísticas en tiempo real
            _main.Sales.CollectionChanged += (s, e) => LoadStats();
            _main.Topups.CollectionChanged += (s, e) => LoadStats();
            _main.Repairs.CollectionChanged += (s, e) => LoadStats();
            _main.Products.CollectionChanged += (s, e) => LoadStats();

            NewQuickSaleCommand = _main.ShowPosCommand;
            RegisterProductCommand = _main.ShowProductManagementCommand;
            NewTopupCommand = _main.ShowTopupsCommand;
            NewRepairCommand = _main.ShowRepairsCommand;
            CashControlCommand = _main.ShowOpenCashCommand;
            ViewAllSalesCommand = _main.ShowSalesHistoryCommand;
        }

        public void LoadStats()
        {
            var today = DateTime.Today;

            // Ventas de hoy (No anuladas)
            TodaySales = _main.Sales
                .Where(s => s.Date.Date == today && !s.IsCancelled)
                .Sum(s => s.Total);

            // Recargas de hoy
            TodayTopups = _main.Topups
                .Where(t => t.DateTime.Date == today)
                .Sum(t => t.Amount);

            // Reparaciones Pendientes
            PendingRepairs = _main.Repairs
                .Count(r => r.Status != "Entregado" && r.Status != "Cancelado");

            // Stock Bajo
            LowStockProducts = _main.Products
                .Count(p => p.Stock <= p.MinStock);

            // Últimas Ventas
            RecentSales.Clear();
            var sales = _main.Sales
                .OrderByDescending(s => s.Date)
                .Take(5);
            foreach (var s in sales) RecentSales.Add(s);
        }
    }
}
