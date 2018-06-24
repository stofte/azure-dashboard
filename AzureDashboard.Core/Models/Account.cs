using AzureDashboard.Core.AzureRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Core.Models
{
    public class Account
    {
        public int RowId { get; set; }
        public UserInfo Info { get; set; }
        public string TenantId { get; set; }
        public IEnumerable<Tenant> Tenants { get; set; }
    }
}
