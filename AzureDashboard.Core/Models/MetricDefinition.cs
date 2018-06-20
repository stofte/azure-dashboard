using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Core.Models
{
    public class MetricDefinition
    {
        public string SubscriptionId { get; set; }
        public string ResourceID { get; set; }
        public IEnumerable<string> TimeGrains { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Dimensions { get; set; }
        public IEnumerable<KeyValuePair<string, string>> DimensionValue { get; set; }
    }
}
