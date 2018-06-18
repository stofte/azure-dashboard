using AzureDashboard.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services.Repositories
{
    public class TenantRepository : Repository<Tenant>
    {
        public override async Task Start()
        {
            ;
        }

        public override Tenant Add()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Tenant> All()
        {
            throw new NotImplementedException();
        }

        public override Tenant Get()
        {
            throw new NotImplementedException();
        }

        public override Tenant Remove()
        {
            throw new NotImplementedException();
        }

        protected override bool VerifySchema()
        {
            throw new NotImplementedException();
        }
    }
}
