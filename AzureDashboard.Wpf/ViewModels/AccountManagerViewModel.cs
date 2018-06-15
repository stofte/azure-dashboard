using AzureDashboard.Wpf.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Wpf.ViewModels
{
    public class AccountManagerViewModel : Screen
    {
        public Prop<bool> Visible { get; set; }

        public AccountManagerViewModel()
        {
            Visible = new Prop<bool>();
        }
    }
}
