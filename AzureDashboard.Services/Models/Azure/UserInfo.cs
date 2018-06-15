using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Services.Models.Azure
{
    /// <summary>
    /// Relevant parts of Microsoft.IdentityModel.Clients.ActiveDirectory.UserInfo
    /// </summary>
    public class UserInfo
    {
        public string UniqueId { get; set; }
        public string DisplayableId { get; set; }
        public string GivenName { get; }
        public string FamilyName { get; }
        public string IdentityProvider { get; }
    }
}
