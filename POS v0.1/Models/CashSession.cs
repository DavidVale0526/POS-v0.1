using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace POS_v0_1.Models
{
    public class CashSession : INotifyPropertyChanged
    {
        public int Id { get; set; }
        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                _isOpen = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(IsTooOld)); // Notificar el cambio a la alerta
            }
        }

        public string OpenedBy { get; set; }
        public string? ClosedBy { get; set; }
        public DateTime OpeningTime { get; set; }
        public DateTime? ClosingTime { get; set; }

        private decimal _openingBalance;
        public decimal OpeningBalance { get => _openingBalance; set { _openingBalance = value; OnPropertyChanged(); OnPropertyChanged(nameof(TheoreticalBalance)); } }

        // Totals for the session
        private decimal _salesTotal;
        public decimal SalesTotal { get => _salesTotal; set { _salesTotal = value; OnPropertyChanged(); OnPropertyChanged(nameof(TheoreticalBalance)); } }

        private decimal _incomesTotal;
        public decimal IncomesTotal { get => _incomesTotal; set { _incomesTotal = value; OnPropertyChanged(); OnPropertyChanged(nameof(TheoreticalBalance)); } }

        private decimal _expensesTotal;
        public decimal ExpensesTotal { get => _expensesTotal; set { _expensesTotal = value; OnPropertyChanged(); OnPropertyChanged(nameof(TheoreticalBalance)); } }

        private decimal _topupsTotal;
        public decimal TopupsTotal { get => _topupsTotal; set { _topupsTotal = value; OnPropertyChanged(); OnPropertyChanged(nameof(TheoreticalBalance)); } }

        private decimal _repairsTotal;
        public decimal RepairsTotal { get => _repairsTotal; set { _repairsTotal = value; OnPropertyChanged(); OnPropertyChanged(nameof(TheoreticalBalance)); } }

        // Desglose por método (Solo ventas/servicios)
        private decimal _cashTotal;
        public decimal CashTotal { get => _cashTotal; set { _cashTotal = value; OnPropertyChanged(); } }

        private decimal _cardTotal;
        public decimal CardTotal { get => _cardTotal; set { _cardTotal = value; OnPropertyChanged(); } }

        private decimal _transferTotal;
        public decimal TransferTotal { get => _transferTotal; set { _transferTotal = value; OnPropertyChanged(); } }

        private decimal _closingBalance;
        public decimal ClosingBalance { get => _closingBalance; set { _closingBalance = value; OnPropertyChanged(); OnPropertyChanged(nameof(Difference)); } }

        public decimal TheoreticalBalance => OpeningBalance + CashTotal + IncomesTotal + TopupsTotal + RepairsTotal - ExpensesTotal;

        public decimal Difference => ClosingBalance - TheoreticalBalance;

        public string Status => IsOpen ? "ABIERTA" : "CERRADA";

        public bool IsTooOld => IsOpen && (DateTime.Now - OpeningTime).TotalHours > 24;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
