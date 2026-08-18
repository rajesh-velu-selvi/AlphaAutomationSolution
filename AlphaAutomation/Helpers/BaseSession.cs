using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Chrome;
using System;
using System.Diagnostics;
using System.Threading;
using System.IO;

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

            // Prefer a dedicated profile to keep state, but starting Chrome with a locked profile can crash.
            options.AddArgument(@"user-data-dir=C:\AutomationChromeProfile");

            try
            {
                WebSession = new ChromeDriver(options);
            }
            catch (Exception)
            {
                // Fallback: try without the custom profile and with safer flags
                try
                {
                    var fallback = new ChromeOptions();
                    fallback.AddArgument("--no-sandbox");
                    fallback.AddArgument("--disable-dev-shm-usage");
                    fallback.AddArgument("--disable-gpu");
                    WebSession = new ChromeDriver(fallback);
                }
                catch (Exception ex)
                {
                    // Rethrow the original failure to preserve context
                    throw new InvalidOperationException("Failed to start Chrome WebSession.", ex);
                }
            }

            WebSession.Manage().Window.Maximize();
            WebSession.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }
        /// <summary>
        /// Launches Outlook if it isn't already running, and waits until its main window
        /// is ready in the UI Automation tree.
        /// </summary>
        /// <param name="timeout">Max time to wait for the process to start and the main window to appear.</param>
        public static void LaunchOutlook(TimeSpan timeout)
        {
            if (Process.GetProcessesByName("OUTLOOK").Length > 0)
                return; // already running, nothing to do

            string? outlookPath = GetOutlookExecutablePath();

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = !string.IsNullOrEmpty(outlookPath) ? outlookPath : "OUTLOOK.EXE",
                    UseShellExecute = true
                };

                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to launch Outlook.", ex);
            }

            // ✅ Wait for the process to exist
            var processWait = Stopwatch.StartNew();
            while (processWait.Elapsed < timeout)
            {
                if (Process.GetProcessesByName("OUTLOOK").Length > 0)
                    break;

                Thread.Sleep(500);
            }

            if (Process.GetProcessesByName("OUTLOOK").Length == 0)
                throw new TimeoutException("Outlook process did not start within the given timeout.");

            // ✅ Wait for the main window (Inbox) to be ready in the UI tree
            if (DesktopSession == null)
                throw new InvalidOperationException("Desktop root session not started; cannot verify Outlook main window.");

            bool mainWindowReady = false;
            var windowWait = Stopwatch.StartNew();

            while (windowWait.Elapsed < timeout)
            {
                try
                {
                    var windows = DesktopSession.FindElementsByClassName("rctrl_renwnd32");

                    foreach (var w in windows)
                    {
                        string title;
                        try
                        {
                            title = w.GetAttribute("Name") ?? "";
                        }
                        catch
                        {
                            continue;
                        }

                        if (title.Contains("Inbox", StringComparison.OrdinalIgnoreCase)
                            || title.Contains("Outlook", StringComparison.OrdinalIgnoreCase))
                        {
                            mainWindowReady = true;
                            break;
                        }
                    }
                }
                catch
                {
                    // UI tree not ready yet; keep polling
                }

                if (mainWindowReady)
                    break;

                Thread.Sleep(500);
            }

            if (!mainWindowReady)
                throw new TimeoutException("Outlook main window did not become ready within the given timeout.");
        }

        private static string? GetOutlookExecutablePath()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"C:\Program Files\Microsoft Office\root\Office16\OUTLOOK.EXE");
                return key?.GetValue(null) as string;
            }
            catch
            {
                return null;
            }
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
                Thread.Sleep(1000);
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

            OutlookSession.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

              
        public static void AttachToOutlookMainWindow(string subject, TimeSpan timeout)
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
            // ✅ WAIT FOR MAIN WINDOW
            WindowsElement? mainWindow = null;
            var windowWait = Stopwatch.StartNew();
            while (windowWait.Elapsed < timeout)
            {
                try
                {
                    var windows = DesktopSession.FindElementsByClassName("rctrl_renwnd32");
                    foreach (var win in windows)
                    {
                        var title = (win.GetAttribute("Name") ?? "").Trim();
                        if (title.EndsWith("- Outlook", StringComparison.OrdinalIgnoreCase)
                            || title.Contains("Inbox", StringComparison.OrdinalIgnoreCase))
                        {
                            mainWindow = win;
                            break;
                        }
                    }
                    if (mainWindow != null)
                        break;
                }
                catch { }
                Thread.Sleep(500);
            }
            if (mainWindow == null)
                throw new TimeoutException("Outlook main window not found.");
            // ✅ ATTACH SESSION
            string handleHex = int.Parse(
                mainWindow.GetAttribute("NativeWindowHandle")).ToString("x");
            var caps = new AppiumOptions();
            caps.AddAdditionalCapability("appTopLevelWindow", handleHex);
            caps.AddAdditionalCapability("deviceName", "Windows");
            OutlookSession = new WindowsDriver<WindowsElement>(
                new Uri(WinAppDriverUrl), caps);
            OutlookSession.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
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
            // Do not call Quit() on the OutlookSession because that can close
            // Outlook compose windows unexpectedly. Instead, detach our reference
            // so a future attach will create a fresh session without sending a Quit
            // command to the Outlook process.
            OutlookSession = null;
            try { DesktopSession?.Quit(); } catch { }
            try { WebSession?.Quit(); } catch { }

            try
            {
                if (WinApp != null && !WinApp.HasExited)
                    WinApp.Kill();
            }
            catch { }
        }

        
    }
}