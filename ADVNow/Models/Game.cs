using SqlKata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADVNow.Models
{
    internal class Game
    {
        public string Title { get; set; } = "";

        public int Id { get; set; } = -1;

        public string Brand { get; set; } = "";

        public string Path { get; set; } = "";

        public int TotalPlayMinutes { get; set; } = 0;

        [Ignore]
        public string TotalPlayString
        {
            get
            {
                int hour = this.TotalPlayMinutes / 60;
                int min = this.TotalPlayMinutes % 60;
                return String.Format("{0:D2}:{1:D2}", hour, min);
            }
        }

        public DateTime LastPlay { get; set; }

        [Ignore]
        public string LastPlayString {
            get
            {
                return LastPlay.ToString("yyyy年MM月dd日");
            }
        }

        public DateTime SellDay { get; set; }

        [Ignore]
        public string SellDayString
        {
            get
            {
                return this.SellDay.ToString("yyyy年MM月");
            }
        }

    }
}
