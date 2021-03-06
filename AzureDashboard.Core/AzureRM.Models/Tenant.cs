﻿using AzureDashboard.Core.AzureRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Core.AzureRM.Models
{
    public class Tenant
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public IEnumerable<VerifiedDomain> VerifiedDomains { get; set; }
    }
}
