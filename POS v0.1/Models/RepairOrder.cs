using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace POS_v0_1.Models
{
    public class RepairOrder
    {
        public int Id { get; set; }
        public DateTime EntryDate { get; set; }
        public string ClientName { get; set; }
        public string? ClientPhone { get; set; }
        public string DeviceBrand { get; set; }
        public string DeviceModel { get; set; }
        public string? IMEI { get; set; }
        public string FailureCategory { get; set; }
        public string ReportedFailure { get; set; }
        public string Status { get; set; } = "Recibido"; // Recibido, Entregado
        public decimal Total { get; set; }

        // RBAC Fields
        public int? ReceivedById { get; set; }
        public string? ReceivedByName { get; set; }
        public int? TechnicianId { get; set; }
        public string? TechnicianName { get; set; }

        public virtual ICollection<RepairStatusHistory> StatusHistory { get; set; } = new ObservableCollection<RepairStatusHistory>();
        public virtual ICollection<RepairPart> Parts { get; set; } = new ObservableCollection<RepairPart>();
    }

    public class RepairStatusHistory
    {
        public int Id { get; set; }
        public int RepairOrderId { get; set; }
        public string Status { get; set; }
        public DateTime ChangeDate { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string? Notes { get; set; }
    }
}
