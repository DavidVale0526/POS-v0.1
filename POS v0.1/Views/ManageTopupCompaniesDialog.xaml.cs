using System;
using System.Windows;
using System.Windows.Controls;
using POS_v0_1.Models;
using POS_v0_1.ViewModels;
using System.Linq;

namespace POS_v0_1.Views
{
    public partial class ManageTopupCompaniesDialog : Window
    {
        private MainViewModel _main;

        public ManageTopupCompaniesDialog(MainViewModel main)
        {
            InitializeComponent();
            _main = main;
            DataContext = _main;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NewCompanyName.Text)) return;

            string colorCode = (ColorPicker.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "#64748B";

            var company = new TopupCompany
            {
                Name = NewCompanyName.Text.Trim().ToUpper(),
                Color = colorCode
            };

            _main.Db.TopupCompanies.Add(company);
            _main.Db.SaveChanges();
            _main.TopupCompanies.Add(company);

            NewCompanyName.Text = "";
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is TopupCompany company)
            {
                var result = MessageBox.Show($"¿Desea eliminar la compañía {company.Name}?", "Confirmar", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    _main.Db.TopupCompanies.Remove(company);
                    _main.Db.SaveChanges();
                    _main.TopupCompanies.Remove(company);
                }
            }
        }

        private void AddPreset_Click(object sender, RoutedEventArgs e)
        {
            if (PresetCompanyCombo.SelectedItem == null)
            {
                MessageBox.Show("Por favor, seleccione una compañía.");
                return;
            }

            string amountText = PresetAmount.Text.Replace(',', '.');
            string costText = PresetCost.Text.Replace(',', '.');

            if (decimal.TryParse(amountText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal amount) &&
                decimal.TryParse(costText, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal cost))
            {
                var co = (TopupCompany)PresetCompanyCombo.SelectedItem;

                // Evitar duplicados exactos
                if (_main.TopupPresets.Any(p => p.CompanyName == co.Name && p.Amount == amount))
                {
                    MessageBox.Show("Ya existe una recarga rápida para esta compañía con ese monto.");
                    return;
                }

                var preset = new TopupPreset
                {
                    CompanyName = co.Name,
                    Amount = amount,
                    Cost = cost
                };

                _main.Db.TopupPresets.Add(preset);
                _main.Db.SaveChanges();
                _main.TopupPresets.Add(preset);

                PresetAmount.Text = "";
                PresetCost.Text = "";
            }
            else
            {
                MessageBox.Show("Por favor, ingrese montos numéricos válidos.");
            }
        }

        private void DeletePreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is TopupPreset preset)
            {
                _main.Db.TopupPresets.Remove(preset);
                _main.Db.SaveChanges();
                _main.TopupPresets.Remove(preset);
            }
        }
    }
}
