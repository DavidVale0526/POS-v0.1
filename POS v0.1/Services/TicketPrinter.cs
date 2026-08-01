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
        private bool _isReprint;

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

                PrintDocument pd = new PrintDocument();
                int paperWidth = _config.TicketFormat == "80mm" ? 315 : 220;
                int itemCount = _sale.Items?.Count ?? 0;
                int calculatedHeight = 290 + (itemCount * 22);

                pd.DefaultPageSettings.PaperSize = new PaperSize("Ticket", paperWidth, calculatedHeight);
                pd.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);
                pd.PrintPage += new PrintPageEventHandler(PrintPageEvent);

                if (showPreview)
                {
                    ShowWpfPreview(pd, paperWidth, calculatedHeight);
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

        private void ShowWpfPreview(PrintDocument pd, int width, int height)
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
                    Title = $"Vista Previa - Ticket #{_sale.Id}",
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
                int maxDescChars = is80mm ? 24 : 14;

                foreach (var item in _sale.Items)
                {
                    string name = item.ProductName ?? "Producto";
                    string desc = name.Length > maxDescChars ? name.Substring(0, maxDescChars) : name;

                    g.DrawString($"{item.Quantity}x", fontRegular, System.Drawing.Brushes.Black, margin, y);
                    g.DrawString(desc, fontRegular, System.Drawing.Brushes.Black, colDescX, y);
                    g.DrawString(item.Subtotal.ToString("N2"), fontRegular, System.Drawing.Brushes.Black, colTotalX, y, right);
                    y += 14f;
                }
            }

            g.DrawString(dashes, fontRegular, System.Drawing.Brushes.Black, margin, y);
            y += 14f;

            // 5. Totales
            g.DrawString("SUBTOTAL:", fontRegular, System.Drawing.Brushes.Black, margin, y);
            g.DrawString($"C$ {_sale.Subtotal:N2}", fontRegular, System.Drawing.Brushes.Black, colTotalX, y, right);
            y += 14f;

            if (_sale.Discount > 0)
            {
                g.DrawString("DESCUENTO:", fontRegular, System.Drawing.Brushes.Black, margin, y);
                g.DrawString($"-C$ {_sale.Discount:N2}", fontRegular, System.Drawing.Brushes.Black, colTotalX, y, right);
                y += 14f;
            }

            if (_sale.Iva > 0)
            {
                g.DrawString($"IVA ({_config.TaxRate}%):", fontRegular, System.Drawing.Brushes.Black, margin, y);
                g.DrawString($"C$ {_sale.Iva:N2}", fontRegular, System.Drawing.Brushes.Black, colTotalX, y, right);
                y += 14f;
            }

            g.DrawString("TOTAL:", fontHeader, System.Drawing.Brushes.Black, margin, y);
            g.DrawString($"C$ {_sale.Total:N2}", fontHeader, System.Drawing.Brushes.Black, colTotalX, y, right);
            y += 24f;

            // 6. Pie de Página
            g.DrawString("¡Gracias por su compra!", fontBold, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 22), center);
            y += 18f;
            g.DrawString("Favor revisar su mercadería.", fontRegular, System.Drawing.Brushes.Black, new RectangleF(0, y, paperWidth, 22), center);
            y += 30f;
        }
    }
}
