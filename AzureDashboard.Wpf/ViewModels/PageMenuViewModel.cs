using AzureDashboard.Wpf.Models;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDashboard.Wpf.ViewModels
{
    public class PageMenuViewModel : Screen
    {
        public Prop<MenuOption> SelectedMenu { get; set; }

        public ObservableCollection<MenuOption> MenuOptions { get; set; }

        public PageMenuViewModel()
        {
            var menuOpts = new MenuOption[]
            {
                new MenuOption
                {
                    Name = "Dashboard",
                    Kind = MenuOptionKinds.Dashboard
                },
                new MenuOption
                {
                    Name = "Accounts",
                    Kind = MenuOptionKinds.Accounts
                },
            };

            SelectedMenu = new Prop<MenuOption>(menuOpts[0]);
            MenuOptions = new ObservableCollection<MenuOption>();
            MenuOptions.Add(menuOpts[0]);
            MenuOptions.Add(menuOpts[1]);
        }
    }
}
