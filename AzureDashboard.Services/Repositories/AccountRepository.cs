using AzureDashboard.Core.Models;
using AzureDashboard.Services.Helpers;
using Dapper;
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
        class AccountSerialized
        {
            public int RowId { get; set; }
            public string UniqueId { get; set; }
            public string DisplayableId { get; set; }
            public string GivenName { get; set; }
            public string FamilyName { get; set; }
            public string TenantId { get; set; }
        }

        class TenantAccountLink
        {
            public string Tenant { get; set; }
            public string Account { get; set; }
        }

        class SubscriptionSerialized
        {
            public string SubscriptionId { get; set; }
            public string DisplayName { get; set; }
            public string Tenant { get; set; }
        }

        public AccountRepository(Database database) : base(database) { }

        public override bool Add(Account account)
        {
            var allCreated = account.Tenants.Select(tenant =>
            {
                var tenantCmd = database.Connection.CreateCommand();
                tenantCmd.CommandText = "insert into Tenant(Id, DisplayName, DefaultVerifiedDomain)" +
                    "values(@id, @displayName, @defaultVerifiedDomain)";
                tenantCmd.Parameters.AddWithValue("@id", tenant.Id);
                tenantCmd.Parameters.AddWithValue("@displayName", tenant.DisplayName);
                tenantCmd.Parameters.AddWithValue("@defaultVerifiedDomain", tenant.DefaultVerifiedDomain);
                return tenantCmd.ExecuteNonQuery() == 1;
            }).All(x => x);

            var allSubsCreated = account.Tenants.Where(x => x.Subscriptions.Count() > 0).Select(x => Tuple.Create(x.Id, x.Subscriptions))
                .Select(x => {
                    var allOk = x.Item2.Select(sub => {
                        var subCmd = database.Connection.CreateCommand();
                        subCmd.CommandText = "insert into Subscription(SubscriptionId, DisplayName, Tenant) " +
                            "values(@subscriptionId, @displayName, @tenant)";
                        subCmd.Parameters.AddWithValue("@subscriptionId", sub.SubscriptionId);
                        subCmd.Parameters.AddWithValue("@displayName", sub.DisplayName);
                        subCmd.Parameters.AddWithValue("@tenant", x.Item1);
                        return subCmd.ExecuteNonQuery() == 1;
                    }).ToList();
                    
                    return allOk.All(b => b);
                }).ToList();
            
            var acctCmd = database.Connection.CreateCommand();
            acctCmd.CommandText = "insert into Account(UniqueId, DisplayableId, GivenName, FamilyName, TenantId)" +
                "values(@uniqueId, @displayableId, @givenName, @familyName, @tenantId)";
            acctCmd.Parameters.AddWithValue("@uniqueId", account.Info.UniqueId);
            acctCmd.Parameters.AddWithValue("@displayableId", account.Info.DisplayableId);
            acctCmd.Parameters.AddWithValue("@givenName", account.Info.GivenName);
            acctCmd.Parameters.AddWithValue("@familyName", account.Info.FamilyName);
            acctCmd.Parameters.AddWithValue("@tenantId", account.TenantId);
            var acctRow = acctCmd.ExecuteNonQuery() == 1;

            var allLinked = account.Tenants.Select(tenant =>
            {
                var tenantCmd = database.Connection.CreateCommand();
                tenantCmd.CommandText = "insert into TenantAccountLink(Tenant, Account) values(@tenant, @account)";
                tenantCmd.Parameters.AddWithValue("@tenant", tenant.Id);
                tenantCmd.Parameters.AddWithValue("@account", account.Info.UniqueId);
                return tenantCmd.ExecuteNonQuery() == 1;
            }).All(x => x);

            return allCreated && acctRow && allLinked;
        }

        public override IEnumerable<Account> All()
        {
            var subs = database.Connection.Query<SubscriptionSerialized>("select * from Subscription");
            var tenants = database.Connection.Query<Tenant>("select * from Tenant");
            foreach(var tenant in tenants)
            {
                var tenantSubs = subs.Where(x => x.Tenant == tenant.Id);
                tenant.Subscriptions = tenantSubs.Select(x => new Core.AzureRM.Models.Subscription
                {
                    SubscriptionId = x.SubscriptionId,
                    DisplayName = x.DisplayName
                });
            }
            var links = database.Connection.Query<TenantAccountLink>("select * from TenantAccountLink");
            var accounts = database.Connection.Query<AccountSerialized>("select * from Account")
                .Select(x =>
                {
                    var accountTenantsIds = links.Where(l => l.Account == x.UniqueId).Select(l => l.Tenant);
                    var accountTenants = tenants.Where(t => accountTenantsIds.Contains(t.Id));
                    // assumed that the if the user isn't a live account, one of the accessible tenants is the home tenant
                    return new Account
                    {
                        RowId =  x.RowId,
                        Info = new Core.AzureRM.Models.UserInfo
                        {
                            UniqueId = x.UniqueId,
                            DisplayableId = x.DisplayableId,
                            FamilyName = x.FamilyName,
                            GivenName = x.GivenName
                        },
                        TenantId = x.TenantId,
                        Tenants = accountTenants
                    };
                });
            return accounts;
        }
        
        public override void Remove(Account account)
        {
            var acc = All().Single(x => x.RowId == account.RowId);
        }
    }
}
