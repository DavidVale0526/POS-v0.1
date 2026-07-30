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

        private string _reference;
        public string Reference
        {
            get => _reference;
            set { _reference = value; OnPropertyChanged(); }
        }

        private string _notes;
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
                decimal lastBalance = _main.BankTransactions.Any() ? _main.BankTransactions.Last().BalanceAfter : 0;
                decimal newBalance = Type == "Depósito" ? lastBalance + Amount : lastBalance - Amount;

                var transaction = new BankTransaction
                {
                    Id = _main.BankTransactions.Any() ? _main.BankTransactions.Max(t => t.Id) + 1 : 1,
                    Date = this.Date,
                    Type = this.Type,
                    Bank = this.Bank,
                    Reference = this.Reference,
                    Amount = this.Amount,
                    BalanceAfter = newBalance,
                    RegisteredBy = "Administrador",
                    Notes = this.Notes
                };

                _main.BankTransactions.Add(transaction);
                _main.CurrentView = new BankHistoryViewModel(_main);
            });

            BackCommand = new RelayCommand(o => _main.CurrentView = new BankHistoryViewModel(_main));
        }
    }
}
