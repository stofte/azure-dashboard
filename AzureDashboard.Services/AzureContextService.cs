using AzureDashboard.Services.Helpers;
using AzureDashboard.Core.Models;
using AzureDashboard.Core.AzureRM.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AzureDashboard.Services.Repositories;

namespace AzureDashboard.Services
{
    public class AzureContextService
    {
        readonly string armBaseUrl = "https://management.azure.com/";
        readonly string authBaseUrl = "https://login.microsoftonline.com/";
        readonly string graphBaseUrl = "https://graph.microsoft.com";

        public static readonly Tenant LiveDotComTenant = new Tenant
        {
            Id = "f8cdef31-a31e-4b4a-93e4-5f571e91255a",
            DisplayName = "live.com"
        };

        readonly Uri powershellReturnUrl = new Uri("urn:ietf:wg:oauth:2.0:oob");
        readonly string powershellAppId = "1950a258-227b-4e31-a9cf-717495945fc2";
        // pretty sure this is correct, though there's little/no docs on this.
        // the live.com tenant doesnt support graph api calls for the org,
        // so this is hardcoded here
        readonly string liveDotComTenantId = "f8cdef31-a31e-4b4a-93e4-5f571e91255a";

        AccountRepository accountRepository;
        FileCache fileCache;
        ApiClient api;
        List<AzureAccessToken> contexts;
        List<Account> accounts;

        public AzureContextService(ApiClient client, AccountRepository accountRepository)
        {
            contexts = new List<AzureAccessToken>();
            accounts = new List<Account>();
            this.api = client;
            this.accountRepository = accountRepository;
        }

        public bool HasContexts { get { return contexts.Count() > 0; } }

        public async Task Start()
        {
            ConfigureConnection();
            fileCache = new FileCache();
            await LoadContexts().ConfigureAwait(false);
        }

        public async Task<AzureAccessToken> Context(Tenant tenant)
        {
            return null;
        }

        public async Task<bool> AddAccount()
        {
            try
            {
                // http://www.cloudidentity.com/blog/2014/08/26/the-common-endpoint-walks-like-a-tenant-talks-like-a-tenant-but-is-not-a-tenant/
                // https://docs.microsoft.com/en-us/azure/active-directory/develop/active-directory-devhowto-multi-tenant-overview
                // use common auth to allow the user to login to any account. the auth response is valid for the selected account only
                var userAuth = await AcquireToken("common", PromptBehavior.SelectAccount).ConfigureAwait(false);
                // the auth points to the accounts home tenant and the access token is valid for that only
                var accountTenantId = GetTenantId(userAuth.Authority);
                // check if we know this account already
                var currAccount = accountRepository.All().SingleOrDefault(x => x.TenantId == accountTenantId
                    && x.Info.UniqueId == userAuth.UserInfo.UniqueId);
                if (currAccount != null)
                {
                    return false;
                }
                var userCtx = AddContext(userAuth);
                // the tenant knowns which other tenants the user is linked to, so query for that
                var tenantIds = await api.AvailableTenants(userCtx);
                var tenants = new List<Tenant>();
                foreach(var tenantId in tenantIds)
                {
                    // do separate graph call to obtain the displayname itself.
                    // adal seems confused when using graph api, so here we specify userId,
                    // otherwise when we obtain the token we might obtain it for the wrong account context.
                    var graphAuth = await AcquireToken(tenantId.TenantId, PromptBehavior.Never, userUniqueId: userAuth.UserInfo.UniqueId, useGraphAuth: true)
                        .ConfigureAwait(false);
                    // pass in a one time token, we persist the values returned from the ms graph call
                    try
                    {
                        var tenant = await api.Tenant(new AzureAccessToken
                        {
                            TenantId = graphAuth.TenantId,
                            Value = graphAuth.AccessToken
                        });
                        var auth = await AcquireToken(tenantId.TenantId, PromptBehavior.Never).ConfigureAwait(false);
                        var subs = await api.Subscriptions(new AzureAccessToken
                        {
                            TenantId = tenantId.TenantId,
                            Value = auth.AccessToken,
                        });
                        tenant.Subscriptions = subs;
                        tenants.Add(tenant);
                    }
                    catch (UnauthorizedAccessException) { }
                }

                var newAccount = new Account
                {
                    Info = new Core.AzureRM.Models.UserInfo
                    {
                        DisplayableId = userAuth.UserInfo.DisplayableId,
                        UniqueId = userAuth.UserInfo.UniqueId,
                        GivenName = userAuth.UserInfo.GivenName,
                        FamilyName = userAuth.UserInfo.FamilyName
                    },
                    TenantId = accountTenantId,
                    Tenants = tenants
                };
                var added = accountRepository.Add(newAccount);
                if (added)
                {
                    accounts.Add(newAccount);
                }
                return added;
            }
            catch (AdalServiceException) // assume user cancelled login
            {
                return false;
            }
        }

        public IEnumerable<Account> GetAccounts()
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
            var accts = accountRepository.All();
            var fileCached = fileCache.ReadItems();
            foreach(var acc in accts)
            {
                var homeToken = fileCached.Where(x => GetTenantId(x.Authority) == acc.TenantId);
                if (homeToken != null)
                {
                    await AcquireToken(acc.TenantId, PromptBehavior.Never).ConfigureAwait(false);
                    foreach(var t in acc.Tenants)
                    {
                        await AcquireToken(acc.TenantId, PromptBehavior.Never).ConfigureAwait(false);
                    }
                }
                accounts.Add(acc);
            }
        }

        async Task<AuthenticationResult> AcquireToken(string tenantId, PromptBehavior promptBehavior, string userUniqueId = null, bool useGraphAuth = false)
        {
            var authUrl = authBaseUrl + tenantId;
            if (Uri.IsWellFormedUriString(tenantId, UriKind.Absolute))
            {
                authUrl = tenantId;
            }
            var ac = new AuthenticationContext(authUrl, fileCache);
            var tokenUrl = useGraphAuth ? graphBaseUrl : armBaseUrl;

            AuthenticationResult result = null;
            if (!string.IsNullOrWhiteSpace(userUniqueId))
            {
                result = await ac.AcquireTokenAsync(tokenUrl, 
                    powershellAppId, powershellReturnUrl, 
                    new PlatformParameters(promptBehavior),
                    new UserIdentifier(userUniqueId, UserIdentifierType.UniqueId)).ConfigureAwait(false);
            }
            else
            {
                result = await ac.AcquireTokenAsync(tokenUrl, powershellAppId, 
                    powershellReturnUrl, new PlatformParameters(promptBehavior)).ConfigureAwait(false);
            }

            return result;
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

        string GetTenantId(string tenant)
        {
            if (Uri.IsWellFormedUriString(tenant, UriKind.Absolute))
            {
                var uri = new Uri(tenant);
                var pq = uri.PathAndQuery;
                // assumes urly value: "/tenantid/"
                return pq.Substring(1, pq.Length - 2);
            }
            return tenant;
        }
    }
}
