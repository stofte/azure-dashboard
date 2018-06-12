using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services.Models
{
    public class MetricData
    {
        public DateTime TimeStamp { get; set; }
        public float Average { get; set; }
    }
}
