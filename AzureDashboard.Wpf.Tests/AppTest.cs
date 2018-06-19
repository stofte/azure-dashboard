using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using FlaUI.Core;
using FlaUI.UIA3;
using Xunit;

namespace AzureDashboard.Wpf.Tests
{
    public class AppTest
    {
        [Fact]
        public void TestDemo()
        {
            var appCodebase = typeof(AzureDashboard.Wpf.Bootstrapper).Assembly.CodeBase;
            var workPath = Path.GetDirectoryName(new Uri(appCodebase).AbsolutePath);
            var procInfo = new ProcessStartInfo
            {
                FileName = "AzureDashboard.Wpf.exe",
                WorkingDirectory = workPath
            };
            
            var app = Application.Launch(procInfo);
            using (var automation = new UIA3Automation())
            {
                var window = app.GetMainWindow(automation, waitTimeout: TimeSpan.FromSeconds(10));
                var zz = window.FindFirstDescendant(x => x.ByClassName("PageMenuView")).FindAllDescendants();
                var z = zz[1].AsListBoxItem();
                var accountsMenuOption = window.FindFirstDescendant(x => x.ByClassName("PageMenuView"))
                    .FindAllDescendants(x => x.ByText("AccountMultiple"));
                accountsMenuOption[0].Click();

                var btn = window.FindFirstChild(x => x.ByName("PageMenu"));
                Assert.Equal("ShellView", window.Title);
            }
            app.Close();
        }
    }
}
