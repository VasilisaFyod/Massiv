using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massiv.Models
{
    public class OrderStatistics
    {
        public int TotalOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int ActiveOrders { get; set; }
        public int OverdueOrders { get; set; }
        public int OnTimeOrders { get; set; }
        public decimal OnTimePercentage { get; set; }
        public List<ChartData> StatusDistribution { get; set; } = new();
        public List<ChartData> MonthlyTrend { get; set; } = new();
    }
}
