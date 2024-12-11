using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.kizwiz.sipnsign.Models
{
    public class UserProgress
    {
        public int SignsLearned { get; set; }
        public int GuessModeSigns { get; set; }
        public int PerformModeSigns { get; set; }
        public int CurrentStreak { get; set; }
        public int CorrectInARow { get; set; }
        public double Accuracy { get; set; }
        public int TotalAttempts { get; set; }
        public int CorrectAttempts { get; set; }
        public TimeSpan TotalPracticeTime { get; set; }
        public List<Achievement> Achievements { get; set; }
        public List<ActivityLog> Activities { get; set; }
    }
}
