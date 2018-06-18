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
        public UserInfo Info { get; set; }
        public Tenant HomeTenant { get; set; }
        public IEnumerable<Tenant> Tenants { get; set; }
    }
}
