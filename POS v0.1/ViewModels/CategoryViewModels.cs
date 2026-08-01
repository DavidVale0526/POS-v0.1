using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class CategoriesViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        public ObservableCollection<Category> Categories => _main.Categories;

        public ICommand ShowCreateCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand EditCategoryCommand { get; }

        public CategoriesViewModel(MainViewModel main)
        {
            _main = main;

            ShowCreateCategoryCommand = new RelayCommand(o => _main.CurrentView = new CreateCategoryViewModel(_main));

            DeleteCategoryCommand = new RelayCommand(o =>
            {
                if (o is Category cat)
                {
                    // Bloqueo 5: Categoría con productos
                    if (cat.ProductCount > 0)
                    {
                        MessageBox.Show($"No se puede eliminar la categoría '{cat.Name}' porque tiene {cat.ProductCount} productos asociados.", "Restricción de Integridad", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var res = MessageBox.Show($"¿Desea eliminar la categoría {cat.Name}?", "Confirmar", MessageBoxButton.YesNo);
                    if (res == MessageBoxResult.Yes)
                    {
                        _main.Categories.Remove(cat);
                        var dbCat = _main.Db.Categories.Find(cat.Id);
                        if (dbCat != null) _main.Db.Categories.Remove(dbCat);
                        _main.Db.SaveChanges();
                    }
                }
            });

            EditCategoryCommand = new RelayCommand(o =>
            {
                if (o is Category cat) _main.CurrentView = new CreateCategoryViewModel(_main, cat);
            });
        }
    }

    public class CreateCategoryViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        private Category _editingCategory;

        public string Title => _editingCategory == null ? "Crear Categoría" : "Editar Categoría";

        private string _name;
        public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

        private string _description;
        public string Description { get => _description; set { _description = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public CreateCategoryViewModel(MainViewModel main, Category category = null)
        {
            _main = main;
            _editingCategory = category;

            if (_editingCategory != null)
            {
                Name = _editingCategory.Name;
                Description = _editingCategory.Description;
            }

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(Name)) return;

                if (_editingCategory == null)
                {
                    var newCat = new Category
                    {
                        Name = Name,
                        Description = Description
                    };
                    _main.Db.Categories.Add(newCat);
                    _main.Db.SaveChanges();
                    _main.Categories.Add(newCat);
                }
                else
                {
                    _editingCategory.Name = Name;
                    _editingCategory.Description = Description;
                    _main.Db.SaveChanges();
                }
                _main.CurrentView = new CategoriesViewModel(_main);
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new CategoriesViewModel(_main));
        }
    }
}
