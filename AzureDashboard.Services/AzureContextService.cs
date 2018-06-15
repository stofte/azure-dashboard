using AzureDashboard.Services.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services
{
    public class AzureContextService
    {
        readonly string armBaseUrl = "https://management.azure.com/";
        readonly string authBaseUrl = "https://login.microsoftonline.com/";

        readonly Uri powershellReturnUrl = new Uri("urn:ietf:wg:oauth:2.0:oob");
        readonly string powershellAppId = "1950a258-227b-4e31-a9cf-717495945fc2";

        FileCache fileCache;
        AccountCache accountCache;

        IEnumerable<AzureContext> contexts;

        public AzureContextService()
        {
            contexts = new AzureContext[] { };
        }

        public bool HasContexts { get { return contexts.Count() > 0; } }

        public async Task Start()
        {
            fileCache = new FileCache();
            accountCache = new AccountCache();

            if (ReusableContext())
            {
                await LoadContexts();
            }
        }

        bool ReusableContext()
        {
            return accountCache.GetAccounts().Count() > 0 && fileCache.ReadItems().Count() > 0;
        }

        public async Task AddAccount()
        {
            using (var client = new HttpClient())
            {
                var commonAuth = await AcquireToken("common", PromptBehavior.Auto);
            }
        }

        async Task LoadContexts()
        {
            foreach (var c in fileCache.ReadItems())
            {
                var auth = await AcquireToken(c.TenantId, PromptBehavior.Never);
            }
        }

        async Task<AuthenticationResult> AcquireToken(string tenantId, PromptBehavior promptBehavior)
        {
            var ac = new AuthenticationContext(authBaseUrl + tenantId, fileCache);
            return await ac.AcquireTokenAsync(armBaseUrl, powershellAppId, powershellReturnUrl, new PlatformParameters(promptBehavior));
        }
    }
}
