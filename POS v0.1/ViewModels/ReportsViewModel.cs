using System;
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
            var topups = _main.Topups.Where(t => t.Date >= start && t.Date <= end).ToList();
            var repairs = _main.Repairs.Where(r => r.CreatedAt >= start && r.CreatedAt <= end).ToList(); // Simplificado
            var movements = _main.FinancialMovements.Where(m => m.Date >= start && m.Date <= end).ToList();

            var summary = new BusinessSummary();

            // Ventas Brutas
            summary.GrossSales = sales.Sum(s => s.Total);
            summary.PosSalesIncome = summary.GrossSales;

            // Costo de productos (Simulado por ahora como un % o buscando en el modelo si existiera)
            // En una app real, el modelo SaleItem tendria el CostPrice al momento de la venta.
            summary.CostOfGoodsSold = sales.Sum(s => s.Total * 0.7m); // Estimacion 70% costo

            // Recargas
            summary.TopupFlow = topups.Sum(t => t.Amount);
            summary.TopupIncome = topups.Sum(t => t.Amount); // O comision si se separa

            // Reparaciones
            var deliveredRepairs = repairs.Where(r => r.Status == "Entregado");
            summary.DeliveredRepairsTotal = deliveredRepairs.Sum(r => r.Price);
            summary.RepairIncome = summary.DeliveredRepairsTotal;

            // Otros ingresos y gastos (Tesorería)
            summary.OtherExtraIncome = movements.Where(m => m.Type == "Ingreso").Sum(m => m.Amount);
            summary.ExpensesAndOutflows = movements.Where(m => m.Type == "Gasto").Sum(m => m.Amount);

            // Utilidad Operativa Real (Ventas - Costo + Recargas + Reparaciones)
            summary.RealOperatingProfit = (summary.GrossSales - summary.CostOfGoodsSold) + (summary.TopupFlow * 0.05m) + summary.RepairIncome;

            // Caja Real
            summary.ActualCashInHand = (summary.TotalGrossIncome - summary.ExpensesAndOutflows);

            Summary = summary;
        }
    }
}
