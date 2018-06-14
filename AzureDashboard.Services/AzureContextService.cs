using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services
{
    public class AzureContextService
    {
        FileCache fileCache;

        public AzureContextService()
        {
            
        }

        public async Task Login()
        {
            fileCache = new FileCache();
        }
    }
}
