using AzureDashboard.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services
{
    public class AccountCache
    {
        string filePath = @".\accounts.dat";
        IEnumerable<Account> accounts;

        public AccountCache()
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                accounts = JsonConvert.DeserializeObject<IEnumerable<Account>>(json);
            }
            else
            {
                accounts = new Account[] { };
            }
        }

        public IEnumerable<Account> GetAccounts()
        {
            return accounts;
        }
    }
}
