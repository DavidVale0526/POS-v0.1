using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        private DateTime _referenceDate = DateTime.Now;
        public DateTime ReferenceDate
        {
            get => _referenceDate;
            set { _referenceDate = value; OnPropertyChanged(); CalculateSummary(); }
        }

        private string _selectedPeriod = "Diario";
        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set { _selectedPeriod = value; OnPropertyChanged(); CalculateSummary(); }
        }

        private BusinessSummary _summary;
        public BusinessSummary Summary
        {
            get => _summary;
            set { _summary = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Periods { get; } = new ObservableCollection<string> { "Diario", "Semanal", "Mensual", "Anual" };

        public ICommand ConsultCommand { get; }
        public ICommand PrintReportCommand { get; }

        public ReportsViewModel(MainViewModel main)
        {
            _main = main;
            ConsultCommand = new RelayCommand(o => CalculateSummary());
            PrintReportCommand = new RelayCommand(o => { /* Logica de impresion */ });
            CalculateSummary();
        }

        private void CalculateSummary()
        {
            var start = ReferenceDate.Date;
            var end = ReferenceDate.Date.AddDays(1).AddTicks(-1);

            if (SelectedPeriod == "Semanal")
            {
                start = ReferenceDate.Date.AddDays(-(int)ReferenceDate.DayOfWeek);
                end = start.AddDays(7).AddTicks(-1);
            }
            else if (SelectedPeriod == "Mensual")
            {
                start = new DateTime(ReferenceDate.Year, ReferenceDate.Month, 1);
                end = start.AddMonths(1).AddTicks(-1);
            }
            else if (SelectedPeriod == "Anual")
            {
                start = new DateTime(ReferenceDate.Year, 1, 1);
                end = start.AddYears(1).AddTicks(-1);
            }

            var sales = _main.Sales.Where(s => s.Date >= start && s.Date <= end).ToList();
            var topups = _main.Topups.Where(t => t.DateTime >= start && t.DateTime <= end).ToList();
            var repairs = _main.Repairs.Where(r => r.EntryDate >= start && r.EntryDate <= end).ToList();
            var movements = _main.FinancialMovements.Where(m => m.Date >= start && m.Date <= end).ToList();

            var summary = new BusinessSummary();

            // 1. Resumen General
            summary.GrossSales = sales.Sum(s => s.Total);
            summary.PosSalesIncome = summary.GrossSales;
            summary.CostOfGoodsSold = sales.Sum(s => s.Total * 0.7m);
            summary.TopupFlow = topups.Sum(t => t.Amount);
            summary.TopupIncome = topups.Sum(t => t.Amount);
            var deliveredRepairs = repairs.Where(r => r.Status == "Entregado");
            summary.DeliveredRepairsTotal = deliveredRepairs.Sum(r => r.Total);
            summary.RepairIncome = summary.DeliveredRepairsTotal;
            summary.OtherExtraIncome = movements.Where(m => m.Type == "Ingreso").Sum(m => m.Amount);
            summary.ExpensesAndOutflows = movements.Where(m => m.Type == "Gasto").Sum(m => m.Amount);
            summary.RealOperatingProfit = (summary.GrossSales - summary.CostOfGoodsSold) + (summary.TopupFlow * 0.05m) + summary.RepairIncome;
            summary.ActualCashInHand = (summary.TotalGrossIncome - summary.ExpensesAndOutflows);

            // 2. Finanzas por Fuente
            summary.FinancialSummaries = movements
                .GroupBy(m => new { m.Source, m.Type })
                .Select(g => new FinancialSourceSummary
                {
                    Source = g.Key.Source,
                    Type = g.Key.Type,
                    Total = g.Sum(m => m.Amount)
                }).ToList();

            // 3. Valorización de Inventario (Estado Actual)
            summary.Inventory = new InventoryValuation
            {
                InvestmentCost = _main.Products.Where(p => p.Stock > 0).Sum(p => p.PurchasePrice * p.Stock),
                ProjectedSalesValue = _main.Products.Where(p => p.Stock > 0).Sum(p => p.Price * p.Stock)
            };

            // 4. Ventas y Clientes
            summary.TopProducts = sales.SelectMany(s => s.Items)
                .GroupBy(i => i.ProductName)
                .Select(g => new RankedItem
                {
                    Name = g.Key,
                    Quantity = g.Sum(i => i.Quantity),
                    TotalSales = g.Sum(i => i.Subtotal)
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(10).ToList();

            summary.TopClients = sales
                .GroupBy(s => s.CustomerName)
                .Select(g => new RankedItem
                {
                    Name = g.Key,
                    Quantity = g.Count(),
                    TotalSales = g.Sum(s => s.Total)
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(5).ToList();

            // 5. Servicios (Recargas)
            summary.TopupReport = topups
                .GroupBy(t => t.Company)
                .Select(g => new ServiceReportItem
                {
                    Operator = g.Key,
                    TicketCount = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount),
                    TotalProfit = g.Sum(t => t.Profit)
                }).ToList();

            Summary = summary;
        }
    }
}
