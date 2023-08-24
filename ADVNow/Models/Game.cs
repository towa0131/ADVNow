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

        public string Brand { get; set; } = "";

        public string Path { get; set; } = "";

        public int TotalPlayMinutes { get; set; }

        public string TotalPlayString
        {
            get
            {
                int hour = this.TotalPlayMinutes / 60;
                int min = this.TotalPlayMinutes % 60;
                return String.Format("{0:D2}:{1:D2}", hour, min);
            }
        }

        public string LastPlay { get; set; }

        public string SellDay { get; set; }

    }
}
