using AzureDashboard.Services.Helpers;
using AzureDashboard.Services.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services
{
    public class AzureContextService
    {
        readonly string armBaseUrl = "https://management.azure.com/";
        readonly string authBaseUrl = "https://login.microsoftonline.com/";
        readonly string graphBaseUrl = "https://graph.microsoft.com";

        readonly Uri powershellReturnUrl = new Uri("urn:ietf:wg:oauth:2.0:oob");
        readonly string powershellAppId = "1950a258-227b-4e31-a9cf-717495945fc2";

        FileCache fileCache;
        AccountCache accountCache;
        ApiClient api;
        List<AzureAccessToken> contexts;
        List<Account> accounts;

        public AzureContextService(ApiClient client)
        {
            contexts = new List<AzureAccessToken>();
            accounts = new List<Account>();
            this.api = client;
        }

        public bool HasContexts { get { return contexts.Count() > 0; } }

        public async Task Start()
        {
            ConfigureConnection();
            fileCache = new FileCache();
            accountCache = new AccountCache();
            if (ReusableContext())
            {
                await LoadContexts().ConfigureAwait(false);
            }
        }

        public async Task<AzureAccessToken> Context(Tenant tenant)
        {
            return null;
        }

        bool ReusableContext()
        {
            return accountCache.GetAccounts().Count() > 0 && fileCache.ReadItems().Count() > 0;
        }

        public async Task<bool> AddAccount()
        {
            try
            {
                // http://www.cloudidentity.com/blog/2014/08/26/the-common-endpoint-walks-like-a-tenant-talks-like-a-tenant-but-is-not-a-tenant/
                // https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-devhowto-multi-tenant-overview
                var authResponse = await AcquireToken("common", PromptBehavior.SelectAccount).ConfigureAwait(false);
                // the auth points to the users selected tenant and the access token is valid for that only
                var accountTenant = authResponse.Authority;
                var userAuth = await AcquireToken(accountTenant, PromptBehavior.Auto).ConfigureAwait(false);
                var userCtx = AddContext(userAuth);
                // the tenant knowns which other tenants the user is linked to, so query for that
                var tenantIds = await api.AvailableTenants(userCtx);
                var tenants = new List<Tenant>();
                foreach(var tenantId in tenantIds)
                {
                    // do separate graph call to obtain the displayname itself
                    var auth = await AcquireToken(tenantId.TenantId, PromptBehavior.Auto, useGraphAuth: true)
                        .ConfigureAwait(false);
                    var ctx = AddContext(auth);
                    var tenant = await api.Tenant(ctx);
                    tenants.Add(tenant);
                }
                accounts.Add(new Account
                {
                    Info = new Models.Azure.UserInfo
                    {
                        DisplayableId = userAuth.UserInfo.DisplayableId,
                        UniqueId = userAuth.UserInfo.UniqueId
                    },
                    Tenants = tenants
                });
                return true;
            }
            catch (AdalServiceException) // assume user cancelled login
            {
                return false;
            }
        }

        public async Task<IEnumerable<Account>> GetAccounts()
        {
            return accounts;
        }

        AzureAccessToken AddContext(AuthenticationResult authentication)
        {
            var ctx = new AzureAccessToken
            {
                TenantId = authentication.TenantId,
                Value = authentication.AccessToken
            };
            contexts.Add(ctx);
            return ctx;
        }

        async Task LoadContexts()
        {
            foreach (var c in fileCache.ReadItems())
            {
                var auth = await AcquireToken(c.TenantId, PromptBehavior.Never).ConfigureAwait(false);
            }
        }

        async Task<AuthenticationResult> AcquireToken(string tenantId, PromptBehavior promptBehavior, bool useGraphAuth = false)
        {
            var authUrl = authBaseUrl + tenantId;
            if (Uri.IsWellFormedUriString(tenantId, UriKind.Absolute))
            {
                authUrl = tenantId;
            }
            var ac = new AuthenticationContext(authUrl, fileCache);
            var tokenUrl = useGraphAuth ? graphBaseUrl : armBaseUrl;
            return await ac.AcquireTokenAsync(tokenUrl, powershellAppId, powershellReturnUrl, new PlatformParameters(promptBehavior)).ConfigureAwait(false);
        }

        string GetTenantId(string authority)
        {

            return authority;
        }

        void QueryUserTenant()
        {

        }
        
        void ConfigureConnection()
        {
            ServicePointManager.DefaultConnectionLimit = 12;
            // http://byterot.blogspot.com/2016/07/singleton-httpclient-dns.html
            var armSp = ServicePointManager.FindServicePoint(new Uri(armBaseUrl));
            var authSp = ServicePointManager.FindServicePoint(new Uri(authBaseUrl));
            authSp.ConnectionLeaseTimeout = armSp.ConnectionLeaseTimeout = 60 * 1000;
        }
    }
}
