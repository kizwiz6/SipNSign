using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.kizwiz.sipnsign.Models
{
    public class ActivityLog
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string IconName { get; set; }
        public DateTime Timestamp { get; set; }
        public string Score { get; set; }
        public ActivityType Type { get; set; }
    }
}
