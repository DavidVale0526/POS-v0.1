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

        private string _notes;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }

        public NewCashCountViewModel(MainViewModel main)
        {
            _main = main;

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
                item.PropertyChanged += (s, e) => OnPropertyChanged(nameof(TotalCalculated));
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
                _main.CurrentView = new CashCountHistoryViewModel(_main);
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new CashCountHistoryViewModel(_main));
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
