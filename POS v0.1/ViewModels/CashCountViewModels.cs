using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;
using System.Collections.Generic;

namespace POS_v0_1.ViewModels
{
    public class CashCountHistoryViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public ObservableCollection<CashCount> CashCounts => _main.CashCounts;

        public ICommand ShowNewCountCommand { get; }

        public CashCountHistoryViewModel(MainViewModel main)
        {
            _main = main;
            ShowNewCountCommand = new RelayCommand(o => _main.CurrentView = new NewCashCountViewModel(_main));
        }
    }

    public class NewCashCountViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public ObservableCollection<DenominationViewModel> Denominations { get; }

        public decimal TotalCalculated => Denominations.Sum(d => d.Subtotal);

        // Propiedades para el resumen comparativo
        public decimal TheoreticalBalance => _main.Session?.TheoreticalBalance ?? 0m;
        public decimal Difference => TotalCalculated - TheoreticalBalance;

        private string _notes;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        private readonly Action<Models.CashCount> _onSaved;

        public NewCashCountViewModel(MainViewModel main, Action<Models.CashCount> onSaved = null)
        {
            _main = main;
            _onSaved = onSaved;

            Denominations = new ObservableCollection<DenominationViewModel>
            {
                new DenominationViewModel("Billete 1000", 1000),
                new DenominationViewModel("Billete 500", 500),
                new DenominationViewModel("Billete 200", 200),
                new DenominationViewModel("Billete 100", 100),
                new DenominationViewModel("Billete 50", 50),
                new DenominationViewModel("Billete 20", 20),
                new DenominationViewModel("Billete 10", 10),
                new DenominationViewModel("Billete 5", 5),
                new DenominationViewModel("Monedas C$", 1)
            };

            foreach (var item in Denominations)
            {
                item.PropertyChanged += (s, e) =>
                {
                    OnPropertyChanged(nameof(TotalCalculated));
                    OnPropertyChanged(nameof(Difference));
                };
            }

            SaveCommand = new RelayCommand(o =>
            {
                var count = new CashCount
                {
                    DateTime = DateTime.Now,
                    PerformedBy = _main.CurrentUser?.Name ?? "Sistema",
                    TotalCalculated = this.TotalCalculated,
                    Notes = this.Notes,
                    Details = Denominations.Select(d => new CashDenomination
                    {
                        Name = d.Name,
                        Value = d.Value,
                        Quantity = d.Quantity
                    }).ToList()
                };

                _main.Db.CashCounts.Add(count);
                _main.Db.SaveChanges();
                _main.CashCounts.Add(count);

                if (_onSaved != null)
                {
                    _onSaved(count);
                    // Regresar al control de caja después del cierre exitoso
                    _main.CurrentView = new CashControlViewModel(_main);
                }
                else
                {
                    _main.CurrentView = new CashCountHistoryViewModel(_main);
                }
            });

            BackCommand = new RelayCommand(o =>
            {
                if (_onSaved != null)
                    _main.CurrentView = new CashControlViewModel(_main);
                else
                    _main.CurrentView = new CashCountHistoryViewModel(_main);
            });
        }
    }

    public class DenominationViewModel : BaseViewModel
    {
        public string Name { get; }
        public decimal Value { get; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Subtotal));
            }
        }

        public decimal Subtotal => Value * Quantity;

        public DenominationViewModel(string name, decimal value)
        {
            Name = name;
            Value = value;
        }
    }
}
