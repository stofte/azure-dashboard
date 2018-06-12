using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services.Models
{
    public class ResourceType
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public bool IsMetricsSupported { get; set; }
    }
}
