using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Core.Models
{
    public class AzureAccessToken
    {
        public string TenantId { get; set; }

        public string Value { get; set; }

        public DateTime ValidTo { get; set; }

        public void Refresh()
        {

        }
    }
}
