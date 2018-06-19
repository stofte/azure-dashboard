using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Core.AzureRM.Models
{
    /// <summary>
    /// Relevant parts of Microsoft.IdentityModel.Clients.ActiveDirectory.UserInfo
    /// </summary>
    public class UserInfo
    {
        public string UniqueId { get; set; }
        public string DisplayableId { get; set; }
        public string GivenName { get; set; }
        public string FamilyName { get; set; }
    }
}
