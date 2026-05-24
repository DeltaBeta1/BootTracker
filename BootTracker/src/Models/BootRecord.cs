using System;

namespace BootTracker.Models
{
    public class BootRecord
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Reason { get; set; }
        public string Approver { get; set; }
        public string BootTime { get; set; }
        public string CreatedAt { get; set; }
    }
}
