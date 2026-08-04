using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using POS_v0_1.Models;

namespace POS_v0_1.Views
{
    public partial class ProductSearchDialog : Window
    {
        private List<Product> _allProducts;
        public Product SelectedProduct { get; private set; }

        public ProductSearchDialog(List<Product> products)
        {
            InitializeComponent();
            _allProducts = products;
            ResultsList.ItemsSource = _allProducts;
            SearchBox.Focus();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string filter = SearchBox.Text.ToLower();
            ResultsList.ItemsSource = _allProducts.Where(p =>
                p.Name.ToLower().Contains(filter) ||
                (p.Barcode != null && p.Barcode.Contains(filter))).ToList();
        }

        private void ResultsList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ConfirmSelection();
        }

        private void Select_Click(object sender, RoutedEventArgs e)
        {
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            if (ResultsList.SelectedItem is Product prod)
            {
                SelectedProduct = prod;
                DialogResult = true;
                Close();
            }
        }
    }
}
