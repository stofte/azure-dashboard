using AzureDashboard.Services.Models.Azure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services
{
    public class AzureUrlService
    {
        readonly string armBaseUrl = "https://management.azure.com/";
        readonly string authBaseUrl = "https://login.microsoftonline.com/";

        readonly Uri powershellReturnUrl = new Uri("urn:ietf:wg:oauth:2.0:oob");
        readonly string powershellAppId = "1950a258-227b-4e31-a9cf-717495945fc2";

        public string OrganizationQueryUrl(TenantIdentifier tenant)
        {
            return "https://graph.microsoft.com/v1.0/{tenant.TenantId}/organization/";
        }

        public string AuthenticationUrl(TenantIdentifier tenant)
        {
            return $"{authBaseUrl}/{tenant.TenantId}";
        }
    }
}
