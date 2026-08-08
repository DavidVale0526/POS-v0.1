using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using POS_v0_1.Models;

namespace POS_v0_1.ViewModels
{
    public class BankHistoryViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        public ObservableCollection<BankTransaction> Transactions => _main.BankTransactions;

        public decimal CurrentBankBalance => Transactions.Any() ? Transactions.Last().BalanceAfter : 0;

        public ICommand ShowNewTransactionCommand { get; }

        public BankHistoryViewModel(MainViewModel main)
        {
            _main = main;
            ShowNewTransactionCommand = new RelayCommand(o => _main.CurrentView = new NewBankTransactionViewModel(_main));
        }
    }

    public class NewBankTransactionViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        private string _type = "Depósito"; // Default
        public string Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        private decimal _amount;
        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); }
        }

        private string _bank = "Banpro";
        public string Bank
        {
            get => _bank;
            set { _bank = value; OnPropertyChanged(); }
        }

        private DateTime _date = DateTime.Now;
        public DateTime Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        private string _reference = string.Empty;
        public string Reference
        {
            get => _reference;
            set { _reference = value; OnPropertyChanged(); }
        }

        private string _notes = string.Empty;
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        public decimal CurrentBalance => _main.BankTransactions.Any() ? _main.BankTransactions.Last().BalanceAfter : 0;

        public ICommand SaveCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand SetTypeCommand { get; }

        public NewBankTransactionViewModel(MainViewModel main)
        {
            _main = main;

            SetTypeCommand = new RelayCommand(o => Type = o.ToString());

            SaveCommand = new RelayCommand(o =>
            {
                if (Amount <= 0)
                {
                    System.Windows.MessageBox.Show("El monto debe ser mayor a cero.", "Validación", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Reference))
                {
                    System.Windows.MessageBox.Show("Debes ingresar un número de referencia o comprobante.", "Validación", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return;
                }

                decimal lastBalance = _main.BankTransactions.Any() ? _main.BankTransactions.Last().BalanceAfter : 0;
                decimal newBalance = Type == "Depósito" ? lastBalance + Amount : lastBalance - Amount;

                var transaction = new BankTransaction
                {
                    Date = this.Date,
                    Type = this.Type,
                    Bank = this.Bank ?? "Sin Banco",
                    Reference = this.Reference ?? "N/A",
                    Amount = this.Amount,
                    BalanceAfter = newBalance,
                    RegisteredBy = _main.CurrentUser?.Name ?? "Sistema",
                    Notes = this.Notes ?? ""
                };

                try
                {
                    _main.Db.BankTransactions.Add(transaction);
                    _main.Db.SaveChanges();
                    _main.BankTransactions.Add(transaction);
                    _main.CurrentView = new BankHistoryViewModel(_main);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error al guardar en base de datos: {ex.Message}\n\nDetalle: {ex.InnerException?.Message}", "Error Fatal", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new BankHistoryViewModel(_main));
        }
    }
}
