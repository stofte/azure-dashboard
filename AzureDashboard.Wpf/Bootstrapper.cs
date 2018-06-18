using AzureDashboard.Services;
using AzureDashboard.Services.Repositories;
using AzureDashboard.Wpf.ViewModels;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AzureDashboard.Wpf
{
    public class Bootstrapper : BootstrapperBase
    {
        private SimpleContainer container = new SimpleContainer();

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            var clientId = ConfigurationManager.AppSettings["ClientId"];
            var clientSecret = ConfigurationManager.AppSettings["ClientSecret"];
            var tenantId = ConfigurationManager.AppSettings["TenantId"];

            var httpClient = new HttpClient();
            var apiClient = new ApiClient(httpClient);
            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();
            container.Instance(new AzureContextService(apiClient));
            container.Instance(new AccountRepository());
            container.Instance(new TenantRepository());
            container.PerRequest<ShellViewModel>();
            container.PerRequest<PageMenuViewModel>();
            container.PerRequest<AccountManagerViewModel>();
            container.PerRequest<DashboardViewModel>();
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            return container.GetInstance(serviceType, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return container.GetAllInstances(serviceType);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }

    // 
    public class RequireDlls
    {
        public MaterialDesignColors.Hue T1 = default(MaterialDesignColors.Hue);
        public MaterialDesignThemes.Wpf.Card T2 = default(MaterialDesignThemes.Wpf.Card);
    }
}
