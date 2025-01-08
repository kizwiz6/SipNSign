using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.kizwiz.sipnsign.Models
{
    public class AchievementItem
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsUnlocked { get; set; }
        public double Progress { get; set; } // 0.0 to 1.0
        public string ProgressText { get; set; } // e.g., "5/10"
        public DateTime? UnlockedDate { get; set; }
    }
}
