using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class FinancialMovementsViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public ObservableCollection<FinancialMovement> Movements => _main.FinancialMovements;

        private ObservableCollection<FinancialMovement> _filteredMovements;
        public ObservableCollection<FinancialMovement> FilteredMovements
        {
            get => _filteredMovements;
            set { _filteredMovements = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> TypeFilters { get; } = new ObservableCollection<string> { "Tipo de Movimiento", "Gasto", "Ingreso" };
        public ObservableCollection<string> SourceFilters { get; } = new ObservableCollection<string> { "Cualquier Fuente / Origen" };

        private string _selectedType = "Tipo de Movimiento";
        public string SelectedType { get => _selectedType; set { _selectedType = value; OnPropertyChanged(); ApplyFilter(); } }

        private string _selectedSource = "Cualquier Fuente / Origen";
        public string SelectedSource { get => _selectedSource; set { _selectedSource = value; OnPropertyChanged(); ApplyFilter(); } }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate { get => _selectedDate; set { _selectedDate = value; OnPropertyChanged(); ApplyFilter(); } }

        public ICommand ShowCreateCommand { get; }
        public ICommand ShowSourcesCommand { get; }
        public ICommand FilterCommand { get; }

        public FinancialMovementsViewModel(MainViewModel main)
        {
            _main = main;

            // Sync sources for filter
            foreach (var src in _main.FinancialSources)
                SourceFilters.Add(src.Name);

            ApplyFilter();

            ShowCreateCommand = new RelayCommand(o => _main.CurrentView = new CreateFinancialMovementViewModel(_main));
            ShowSourcesCommand = new RelayCommand(o => _main.CurrentView = new FinancialSourcesViewModel(_main));
            FilterCommand = new RelayCommand(o => ApplyFilter());
        }

        private void ApplyFilter()
        {
            var filtered = Movements.AsEnumerable();

            if (SelectedType != "Tipo de Movimiento")
                filtered = filtered.Where(m => m.Type == SelectedType);

            if (SelectedSource != "Cualquier Fuente / Origen")
                filtered = filtered.Where(m => m.Source == SelectedSource);

            if (SelectedDate.HasValue)
                filtered = filtered.Where(m => m.Date.Date == SelectedDate.Value.Date);

            FilteredMovements = new ObservableCollection<FinancialMovement>(filtered.OrderByDescending(m => m.Date));
        }
    }

    public class FinancialSourcesViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;
        public ObservableCollection<FinancialSource> Sources => _main.FinancialSources;

        private string _newName;
        public string NewName { get => _newName; set { _newName = value; OnPropertyChanged(); } }

        private string _newDescription;
        public string NewDescription { get => _newDescription; set { _newDescription = value; OnPropertyChanged(); } }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public FinancialSourcesViewModel(MainViewModel main)
        {
            _main = main;

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(NewName)) return;
                var source = new FinancialSource
                {
                    Name = NewName,
                    Description = NewDescription
                };
                _main.Db.FinancialSources.Add(source);
                _main.Db.SaveChanges();
                Sources.Add(source);
                NewName = string.Empty;
                NewDescription = string.Empty;
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new FinancialMovementsViewModel(_main));
        }
    }

    public class CreateFinancialMovementViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        private string _type = "Gasto"; // Default
        public string Type { get => _type; set { _type = value; OnPropertyChanged(); } }

        public ObservableCollection<FinancialSource> Sources => _main.FinancialSources;
        private FinancialSource _selectedSource;
        public FinancialSource SelectedSource { get => _selectedSource; set { _selectedSource = value; OnPropertyChanged(); } }

        public string Concept { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Reference { get; set; }
        public string Notes { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand SetTypeCommand { get; }

        public CreateFinancialMovementViewModel(MainViewModel main)
        {
            _main = main;

            SetTypeCommand = new RelayCommand(o => Type = o.ToString());

            SaveCommand = new RelayCommand(o =>
            {
                if (string.IsNullOrWhiteSpace(Concept) || SelectedSource == null || Amount <= 0) return;

                var move = new FinancialMovement
                {
                    Date = Date,
                    Type = Type,
                    Concept = Concept,
                    Source = SelectedSource.Name,
                    Amount = Amount,
                    RegisteredBy = _main.CurrentUser?.Name ?? "Sistema",
                    Reference = Reference,
                    Notes = Notes
                };

                _main.Db.FinancialMovements.Add(move);
                var dbSource = _main.Db.FinancialSources.Find(SelectedSource.Id);
                if (dbSource != null) dbSource.MovementCount++;
                _main.Db.SaveChanges();

                _main.FinancialMovements.Add(move);
                SelectedSource.MovementCount++;
                _main.CurrentView = new FinancialMovementsViewModel(_main);
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new FinancialMovementsViewModel(_main));
        }
    }
}
