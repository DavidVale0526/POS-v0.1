using System.Windows;
using POS_v0_1.Models;

namespace POS_v0_1.Views
{
    public partial class QuickCategoryDialog : Window
    {
        public Category Category { get; private set; }

        public QuickCategoryDialog()
        {
            InitializeComponent();
            NameBox.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Category = new Category
            {
                Name = NameBox.Text,
                Description = DescBox.Text
            };

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
