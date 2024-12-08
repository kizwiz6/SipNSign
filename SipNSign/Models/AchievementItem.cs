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
    }
}
