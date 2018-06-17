using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Wpf.Models
{
    public class MenuOption
    {
        public string Name { get; set; }
        public string Icon
        {
            get
            {
                switch (Kind)
                {
                    case MenuOptionKinds.Accounts: 
                        return "AccountMultiple";
                    case MenuOptionKinds.Dashboard:
                        return "Apps";
                    default:
                        throw new ArgumentException("unknown kinds");
                }
            }
        }
        public MenuOptionKinds Kind { get; set; }
    }
}
