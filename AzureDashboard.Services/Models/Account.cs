using AzureDashboard.Services.Models.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services.Models
{
    public class Account
    {
        public UserInfo Info { get; set; }
        public IEnumerable<Tenant> Tenants { get; set; }
    }
}
