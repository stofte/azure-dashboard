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
    public class Database
    {
        class Schema
        {
            public string Name { get; set; }
            public string Sql { get; set; }
        }


        string fileName { get { return @".\db.dat"; } }

        string tenantsSql = @"
CREATE TABLE Tenant(
    Id nvarchar(50) PRIMARY KEY NOT NULL,
    DisplayName nvarchar(200) NOT NULL,
    DefaultVerifiedDomain nvarchar(200) NOT NULL
)";
        string subscriptionsSql = @"
CREATE TABLE Subscription(
    SubscriptionId nvarchar(50) PRIMARY KEY NOT NULL,
    DisplayName nvarchar(200) NOT NULL,
    Tenant nvarchar(50) NOT NULL,
    FOREIGN KEY(Tenant) REFERENCES Tenant(Id)
)";

        string accountSql = @"
CREATE TABLE Account(
    UniqueId nvarchar(50) PRIMARY KEY NOT NULL,
    DisplayableId nvarchar(200) NOT NULL,
    GivenName nvarchar(200),
    FamilyName nvarchar(200),
    TenantId nvarchar(50) NOT NULL
)";

        string tenantAcconutLinkSql = @"
CREATE TABLE TenantAccountLink(
    Tenant nvarchar(50) NOT NULL,
    Account nvarchar(50) NOT NULL,
    UNIQUE(Tenant, Account) ON CONFLICT FAIL,
    FOREIGN KEY(Tenant) REFERENCES Tenant(id),
    FOREIGN KEY(Account) REFERENCES Account(UniqueId)
)";

        SQLiteConnection connection;

        public SQLiteConnection Connection { get { return connection; } }

        public Database Initialize()
        {
            if (!File.Exists(fileName))
            {
                SQLiteConnection.CreateFile(fileName);
            }
            connection = new SQLiteConnection($"Data Source={fileName};Version=3;");
            connection.Open();

            var schemas = GetSchema();
            var subscriptionSchema = schemas.SingleOrDefault(x => x.Name == "Subscription");
            var tenantsSchema = schemas.SingleOrDefault(x => x.Name == "Tenant");
            var accountSchema = schemas.SingleOrDefault(x => x.Name == "Account");
            var linkSchema = schemas.SingleOrDefault(x => x.Name == "TenantAccountLink");
            var schemaOk = subscriptionSchema != null && subscriptionSchema.Sql == subscriptionsSql.Trim() &&
                tenantsSchema != null && tenantsSchema.Sql == tenantsSql.Trim() &&
                accountSchema != null && accountSchema.Sql == accountSql.Trim() &&
                linkSchema != null && linkSchema.Sql == tenantAcconutLinkSql.Trim();
            if (!schemaOk)
            {
                connection.ExecuteNonQuery($"DROP TABLE IF EXISTS TenantAccountLink;");
                connection.ExecuteNonQuery($"DROP TABLE IF EXISTS Account;");
                connection.ExecuteNonQuery($"DROP TABLE IF EXISTS Subscription;");
                connection.ExecuteNonQuery($"DROP TABLE IF EXISTS Tenant;");
                connection.ExecuteNonQuery(tenantsSql);
                connection.ExecuteNonQuery(subscriptionsSql);
                connection.ExecuteNonQuery(accountSql);
                connection.ExecuteNonQuery(tenantAcconutLinkSql);
            }
            return this;
        }

        IEnumerable<Schema> GetSchema()
        {
            var sql = @"
            SELECT name, sql FROM sqlite_master
            WHERE type='table'
            ORDER BY name;
            ";

            var vals = connection.Query<Schema>(sql);
            return vals;
        }
    }
}
