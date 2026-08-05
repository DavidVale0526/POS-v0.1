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
        public decimal PosCashIncome { get; set; }
        public decimal PosCardIncome { get; set; }
        public decimal PosTransferIncome { get; set; }

        public decimal TopupIncome { get; set; }
        public decimal RepairIncome { get; set; }
        public decimal OtherExtraIncome { get; set; }
        public decimal TotalGrossIncome => PosSalesIncome + TopupIncome + RepairIncome + OtherExtraIncome;

        public decimal ExpensesAndOutflows { get; set; }
        public decimal CostOfGoodsSold { get; set; }
        public decimal TotalOutflows => ExpensesAndOutflows + CostOfGoodsSold;

        public decimal TotalNetProfit => TotalGrossIncome - TotalOutflows;
        public decimal ActualCashInHand { get; set; }
        public bool IsSessionOpen { get; set; }
        public string CashTitleText { get; set; } = "Efectivo en Caja (Real)";
        public string CashStatusText { get; set; } = "Dinero físico que debe haber";

        // Nuevos campos para pestañas detalladas
        public List<FinancialSourceSummary> FinancialSummaries { get; set; } = new List<FinancialSourceSummary>();
        public InventoryValuation Inventory { get; set; } = new InventoryValuation();
        public List<RankedItem> TopProducts { get; set; } = new List<RankedItem>();
        public List<RankedItem> TopClients { get; set; } = new List<RankedItem>();
        public List<RankedItem> SalesBySeller { get; set; } = new List<RankedItem>();
        public List<ServiceReportItem> TopupReport { get; set; } = new List<ServiceReportItem>();
        public List<ServiceReportItem> RepairsByTechnician { get; set; } = new List<ServiceReportItem>();
        public List<ServiceReportItem> RepairsByReceiver { get; set; } = new List<ServiceReportItem>();

        // Phase 4: Visual Intelligence
        public List<DailySalesSummary> DailySalesHistory { get; set; } = new List<DailySalesSummary>();
        public List<RankedItem> TopCategories { get; set; } = new List<RankedItem>();
    }

    public class DailySalesSummary
    {
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public string DayName => Date.ToString("ddd", new System.Globalization.CultureInfo("es-NI"));
    }

    public class FinancialSourceSummary
    {
        public string Source { get; set; }
        public string Type { get; set; }
        public decimal Total { get; set; }
    }

    public class InventoryValuation
    {
        public decimal InvestmentCost { get; set; }
        public decimal ProjectedSalesValue { get; set; }
        public decimal PotentialGrossProfit => ProjectedSalesValue - InvestmentCost;
    }

    public class RankedItem
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal TotalSales { get; set; }
    }

    public class ServiceReportItem
    {
        public string Operator { get; set; }
        public int TicketCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalProfit { get; set; }
    }

    public class ReportPeriod
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
