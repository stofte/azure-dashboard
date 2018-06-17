using AzureDashboard.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services
{
    public class AzureClientProvider
    {
        AzureContextService azureContextService;
        ApiClient[] clients = new ApiClient[100];

        public AzureClientProvider(AzureContextService azureContextService)
        {
            this.azureContextService = azureContextService;
        }

        public ApiClient GetClient(Tenant tenant)
        {
            ApiClient client = null;
            for(var i = 0; i < clients.Length; i++)
            {
                if (clients[i].TenantId == tenant.Id)
                {
                    client = clients[i];
                    break;
                }
            }
            if (client == null)
            {
                var ctx = azureContextService.Context(tenant);
            }
            return client;
        }
    }
}
