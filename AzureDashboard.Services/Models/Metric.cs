using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services.Models
{
    public class Metric
    {
        public string Id { get; set; }
        public bool IsDimensionRequired { get; set; }
        public string Unit { get; set; }
        public Name Name { get; set; }
        public string DisplayName { get; set; }

        public string PrimaryAggregationType { get; set; }

        public IEnumerable<string> SupportedAggregationTypes { get; set; }

        public IEnumerable<MetricAvailability> MetricAvailabilities { get; set; }

        public IEnumerable<MetricDimension> Dimensions { get; set; }
        
        public class MetricAvailability
        {
            public string TimeGrain { get; set; }
            public string Retention { get; set; }
        }

        public class MetricDimension
        {
            public string Value { get; set; }
            public string LocalizedValue { get; set; }
        }
    }
}
