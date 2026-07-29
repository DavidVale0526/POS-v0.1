using System;

namespace POS_v0_1.Models
{
    public class RepairOrder
    {
        public int Id { get; set; }
        public DateTime EntryDate { get; set; }
        public string ClientName { get; set; }
        public string ClientPhone { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceModel { get; set; }
        public string IMEI { get; set; }
        public string FailureCategory { get; set; }
        public string ReportedFailure { get; set; }
        public string Status { get; set; } = "Recibido"; // Recibido, Entregado
        public decimal Total { get; set; }
    }
}
