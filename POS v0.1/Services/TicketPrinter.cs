using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using POS_v0_1.Models;
using POS_v0_1.Data;

namespace POS_v0_1.Services
{
    public class TicketPrinter
    {
        private readonly BusinessConfiguration _config;
        private Sale _sale;

        public TicketPrinter()
        {
            using (var db = new AppDbContext())
            {
                _config = db.BusinessConfigs.FirstOrDefault() ?? new BusinessConfiguration();
            }
        }

        public void PrintSaleTicket(Sale sale)
        {
            try
            {
                _sale = sale;
                PrintDocument pd = new PrintDocument();

                // Configurar el ancho del papel según la configuración
                // 58mm es aprox 220 centésimas de pulgada, 80mm aprox 315
                int width = _config.TicketFormat == "80mm" ? 315 : 220;
                pd.DefaultPageSettings.PaperSize = new PaperSize("Custom", width, 0); // Altura 0 para rollo continuo

                pd.PrintPage += new PrintPageEventHandler(PrintPageEvent);
                pd.Print();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error al imprimir: " + ex.Message);
            }
        }

        private void PrintPageEvent(object sender, PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            int width = _config.TicketFormat == "80mm" ? 300 : 200;
            Font fontRegular = new Font("Courier New", 8);
            Font fontBold = new Font("Courier New", 9, FontStyle.Bold);
            Font fontTitle = new Font("Courier New", 12, FontStyle.Bold);

            float y = 10;
            float margin = 10;
            StringFormat center = new StringFormat { Alignment = StringAlignment.Center };
            StringFormat right = new StringFormat { Alignment = StringAlignment.Far };

            // 1. Logo (si existe)
            if (!string.IsNullOrEmpty(_config.LogoPath) && System.IO.File.Exists(_config.LogoPath))
            {
                try
                {
                    Image logo = Image.FromFile(_config.LogoPath);
                    int imgWidth = width / 2;
                    int imgHeight = (logo.Height * imgWidth) / logo.Width;
                    g.DrawImage(logo, (width - imgWidth) / 2, y, imgWidth, imgHeight);
                    y += imgHeight + 10;
                }
                catch { }
            }

            // 2. Encabezado
            g.DrawString(_config.BusinessName, fontTitle, Brushes.Black, new RectangleF(0, y, width, 25), center);
            y += 20;

            if (!string.IsNullOrEmpty(_config.Address))
            {
                g.DrawString(_config.Address, fontRegular, Brushes.Black, new RectangleF(0, y, width, 40), center);
                y += 30;
            }

            if (!string.IsNullOrEmpty(_config.Phone))
            {
                g.DrawString("Tel: " + _config.Phone, fontRegular, Brushes.Black, new RectangleF(0, y, width, 20), center);
                y += 15;
            }

            g.DrawString(new string('-', 30), fontRegular, Brushes.Black, margin, y);
            y += 15;

            // 3. Info Venta
            g.DrawString("Ticket: #" + _sale.Id, fontBold, Brushes.Black, margin, y);
            y += 15;
            g.DrawString("Fecha:  " + _sale.Date.ToString("dd/MM/yyyy HH:mm"), fontRegular, Brushes.Black, margin, y);
            y += 15;
            g.DrawString("Cajero: " + (_sale.SellerName ?? "Admin"), fontRegular, Brushes.Black, margin, y);
            y += 15;
            g.DrawString("Cliente:" + (_sale.CustomerName ?? "Consumidor Final"), fontRegular, Brushes.Black, margin, y);
            y += 20;

            g.DrawString(new string('-', 30), fontRegular, Brushes.Black, margin, y);
            y += 15;

            // 4. Detalle de Productos
            g.DrawString("CANT  DESCRIPCIÓN      TOTAL", fontBold, fontRegular.Size, Brushes.Black, margin, y);
            y += 15;

            foreach (var item in _sale.Items)
            {
                string desc = item.ProductName.Length > 15 ? item.ProductName.Substring(0, 15) : item.ProductName;
                g.DrawString($"{item.Quantity}x {desc}", fontRegular, Brushes.Black, margin, y);
                g.DrawString(item.Subtotal.ToString("N2"), fontRegular, Brushes.Black, width - margin, y, right);
                y += 15;
            }

            g.DrawString(new string('-', 30), fontRegular, Brushes.Black, margin, y);
            y += 15;

            // 5. Totales
            g.DrawString("SUBTOTAL:", fontRegular, Brushes.Black, margin, y);
            g.DrawString(_sale.Subtotal.ToString("C$ N2"), fontRegular, Brushes.Black, width - margin, y, right);
            y += 15;

            if (_sale.Discount > 0)
            {
                g.DrawString("DESCUENTO:", fontRegular, Brushes.Black, margin, y);
                g.DrawString("-" + _sale.Discount.ToString("C$ N2"), fontRegular, Brushes.Black, width - margin, y, right);
                y += 15;
            }

            if (_sale.Iva > 0)
            {
                g.DrawString("IVA (" + _config.TaxRate + "%):", fontRegular, Brushes.Black, margin, y);
                g.DrawString(_sale.Iva.ToString("C$ N2"), fontRegular, Brushes.Black, width - margin, y, right);
                y += 15;
            }

            g.DrawString("TOTAL:", fontTitle, Brushes.Black, margin, y);
            g.DrawString(_sale.Total.ToString("C$ N2"), fontTitle, Brushes.Black, width - margin, y, right);
            y += 30;

            // 6. Pie de página
            g.DrawString("¡Gracias por su compra!", fontBold, Brushes.Black, new RectangleF(0, y, width, 20), center);
            y += 20;
            g.DrawString("Favor revisar su mercadería.", fontRegular, Brushes.Black, new RectangleF(0, y, width, 20), center);
            y += 40; // Espacio final para corte
        }
    }
}
