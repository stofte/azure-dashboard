using AzureDashboard.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AzureDashboard.Wpf.Models
{
    public class SubscriptionsDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement elemnt = container as FrameworkElement;
            var tenant = item as Tenant;
            if (tenant.Subscriptions != null && tenant.Subscriptions.Count() > 0)
            {
                return elemnt.FindResource("TenantSubscriptionDataTemplate") as DataTemplate;
            }
            else
            {
                return elemnt.FindResource("TenantEmptySubscriptionDataTemplate") as DataTemplate;
            }
        }
    }
}
