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


        protected override async void OnInitialize()
        {
            var model = IoC.Get<AccountManagerViewModel>();
            AccountManagerView.Value = model;
            base.OnInitialize();
        }

        protected override async void OnViewReady(object view)
        {
            await azureContextService.Start();
            AccountManagerView.Value.Visible.Value = !azureContextService.HasContexts;
            ActivateItem(AccountManagerView.Value);
            base.OnViewReady(view);
        }
    }
}
