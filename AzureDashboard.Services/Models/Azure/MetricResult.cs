using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services.Models.Azure
{
    public class MetricResult
    {
        public IEnumerable<DimensionValue> MetadataValues { get; set; }
        public IEnumerable<MetricData> Data { get; set; }
    }
}
