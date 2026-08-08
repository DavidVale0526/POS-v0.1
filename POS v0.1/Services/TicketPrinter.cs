using System;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using POS_v0_1.Models;
using POS_v0_1.Data;

namespace POS_v0_1.Services
{
    public class TicketPrinter
    {
        private readonly BusinessConfiguration _config;
        private Sale _sale;
        private RepairOrder _repair;
        private BusinessSummary _report;
        private bool _isReprint;
        private string _printMode = "Sale"; // "Sale", "Repair", "Report"

        public TicketPrinter()
        {
            using (var db = new AppDbContext())
            {
                _config = db.BusinessConfigs.FirstOrDefault() ?? new BusinessConfiguration();
            }
        }

        public void PrintSaleTicket(Sale sale, bool isReprint = false, bool showPreview = true)
        {
            try
            {
                _sale = sale;
                _isReprint = isReprint;
                _printMode = "Sale";

                PrintDocument pd = new PrintDocument();
                int paperWidth = _config.TicketFormat == "80mm" ? 315 : 220;
                int itemCount = _sale.Items?.Count ?? 0;
                int calculatedHeight = 290 + (itemCount * 22);

                pd.DefaultPageSettings.PaperSize = new PaperSize("Ticket", paperWidth, calculatedHeight);
                pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                pd.PrintPage += new PrintPageEventHandler(PrintPageEvent);

                if (showPreview)
                {
                    ShowWpfPreview(pd, paperWidth, calculatedHeight, $"Ticket #{_sale.Id}");
                }
                else
                {
                    pd.Print();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error al procesar ticket: " + ex.Message);
            }
        }

        public void PrintBusinessReport(BusinessSummary report, string periodName, bool showPreview = true)
        {
            try
            {
                _report = report;
                _printMode = "Report";

                PrintDocument pd = new PrintDocument();
                int paperWidth = _config.TicketFormat == "80mm" ? 315 : 220;
                int calculatedHeight = 800; // Reportes son largos

                pd.DefaultPageSettings.PaperSize = new PaperSize("Report", paperWidth, calculatedHeight);
                pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                pd.PrintPage += new PrintPageEventHandler(PrintPageEvent);

                if (showPreview)
                {
                    ShowWpfPreview(pd, paperWidth, calculatedHeight, $"Reporte - {periodName}");
                }
                else
                {
                    pd.Print();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error al imprimir reporte: " + ex.Message);
            }
        }

        public void PrintRepairTicket(RepairOrder repair, bool showPreview = true)
        {
            try
            {
                _repair = repair;
                _printMode = "Repair";

                PrintDocument pd = new PrintDocument();
                int paperWidth = _config.TicketFormat == "80mm" ? 315 : 220;
                int calculatedHeight = 500; // Suficiente para datos de cliente y diagnóstico

                pd.DefaultPageSettings.PaperSize = new PaperSize("RepairTicket", paperWidth, calculatedHeight);
                pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                pd.PrintPage += new PrintPageEventHandler(PrintPageEvent);

                if (showPreview)
                {
                    ShowWpfPreview(pd, paperWidth, calculatedHeight, $"Orden de Servicio #{_repair.Id}");
                }
                else
                {
                    pd.Print();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error al imprimir ticket de reparación: " + ex.Message);
            }
        }

        private void ShowWpfPreview(PrintDocument pd, int width, int height, string title)
        {
            int scale = 2;
            int bmpWidth = width * scale;
            int bmpHeight = height * scale;

            using (Bitmap bmp = new Bitmap(bmpWidth, bmpHeight))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.ScaleTransform(scale, scale);
                    g.Clear(System.Drawing.Color.White);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                    PrintPageEventArgs args = new PrintPageEventArgs(g, new Rectangle(0, 0, width, height), new Rectangle(0, 0, width, height), pd.DefaultPageSettings);
                    PrintPageEvent(this, args);
                }

                var imgSource = BitmapToImageSource(bmp);

                var previewWindow = new Window
                {
                    Title = $"Vista Previa - {title}",
                    Width = 440,
                    Height = 650,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    Background = System.Windows.Media.Brushes.DarkGray,
                    Owner = System.Windows.Application.Current?.MainWindow
                };

                var mainGrid = new Grid();
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var scroll = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Margin = new Thickness(10)
                };

                var imgControl = new System.Windows.Controls.Image
                {
                    Source = imgSource,
                    Width = 320,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Top
                };
                scroll.Content = imgControl;
                Grid.SetRow(scroll, 0);
                mainGrid.Children.Add(scroll);

                var buttonPanel = new StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    Margin = new Thickness(10)
                };

                var btnPrint = new System.Windows.Controls.Button
                {
                    Content = "🖨️ Imprimir Ticket",
                    Width = 140,
                    Height = 36,
                    Margin = new Thickness(5),
                    Background = System.Windows.Media.Brushes.DarkGreen,
                    Foreground = System.Windows.Media.Brushes.White,
                    FontWeight = FontWeights.Bold,
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                btnPrint.Click += (s, e) =>
                {
                    try
                    {
                        pd.Print();
                        previewWindow.Close();
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show("Error al imprimir: " + ex.Message);
                    }
                };

                var btnClose = new System.Windows.Controls.Button
                {
                    Content = "Cerrar",
                    Width = 90,
                    Height = 36,
                    Margin = new Thickness(5),
                    Cursor = System.Windows.Input.Cursors.Hand
                };
                btnClose.Click += (s, e) => previewWindow.Close();

                buttonPanel.Children.Add(btnPrint);
                buttonPanel.Children.Add(btnClose);
                Grid.SetRow(buttonPanel, 1);
                mainGrid.Children.Add(buttonPanel);

                previewWindow.Content = mainGrid;
                previewWindow.ShowDialog();
            }
        }

        private System.Windows.Media.ImageSource BitmapToImageSource(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        private void PrintPageEvent(object sender, PrintPageEventArgs e)
        {
            if (_printMode == "Report")
            {
                PrintReportPage(e);
            }
            else if (_printMode == "Repair")
            {
                PrintRepairPage(e);
            }
            else
            {
                PrintSalePage(e);
            }
        }

        private void PrintReportPage(PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            bool is80mm = _config.TicketFormat == "80mm";
            float paperWidth = is80mm ? 315f : 220f;
            float margin = 6f;

            System.Drawing.Font fontRegular = new System.Drawing.Font("Consolas", 7.5f, System.Drawing.FontStyle.Regular);
            System.Drawing.Font fontBold = new System.Drawing.Font("Consolas", 8f, System.Drawing.FontStyle.Bold);
            System.Drawing.Font fontTitle = new System.Drawing.Font("Consolas", 10.5f, System.Drawing.FontStyle.Bold);
            System.Drawing.Font fontHeader = new System.Drawing.Font("Consolas", 8f, System.Drawing.FontStyle.Bold);

            float y = 10f;
            StringFormat center = new StringFormat { Alignment = StringAlignment.Center };
            StringFormat right = new StringFormat { Alignment = StringAlignment.Far };
            string dashes = new string('-', is80mm ? 44 : 31);

            g.DrawString("REPORTE FINANCIERO", fontTitle, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 22), center);
            y += 20f;
            g.DrawString(DateTime.Now.ToString("dd/MM/yyyy HH:mm"), fontRegular, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 18), center);
            y += 20f;

            g.DrawString(dashes, fontRegular, System.Drawing.Brushes.Black, margin, y);
            y += 14f;

            g.DrawString("INGRESOS", fontBold, System.Drawing.Brushes.Black, margin, y);
            y += 14f;
            g.DrawString("Ventas Brutas:", fontRegular, System.Drawing.Brushes.Black, margin, y);
            g.DrawString(_report.GrossSales.ToString("N2"), fontRegular, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 14f;
            g.DrawString("Descuentos:", fontRegular, System.Drawing.Brushes.Black, margin, y);
            g.DrawString("-" + _report.TotalDiscounts.ToString("N2"), fontRegular, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 14f;
            g.DrawString("Ventas POS (Netas):", fontBold, System.Drawing.Brushes.Black, margin, y);
            g.DrawString(_report.PosSalesIncome.ToString("N2"), fontBold, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 14f;

            // Desglose de Ventas
            g.DrawString("  • Efectivo:", fontRegular, System.Drawing.Brushes.Black, margin, y);
            g.DrawString(_report.PosCashIncome.ToString("N2"), fontRegular, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 14f;
            g.DrawString("  • Tarjeta:", fontRegular, System.Drawing.Brushes.Black, margin, y);
            g.DrawString(_report.PosCardIncome.ToString("N2"), fontRegular, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 14f;
            g.DrawString("  • Transferencia:", fontRegular, System.Drawing.Brushes.Black, margin, y);
            g.DrawString(_report.PosTransferIncome.ToString("N2"), fontRegular, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 14f;

            g.DrawString("Recargas:", fontRegular, System.Drawing.Brushes.Black, margin, y);
            g.DrawString(_report.TopupIncome.ToString("N2"), fontRegular, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 14f;
            g.DrawString("Reparaciones:", fontRegular, System.Drawing.Brushes.Black, margin, y);
            g.DrawString(_report.RepairIncome.ToString("N2"), fontRegular, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 14f;
            g.DrawString("Otros:", fontRegular, System.Drawing.Brushes.Black, margin, y);
            g.DrawString(_report.OtherExtraIncome.ToString("N2"), fontRegular, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 14f;
            g.DrawString("TOTAL INGRESOS:", fontBold, System.Drawing.Brushes.Black, margin, y);
            g.DrawString(_report.TotalGrossIncome.ToString("N2"), fontBold, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 20f;

            g.DrawString("EGRESOS Y COSTOS", fontBold, System.Drawing.Brushes.Black, margin, y);
            y += 14f;
            g.DrawString("Costo de Ventas:", fontRegular, System.Drawing.Brushes.Black, margin, y);
            g.DrawString(_report.CostOfGoodsSold.ToString("N2"), fontRegular, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 14f;
            g.DrawString("Gastos/Egresos:", fontRegular, System.Drawing.Brushes.Black, margin, y);
            g.DrawString(_report.ExpensesAndOutflows.ToString("N2"), fontRegular, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 14f;
            g.DrawString("TOTAL EGRESOS:", fontBold, System.Drawing.Brushes.Black, margin, y);
            g.DrawString(_report.TotalOutflows.ToString("N2"), fontBold, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 20f;

            g.DrawString(dashes, fontRegular, System.Drawing.Brushes.Black, margin, y);
            y += 14f;
            g.DrawString("UTILIDAD NETA:", fontTitle, System.Drawing.Brushes.Black, margin, y);
            g.DrawString(_report.TotalNetProfit.ToString("N2"), fontTitle, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 25f;

            g.DrawString("EFECTIVO EN MANO:", fontBold, System.Drawing.Brushes.Black, margin, y);
            g.DrawString(_report.ActualCashInHand.ToString("N2"), fontBold, System.Drawing.Brushes.Black, paperWidth - margin, y, right);
            y += 20f;

            g.DrawString("--- Fin de Reporte ---", fontRegular, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 20), center);
        }

        private void PrintSalePage(PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            bool is80mm = _config.TicketFormat == "80mm";
            float paperWidth = is80mm ? 315f : 220f;
            float margin = 6f;

            System.Drawing.Font fontRegular = new System.Drawing.Font("Consolas", 7.5f, System.Drawing.FontStyle.Regular);
            System.Drawing.Font fontBold = new System.Drawing.Font("Consolas", 8f, System.Drawing.FontStyle.Bold);
            System.Drawing.Font fontTitle = new System.Drawing.Font("Consolas", 10.5f, System.Drawing.FontStyle.Bold);
            System.Drawing.Font fontHeader = new System.Drawing.Font("Consolas", 8f, System.Drawing.FontStyle.Bold);

            float y = 10f;
            StringFormat center = new StringFormat { Alignment = StringAlignment.Center };
            StringFormat right = new StringFormat { Alignment = StringAlignment.Far };

            string dashes = new string('-', is80mm ? 44 : 31);

            // 1. Logo (si existe)
            if (!string.IsNullOrEmpty(_config.LogoPath) && System.IO.File.Exists(_config.LogoPath))
            {
                try
                {
                    using (System.Drawing.Image logo = System.Drawing.Image.FromFile(_config.LogoPath))
                    {
                        int imgWidth = (int)(paperWidth / 2.2f);
                        int imgHeight = (logo.Height * imgWidth) / logo.Width;
                        g.DrawImage(logo, (paperWidth - imgWidth) / 2f, y, imgWidth, imgHeight);
                        y += imgHeight + 8f;
                    }
                }
                catch { }
            }

            // 2. Encabezado de la Empresa
            g.DrawString(_config.BusinessName ?? "Mi Tienda", fontTitle, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 22), center);
            y += 18f;

            if (_isReprint)
            {
                g.DrawString("*** RE-IMPRESO ***", fontBold, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 18), center);
                y += 15f;
            }

            if (!string.IsNullOrEmpty(_config.Address))
            {
                g.DrawString(_config.Address, fontRegular, System.Drawing.Brushes.Black, new RectangleF(5, y, paperWidth - 10, 30), center);
                y += 24f;
            }

            if (!string.IsNullOrEmpty(_config.Phone))
            {
                g.DrawString("Tel: " + _config.Phone, fontRegular, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 18), center);
                y += 15f;
            }

            if (!string.IsNullOrEmpty(_config.Email))
            {
                g.DrawString(_config.Email, fontRegular, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 18), center);
                y += 15f;
            }

            g.DrawString(dashes, fontRegular, System.Drawing.Brushes.Black, margin, y);
            y += 14f;

            // 3. Info de Venta
            g.DrawString("Ticket: #" + _sale.Id, fontHeader, System.Drawing.Brushes.Black, margin, y);
            y += 14f;
            g.DrawString("Fecha:  " + _sale.Date.ToString("dd/MM/yyyy HH:mm"), fontRegular, System.Drawing.Brushes.Black, margin, y);
            y += 14f;
            g.DrawString("Cajero: " + (_sale.SellerName ?? "Admin"), fontRegular, System.Drawing.Brushes.Black, margin, y);
            y += 14f;
            g.DrawString("Cliente: " + (_sale.CustomerName ?? "Consumidor Final"), fontRegular, System.Drawing.Brushes.Black, margin, y);
            y += 16f;

            g.DrawString(dashes, fontRegular, System.Drawing.Brushes.Black, margin, y);
            y += 14f;

            // 4. Encabezado Tabla Productos
            float colQtyWidth = 36f;
            float colDescX = margin + colQtyWidth;
            float colTotalX = paperWidth - margin;

            g.DrawString("CANT", fontBold, System.Drawing.Brushes.Black, margin, y);
            g.DrawString("DESCRIPCIÓN", fontBold, System.Drawing.Brushes.Black, colDescX, y);
            g.DrawString("TOTAL", fontBold, System.Drawing.Brushes.Black, colTotalX, y, right);
            y += 15f;

            // Detalle de Productos
            if (_sale.Items != null)
            {
                foreach (var item in _sale.Items)
                {
                    string name = item.ProductName ?? "Producto";

                    // Línea 1: Cantidad y Nombre
                    g.DrawString($"{item.Quantity}x {name}", fontBold, System.Drawing.Brushes.Black, margin, y);
                    y += 13f;

                    // Línea 2: Precio unitario, Descuento y Subtotal de línea
                    string detail = $"   {item.UnitPrice:N2}";
                    if (item.Discount > 0)
                    {
                        detail += $" (-{item.Discount:N2})";
                    }

                    g.DrawString(detail, fontRegular, System.Drawing.Brushes.Black, margin, y);
                    g.DrawString(item.Subtotal.ToString("N2"), fontRegular, System.Drawing.Brushes.Black, colTotalX, y, right);
                    y += 13f;

                    // Línea 3: IMEI (si existe)
                    if (!string.IsNullOrEmpty(item.Imei))
                    {
                        g.DrawString($"   IMEI: {item.Imei}", new System.Drawing.Font("Consolas", 6.5f, System.Drawing.FontStyle.Italic), System.Drawing.Brushes.DimGray, margin, y);
                        y += 12f;
                    }

                    y += 2f; // Espaciado entre items
                }
            }

            g.DrawString(dashes, fontRegular, System.Drawing.Brushes.Black, margin, y);
            y += 14f;

            // 5. Totales
            g.DrawString("SUBTOTAL BRUTO:", fontRegular, System.Drawing.Brushes.Black, margin, y);
            g.DrawString($"C$ {_sale.GrossAmount:N2}", fontRegular, System.Drawing.Brushes.Black, colTotalX, y, right);
            y += 14f;

            decimal totalSavings = _sale.TotalDiscount;
            if (totalSavings > 0)
            {
                g.DrawString("DESCUENTOS TOT.:", fontRegular, System.Drawing.Brushes.Black, margin, y);
                g.DrawString($"-C$ {totalSavings:N2}", fontRegular, System.Drawing.Brushes.Black, colTotalX, y, right);
                y += 14f;
            }

            if (_sale.Iva > 0)
            {
                g.DrawString($"IVA ({_config.TaxRate}%):", fontRegular, System.Drawing.Brushes.Black, margin, y);
                g.DrawString($"C$ {_sale.Iva:N2}", fontRegular, System.Drawing.Brushes.Black, colTotalX, y, right);
                y += 14f;
            }

            g.DrawString("TOTAL A PAGAR:", fontHeader, System.Drawing.Brushes.Black, margin, y);
            g.DrawString($"C$ {_sale.Total:N2}", fontHeader, System.Drawing.Brushes.Black, colTotalX, y, right);
            y += 24f;

            // 6. Pie de Página
            g.DrawString("¡Gracias por su compra!", fontBold, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 22), center);
            y += 18f;
            g.DrawString("Favor revisar su mercadería.", fontRegular, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 22), center);
            y += 30f;
        }

        private void PrintRepairPage(PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            bool is80mm = _config.TicketFormat == "80mm";
            float paperWidth = is80mm ? 315f : 220f;
            float margin = 6f;

            System.Drawing.Font fontRegular = new System.Drawing.Font("Consolas", 7.5f, System.Drawing.FontStyle.Regular);
            System.Drawing.Font fontBold = new System.Drawing.Font("Consolas", 8f, System.Drawing.FontStyle.Bold);
            System.Drawing.Font fontTitle = new System.Drawing.Font("Consolas", 10.5f, System.Drawing.FontStyle.Bold);
            System.Drawing.Font fontHeader = new System.Drawing.Font("Consolas", 8f, System.Drawing.FontStyle.Bold);

            float y = 10f;
            StringFormat center = new StringFormat { Alignment = StringAlignment.Center };
            StringFormat left = new StringFormat { Alignment = StringAlignment.Near };
            string dashes = new string('-', is80mm ? 44 : 31);

            // 1. Logo
            if (!string.IsNullOrEmpty(_config.LogoPath) && System.IO.File.Exists(_config.LogoPath))
            {
                try
                {
                    using (System.Drawing.Image logo = System.Drawing.Image.FromFile(_config.LogoPath))
                    {
                        int imgWidth = (int)(paperWidth / 2.5f);
                        int imgHeight = (logo.Height * imgWidth) / logo.Width;
                        g.DrawImage(logo, (paperWidth - imgWidth) / 2f, y, imgWidth, imgHeight);
                        y += imgHeight + 8f;
                    }
                }
                catch { }
            }

            // 2. Encabezado
            g.DrawString(_config.BusinessName ?? "Mi Tienda", fontTitle, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 22), center);
            y += 18f;
            g.DrawString("ORDEN DE SERVICIO TÉCNICO", fontBold, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 18), center);
            y += 15f;

            g.DrawString(dashes, fontRegular, System.Drawing.Brushes.Black, margin, y);
            y += 14f;

            // 3. Datos de la Orden
            g.DrawString($"ORDEN: #{_repair.Id}", fontHeader, System.Drawing.Brushes.Black, margin, y);
            y += 14f;
            g.DrawString($"FECHA: {_repair.EntryDate:dd/MM/yyyy HH:mm}", fontRegular, System.Drawing.Brushes.Black, margin, y);
            y += 14f;
            g.DrawString($"CLIENTE: {_repair.ClientName}", fontBold, System.Drawing.Brushes.Black, margin, y);
            y += 14f;
            if (!string.IsNullOrEmpty(_repair.ClientPhone))
            {
                g.DrawString($"TEL: {_repair.ClientPhone}", fontRegular, System.Drawing.Brushes.Black, margin, y);
                y += 14f;
            }

            g.DrawString(dashes, fontRegular, System.Drawing.Brushes.Black, margin, y);
            y += 14f;

            // 4. Datos del Equipo
            g.DrawString("EQUIPO:", fontBold, System.Drawing.Brushes.Black, margin, y);
            y += 14f;
            g.DrawString($"{_repair.DeviceBrand} {_repair.DeviceModel}", fontRegular, System.Drawing.Brushes.Black, margin, y);
            y += 14f;
            if (!string.IsNullOrEmpty(_repair.IMEI))
            {
                g.DrawString($"IMEI/SN: {_repair.IMEI}", fontRegular, System.Drawing.Brushes.Black, margin, y);
                y += 14f;
            }

            y += 10f;
            g.DrawString("FALLA REPORTADA:", fontBold, System.Drawing.Brushes.Black, margin, y);
            y += 14f;
            RectangleF failureRect = new RectangleF(margin, y, paperWidth - (margin * 2), 60);
            g.DrawString(_repair.ReportedFailure ?? "No especificada", fontRegular, System.Drawing.Brushes.Black, failureRect, left);
            y += 45f;

            g.DrawString(dashes, fontRegular, System.Drawing.Brushes.Black, margin, y);
            y += 14f;

            // 5. Términos y Firma
            g.DrawString("CONDICIONES:", fontBold, System.Drawing.Brushes.Black, margin, y);
            y += 14f;
            string terms = "- No hay garantía por humedad o golpes.\n- 30 días para retirar, luego se declara abandono.";
            g.DrawString(terms, new System.Drawing.Font("Consolas", 6.5f), System.Drawing.Brushes.Black, new RectangleF(margin, y, paperWidth - (margin * 2), 40), left);
            y += 40f;

            y += 20f;
            g.DrawString("_______________________", fontRegular, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 18), center);
            y += 15f;
            g.DrawString("Firma de Recibido", fontRegular, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 18), center);
            y += 25f;

            g.DrawString("--- Fin de Orden ---", fontRegular, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 20), center);
        }
    }
}
