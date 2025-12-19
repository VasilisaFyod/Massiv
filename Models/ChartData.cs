using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massiv.Models
{
    public class ChartData
    {
        public string Label { get; set; } = string.Empty;
        public int Value { get; set; }
        public double Percentage { get; set; }
        public string Color { get; set; } = "#808080";
    }
}
