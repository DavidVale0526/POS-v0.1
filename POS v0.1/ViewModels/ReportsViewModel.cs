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
        public ICommand ExportCommand { get; }
        public ICommand ExportPdfCommand { get; }

        public ReportsViewModel(MainViewModel main)
        {
            _main = main;
            ConsultCommand = new RelayCommand(o => CalculateSummary());
            PrintReportCommand = new RelayCommand(o =>
            {
                if (Summary == null) return;
                var printer = new Services.TicketPrinter();
                printer.PrintBusinessReport(Summary, SelectedPeriod);
            });

            ExportCommand = new RelayCommand(o => ExportToCsv());
            ExportPdfCommand = new RelayCommand(o => ExportToPdf());
            CalculateSummary();
        }

        private void ExportToPdf()
        {
            if (Summary == null) return;

            try
            {
                // Para implementar un reporte A4 formal sin librerías externas complejas,
                // usamos el PrintDialog de WPF que permite guardar como PDF de forma nativa.
                var printDialog = new System.Windows.Controls.PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Creamos un visual específico para A4
                    var visual = new System.Windows.Controls.StackPanel
                    {
                        Width = 800, // Aprox A4 width
                        Margin = new System.Windows.Thickness(50),
                        Background = System.Windows.Media.Brushes.White
                    };

                    // Header
                    visual.Children.Add(new System.Windows.Controls.TextBlock {
                        Text = _main.BusinessConfig?.BusinessName ?? "REPORTE DE NEGOCIO",
                        FontSize = 24, FontWeight = System.Windows.FontWeights.Bold,
                        Margin = new System.Windows.Thickness(0, 0, 0, 10)
                    });
                    visual.Children.Add(new System.Windows.Controls.TextBlock {
                        Text = $"Período: {SelectedPeriod} | Fecha: {ReferenceDate:dd/MM/yyyy}",
                        FontSize = 14, Margin = new System.Windows.Thickness(0, 0, 0, 30)
                    });

                    // Grid de Resumen
                    var grid = new System.Windows.Controls.Grid();
                    grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });

                    AddSummaryRow(visual, "Ventas Totales (Brutas)", $"C$ {Summary.GrossSales:N2}");
                    AddSummaryRow(visual, "Descuentos Aplicados", $"- C$ {Summary.TotalDiscounts:N2}");
                    AddSummaryRow(visual, "Ventas Netas (Ingreso POS)", $"C$ {Summary.NetSales:N2}", true);
                    AddSummaryRow(visual, "Utilidad Real Operativa", $"C$ {Summary.RealOperatingProfit:N2}");
                    AddSummaryRow(visual, "Costo de Ventas", $"C$ {Summary.CostOfGoodsSold:N2}");
                    AddSummaryRow(visual, "Gastos y Egresos", $"C$ {Summary.ExpensesAndOutflows:N2}");
                    AddSummaryRow(visual, "Utilidad Neta Final", $"C$ {Summary.TotalNetProfit:N2}", true);

                    visual.Children.Add(new System.Windows.Controls.Separator { Margin = new System.Windows.Thickness(0, 20, 0, 20) });

                    // Top Productos
                    visual.Children.Add(new System.Windows.Controls.TextBlock { Text = "TOP 10 PRODUCTOS MÁS VENDIDOS", FontWeight = System.Windows.FontWeights.Bold, Margin = new System.Windows.Thickness(0, 0, 0, 10) });
                    foreach (var item in Summary.TopProducts)
                    {
                        AddSummaryRow(visual, item.Name, $"{item.Quantity} uds - C$ {item.TotalSales:N2}");
                    }

                    // Forzar el layout
                    visual.Measure(new System.Windows.Size(printDialog.PrintableAreaWidth, double.PositiveInfinity));
                    visual.Arrange(new System.Windows.Rect(new System.Windows.Point(0, 0), visual.DesiredSize));

                    printDialog.PrintVisual(visual, "Reporte de Negocio A4");
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error al exportar PDF: " + ex.Message);
            }
        }

        private void AddSummaryRow(System.Windows.Controls.StackPanel parent, string label, string value, bool isBold = false)
        {
            var p = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, Margin = new System.Windows.Thickness(0, 5, 0, 5) };
            p.Children.Add(new System.Windows.Controls.TextBlock { Text = label + ": ", Width = 250, FontWeight = isBold ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal });
            p.Children.Add(new System.Windows.Controls.TextBlock { Text = value, FontWeight = isBold ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal });
            parent.Children.Add(p);
        }

        private void ExportToCsv()
        {
            if (Summary == null) return;

            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Archivo CSV (*.csv)|*.csv",
                    FileName = $"Reporte_{SelectedPeriod}_{DateTime.Now:yyyyMMdd}"
                };

                if (dialog.ShowDialog() == true)
                {
                    var lines = new List<string>
                    {
                        "Concepto,Monto",
                        $"Ventas Brutas,{Summary.GrossSales}",
                        $"Costo de Ventas,{Summary.CostOfGoodsSold}",
                        $"Utilidad Operativa Real,{Summary.RealOperatingProfit}",
                        $"Flujo Recargas,{Summary.TopupFlow}",
                        $"Reparaciones Cobradas,{Summary.DeliveredRepairsTotal}",
                        $"Ingresos Extras,{Summary.OtherExtraIncome}",
                        $"Gastos y Egresos,{Summary.ExpensesAndOutflows}",
                        $"Utilidad Neta Total,{Summary.TotalNetProfit}",
                        $"Efectivo en Mano,{Summary.ActualCashInHand}"
                    };

                    System.IO.File.WriteAllLines(dialog.FileName, lines, System.Text.Encoding.UTF8);
                    System.Windows.MessageBox.Show("Reporte exportado exitosamente.", "Éxito", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error al exportar: " + ex.Message);
            }
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
            var sessions = _main.CashSessions.Where(s => s.OpeningTime >= start && s.OpeningTime <= end).ToList();

            var summary = new BusinessSummary();

            // 1. Resumen General
            summary.GrossSales = sales.Sum(s => s.GrossAmount > 0 ? s.GrossAmount : (s.Total + s.TotalDiscount));
            summary.TotalDiscounts = sales.Sum(s => s.TotalDiscount);
            summary.NetSales = sales.Sum(s => s.Total);

            summary.PosSalesIncome = summary.NetSales;
            summary.PosCashIncome = sales.Sum(s => s.AmountCash);
            summary.PosCardIncome = sales.Sum(s => s.AmountCard);
            summary.PosTransferIncome = sales.Sum(s => s.AmountTransfer);

            // Cálculo real del costo de venta basado en el costo histórico guardado en SaleItem
            summary.CostOfGoodsSold = sales.SelectMany(s => s.Items).Sum(i => i.PurchasePrice * i.Quantity);

            summary.TopupFlow = topups.Sum(t => t.Amount);
            summary.TopupIncome = summary.TopupFlow;

            var deliveredRepairs = repairs.Where(r => r.Status == "Entregado");
            summary.DeliveredRepairsTotal = deliveredRepairs.Sum(r => r.Total);
            summary.RepairIncome = summary.DeliveredRepairsTotal;

            summary.OtherExtraIncome = movements.Where(m => m.Type == "Ingreso").Sum(m => m.Amount);
            summary.ExpensesAndOutflows = movements.Where(m => m.Type == "Gasto").Sum(m => m.Amount);

            // CORRECCIÓN: La utilidad operativa real debe basarse en el ingreso NETO (lo cobrado) menos el costo.
            summary.RealOperatingProfit = (summary.NetSales - summary.CostOfGoodsSold) + topups.Sum(t => t.Profit) + summary.RepairIncome;

            // Determinar efectivo en caja de forma más precisa
            var activeSession = _main.Session ?? _main.CashSessions.FirstOrDefault(s => s.IsOpen);

            if (activeSession != null && activeSession.IsOpen && activeSession.OpeningTime.Date == ReferenceDate.Date)
            {
                summary.IsSessionOpen = true;
                summary.CashTitleText = "Efectivo en Caja (En Curso)";
                summary.CashStatusText = "Caja Abierta (Calculado en tiempo real)";
                summary.ActualCashInHand = activeSession.TheoreticalBalance;
            }
            else
            {
                // Si el reporte es para hoy pero no hay caja abierta hoy, o es para otro día,
                // buscamos la última caja cerrada en el periodo seleccionado.
                var closedSessionsInPeriod = sessions.Where(s => !s.IsOpen && s.ClosingTime != null).ToList();
                if (closedSessionsInPeriod.Any())
                {
                    summary.IsSessionOpen = false;
                    summary.CashTitleText = "Saldo de Cierre (Auditado)";
                    var lastClosed = closedSessionsInPeriod.OrderByDescending(s => s.ClosingTime).First();
                    summary.CashStatusText = $"Caja Cerrada ({lastClosed.ClosingTime:dd/MM/yyyy HH:mm})";
                    summary.ActualCashInHand = lastClosed.ClosingBalance;
                }
                else
                {
                    summary.IsSessionOpen = false;
                    summary.CashTitleText = "Saldo de Caja";
                    summary.CashStatusText = "Caja Cerrada (Sin sesiones en este periodo)";
                    summary.ActualCashInHand = 0m;
                }
            }

            // 2. Finanzas por Fuente
            var allFinancial = new List<FinancialSourceSummary>();

            // Sumar ventas por método
            decimal salesCash = sales.Sum(s => s.AmountCash);
            decimal salesCard = sales.Sum(s => s.AmountCard);
            decimal salesTrans = sales.Sum(s => s.AmountTransfer);

            if (salesCash > 0) allFinancial.Add(new FinancialSourceSummary { Source = "Caja", Type = "Ventas POS", Total = salesCash });
            if (salesCard > 0) allFinancial.Add(new FinancialSourceSummary { Source = "Banco (Tarjeta)", Type = "Ventas POS", Total = salesCard });
            if (salesTrans > 0) allFinancial.Add(new FinancialSourceSummary { Source = "Banco (Transf.)", Type = "Ventas POS", Total = salesTrans });

            if (summary.TopupIncome > 0) allFinancial.Add(new FinancialSourceSummary { Source = "Caja", Type = "Recargas", Total = summary.TopupIncome });
            if (summary.RepairIncome > 0) allFinancial.Add(new FinancialSourceSummary { Source = "Caja", Type = "Reparaciones", Total = summary.RepairIncome });

            // Movimientos manuales
            allFinancial.AddRange(movements.GroupBy(m => new { SourceName = m.FinancialSource?.Name ?? "Desconocido", m.Type })
                .Select(g => new FinancialSourceSummary
                {
                    Source = g.Key.SourceName,
                    Type = g.Key.Type,
                    Total = g.Sum(m => m.Amount)
                }));

            summary.FinancialSummaries = allFinancial;

            // Datos para Gráfica de Utilidad
            summary.UtilityComparison = new List<RankedItem>
            {
                new RankedItem { Name = "Ventas Netas", TotalSales = summary.NetSales },
                new RankedItem { Name = "Costo Ventas", TotalSales = summary.CostOfGoodsSold },
                new RankedItem { Name = "Utilidad Op.", TotalSales = summary.RealOperatingProfit }
            };

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

            summary.SalesBySeller = sales
                .GroupBy(s => s.SellerName ?? "Usuario")
                .Select(g => new RankedItem
                {
                    Name = g.Key,
                    Quantity = g.Count(),
                    TotalSales = g.Sum(s => s.Total)
                })
                .OrderByDescending(x => x.TotalSales)
                .ToList();

            // 5. Servicios (Reparaciones y Recargas)
            summary.TopupReport = topups
                .GroupBy(t => t.Company)
                .Select(g => new ServiceReportItem
                {
                    Operator = g.Key,
                    TicketCount = g.Count(),
                    TotalAmount = g.Sum(t => t.Amount),
                    TotalProfit = g.Sum(t => t.Profit)
                }).ToList();

            summary.RepairsByTechnician = repairs
                .GroupBy(r => r.TechnicianName ?? "Sin Asignar")
                .Select(g => new ServiceReportItem
                {
                    Operator = g.Key,
                    TicketCount = g.Count(),
                    TotalAmount = g.Sum(r => r.Total),
                    TotalProfit = g.Where(r => r.Status == "Entregado").Sum(r => r.Total)
                }).ToList();

            summary.RepairsByReceiver = repairs
                .GroupBy(r => r.ReceivedByName ?? "Sin Asignar")
                .Select(g => new ServiceReportItem
                {
                    Operator = g.Key,
                    TicketCount = g.Count(),
                    TotalAmount = g.Sum(r => r.Total),
                    TotalProfit = g.Where(r => r.Status == "Entregado").Sum(r => r.Total)
                }).ToList();

            // 6. Inteligencia Visual (Fase 4)
            // Historial de ventas últimos 7 días
            var history = new List<DailySalesSummary>();
            for (int i = 6; i >= 0; i--)
            {
                var day = DateTime.Today.AddDays(-i);
                history.Add(new DailySalesSummary
                {
                    Date = day,
                    Total = _main.Sales.Where(s => s.Date.Date == day.Date && !s.IsCancelled).Sum(s => s.Total)
                });
            }
            summary.DailySalesHistory = history;

            // Top Categorías más rentables
            summary.TopCategories = sales.SelectMany(s => s.Items)
                .GroupBy(i => {
                    var prod = _main.Products.FirstOrDefault(p => p.Id == i.ProductId);
                    return prod?.Category?.Name ?? "General";
                })
                .Select(g => new RankedItem
                {
                    Name = g.Key,
                    Quantity = g.Sum(i => i.Quantity),
                    TotalSales = g.Sum(i => i.Subtotal)
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(5).ToList();

            Summary = summary;
        }
    }
}
