using System;
using System.Collections.Generic;

namespace POS_v0_1.Models
{
    public class BusinessSummary
    {
        public decimal GrossSales { get; set; }
        public decimal RealOperatingProfit { get; set; }
        public decimal TopupFlow { get; set; }
        public decimal DeliveredRepairsTotal { get; set; }

        public decimal PosSalesIncome { get; set; }
        public decimal TopupIncome { get; set; }
        public decimal RepairIncome { get; set; }
        public decimal OtherExtraIncome { get; set; }
        public decimal TotalGrossIncome => PosSalesIncome + TopupIncome + RepairIncome + OtherExtraIncome;

        public decimal ExpensesAndOutflows { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal TotalOutflows => ExpensesAndOutflows + CostOfGoodsSold;

        public decimal TotalNetProfit => TotalGrossIncome - TotalOutflows;
        public decimal ActualCashInHand { get; set; }
    }

    public class ReportPeriod
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
