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

        public decimal TodaySales { get; private set; }
        public decimal TodayTopups { get; private set; }
        public int PendingRepairs { get; private set; }
        public int LowStockProducts { get; private set; }

        public ObservableCollection<Sale> RecentSales { get; } = new ObservableCollection<Sale>();

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
                .Where(t => t.Date.Date == today)
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

            OnPropertyChanged(nameof(TodaySales));
            OnPropertyChanged(nameof(TodayTopups));
            OnPropertyChanged(nameof(PendingRepairs));
            OnPropertyChanged(nameof(LowStockProducts));
            OnPropertyChanged(nameof(RecentSales));
        }
    }
}
