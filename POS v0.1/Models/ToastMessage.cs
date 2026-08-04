using System;

namespace POS_v0_1.Models
{
    public class ToastMessage
    {
        public string Message { get; set; }
        public string Type { get; set; } // Success, Info, Warning, Error
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
