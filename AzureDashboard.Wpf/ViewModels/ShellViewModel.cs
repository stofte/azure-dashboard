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

        public Prop<DashboardViewModel> DashboardView { get; set; }

        public Prop<PageMenuViewModel> PageMenuView { get; set; }

        public ShellViewModel(AzureContextService azureContextService)
        {
            this.azureContextService = azureContextService;
            AccountManagerView = new Prop<AccountManagerViewModel>();
            DashboardView = new Prop<DashboardViewModel>();
            PageMenuView = new Prop<PageMenuViewModel>();
        }


        protected override async void OnInitialize()
        {
            AccountManagerView.Value = IoC.Get<AccountManagerViewModel>();
            DashboardView.Value = IoC.Get<DashboardViewModel>();
            PageMenuView.Value = IoC.Get<PageMenuViewModel>();
            PageMenuView.Value.SelectedMenu.PropertyChanged += SelectedMenu_PropertyChanged;
            base.OnInitialize();
        }

        private void SelectedMenu_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var menu = sender as Prop<MenuOption>;
            SetPage(menu.Value.Kind);
        }

        protected override async void OnViewReady(object view)
        {
            await azureContextService.Start();
            // check what menu item is selected in the pagemenu
            SetPage(PageMenuView.Value.SelectedMenu.Value.Kind);
            base.OnViewReady(view);
        }

        void SetPage(MenuOptionKinds kind)
        {
            switch (kind)
            {
                case MenuOptionKinds.Dashboard:
                    ActivateItem(DashboardView.Value);
                    break;
                case MenuOptionKinds.Accounts:
                    ActivateItem(AccountManagerView.Value);
                    break;
                default:
                    throw new ArgumentException("Unknown menu kind");
            }
        }
    }
}
