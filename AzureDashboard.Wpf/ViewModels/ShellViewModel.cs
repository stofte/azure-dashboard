using AzureDashboard.Services;
using AzureDashboard.Wpf.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Wpf.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        AzureContextService azureContextService;

        public Prop<AccountManagerViewModel> AccountManagerView { get; set; }

        public ShellViewModel(AzureContextService azureContextService)
        {
            this.azureContextService = azureContextService;
            AccountManagerView = new Prop<AccountManagerViewModel>();
        }

        public void AddAccount()
        {

        }

        protected override async void OnInitialize()
        {
            await azureContextService.Start();
            var model = IoC.Get<AccountManagerViewModel>();
            model.Visible.Value = !azureContextService.HasContexts;
            AccountManagerView.Value = model;
            ActivateItem(model);
            base.OnInitialize();
        }
    }
}
