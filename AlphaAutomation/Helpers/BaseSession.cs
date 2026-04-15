using System;
using System.Diagnostics;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Chrome;

namespace AlphaAutomation.Helpers
{
    public static class BaseSession
    {
        private const string WinAppDriverUrl = "http://127.0.0.1:4723";

        public static Process? WinApp;
        public static WindowsDriver<WindowsElement>? DesktopSession;
        public static WindowsDriver<WindowsElement>? OutlookSession;
        public static IWebDriver? WebSession;

        public static void StartRootDesktop(string winAppDriverPath)
        {
            if (WinApp == null || WinApp.HasExited)
            {
                WinApp = Process.Start(winAppDriverPath);
                Thread.Sleep(500); // REQUIRED
            }

            if (DesktopSession == null)
            {
                var opts = new AppiumOptions();
                opts.AddAdditionalCapability("app", "Root");

                DesktopSession = new WindowsDriver<WindowsElement>(
                    new Uri(WinAppDriverUrl), opts);
            }
        }

        public static void StartWeb()
        {
            if (WebSession != null) return;

            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            // Disable Chrome's Private Network Access / local network prompt which
            // asks "Access other devices on your local network".
            options.AddArgument("--disable-features=BlockInsecurePrivateNetworkRequests,OutOfBlinkCors");

            // If you'd rather use a real Chrome profile that already granted the
            // permission, uncomment and set a path:
            // options.AddArgument($"user-data-dir=C:\\Path\\To\\ChromeProfile");

            WebSession = new ChromeDriver(options);
        }

        public static void AttachToOutlookComposeWindow(TimeSpan timeout)
        {
            if (DesktopSession == null)
                throw new InvalidOperationException("Desktop root session not started.");

            // ✅ WAIT FOR OUTLOOK PROCESS
            var processWait = Stopwatch.StartNew();
            while (processWait.Elapsed < timeout)
            {
                if (Process.GetProcessesByName("OUTLOOK").Length > 0)
                    break;

                Thread.Sleep(500);
            }

            // ✅ WAIT FOR COMPOSE WINDOW
            WindowsElement? compose = null;
            var windowWait = Stopwatch.StartNew();

            while (windowWait.Elapsed < timeout)
            {
                try
                {
                    var windows = DesktopSession.FindElementsByClassName("rctrl_renwnd32");

                    foreach (var win in windows)
                    {
                        var title = (win.GetAttribute("Name") ?? "").Trim();

                        if ((title.Contains("Untitled", StringComparison.OrdinalIgnoreCase)
                             || title.Contains("New Message", StringComparison.OrdinalIgnoreCase)
                             || title.Contains("Message", StringComparison.OrdinalIgnoreCase))
                            && !title.Contains("Inbox"))
                        {
                            compose = win;
                            break;
                        }
                    }

                    if (compose != null)
                        break;
                }
                catch { }

                Thread.Sleep(500);
            }

            if (compose == null)
                throw new TimeoutException("Outlook compose window not found.");

            // ✅ ATTACH SESSION
            string handleHex = int.Parse(
                compose.GetAttribute("NativeWindowHandle")).ToString("x");

            var caps = new AppiumOptions();
            caps.AddAdditionalCapability("appTopLevelWindow", handleHex);
            caps.AddAdditionalCapability("deviceName", "Windows");

            OutlookSession = new WindowsDriver<WindowsElement>(
                new Uri(WinAppDriverUrl), caps);

            OutlookSession.Manage().Timeouts()
                .ImplicitWait = TimeSpan.FromSeconds(10);
        }
        public static void ClickfileCabinetButton()
        {
            if (WebSession == null) return;
            try
            {
                var fileCabinet = WebSession.FindElement(By.Id("lnkFileCabinet"));
                if (fileCabinet.Displayed && fileCabinet.Enabled)
                    fileCabinet.Click();

            }
            catch { }
        }
        public static void QuitAll()
        {
            try { OutlookSession?.Quit(); } catch { }
            try { DesktopSession?.Quit(); } catch { }
            //try { WebSession?.Quit(); } catch { }

            try
            {
                if (WinApp != null && !WinApp.HasExited)
                    WinApp.Kill();
            }
            catch { }
        }
    }
}