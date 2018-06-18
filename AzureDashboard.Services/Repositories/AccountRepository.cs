using AzureDashboard.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services.Repositories
{
    public class AccountRepository : Repository<Account>
    {
        string fileName = @".\accounts.dat";

        public AccountRepository()
        {
            var fileExists = !File.Exists(fileName);
            var createDb = !fileExists;
            createDb = createDb && VerifySchema();
            if (createDb)
            {

            }
        }

        public override async Task Start()
        {
            ;
        }

        public override Account Add()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<Account> All()
        {
            throw new NotImplementedException();
        }

        public override Account Get()
        {
            throw new NotImplementedException();
        }

        public override Account Remove()
        {
            throw new NotImplementedException();
        }

        protected override bool VerifySchema()
        {
            throw new NotImplementedException();
        }
    }
}
