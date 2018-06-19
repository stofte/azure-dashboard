using AzureDashboard.Services;
using AzureDashboard.Core.Models;
using AzureDashboard.Core.AzureRM.Models;
using AzureDashboard.Wpf.Models;
using Caliburn.Micro;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Wpf.ViewModels
{
    public class AccountManagerViewModel : Screen
    {
        AzureContextService azureContextService;
        public Prop<bool> Visible { get; set; }
        public Prop<bool> AddAccountsIsEnabled { get; set; }
        public Prop<bool> AddAccountsSpinnerIsEnabled { get; set; }
        public Prop<string> AddAccountContent { get; set; }


        readonly string addAccountEnabledContent = "Add account";
        readonly string addAccountDisabledContent = "Adding account";

        public ObservableCollection<Account> Accounts { get; set; }

        public AccountManagerViewModel(AzureContextService azureContextService)
        {
            this.azureContextService = azureContextService;
            Visible = new Prop<bool>();
            AddAccountsIsEnabled = new Prop<bool>(true);
            AddAccountsSpinnerIsEnabled = new Prop<bool>(false);
            AddAccountContent = new Prop<string>(addAccountEnabledContent);
            Accounts = new ObservableCollection<Account>();
        }

        protected override void OnActivate()
        {
            Accounts.Clear();
            foreach(var acc in azureContextService.GetAccounts())
            {
                Accounts.Add(acc);
            }
            base.OnActivate();
        }

        public async Task AddAccount()
        {
            AddAccountsIsEnabled.Value = false;
            AddAccountsSpinnerIsEnabled.Value = true;
            AddAccountContent.Value = addAccountDisabledContent;
            var added = await azureContextService.AddAccount();
            var accs = azureContextService.GetAccounts();
            Accounts.Clear();
            foreach(var acc in accs)
            {
                Accounts.Add(acc);
            }
            AddAccountsIsEnabled.Value = true;
            AddAccountsSpinnerIsEnabled.Value = false;
            AddAccountContent.Value = addAccountEnabledContent;
        }
    }
}
