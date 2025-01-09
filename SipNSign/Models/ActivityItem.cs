using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.kizwiz.sipnsign.Models
{
    public class ActivityItem
    {
        public required string Icon { get; set; }
        public required string Description { get; set; }
        public required string TimeAgo { get; set; }
        public required string Score { get; set; }
    }
}
