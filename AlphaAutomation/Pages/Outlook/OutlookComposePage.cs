using System;
using System.Linq;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium;


namespace AlphaAutomation.Pages.Outlook
{
    public class OutlookComposePage
    {
        private readonly WindowsDriver<WindowsElement> _session;

        public OutlookComposePage(WindowsDriver<WindowsElement> outlookSession)
        {
            _session = outlookSession ?? throw new ArgumentNullException(nameof(outlookSession));
        }

        public void ClickPlugin()
        {
            var moreCommands = TryFind(() => _session.FindElementByXPath("//*[@Name='More Commands']"))
                               ?? TryFind(() => _session.FindElementsByClassName("NetUIOverflowAnchor").FirstOrDefault());
            moreCommands?.Click();

            // Wait for the plugin menu item to appear — retry a few times to avoid race conditions with WinAppDriver
            WindowsElement? plugin = null;
            for (int attempt = 0; attempt < 8 && plugin == null; attempt++)
            {
                plugin = TryFind(() => _session.FindElementByXPath("//*[@Name='Correspondence (Alpha)']"))
                         ?? TryFind(() => _session.FindElementsByClassName("NetUITWBtnMenuItem").FirstOrDefault());
                if (plugin == null)
                    System.Threading.Thread.Sleep(250);
            }

            // Attempt to click the plugin with retries in case the remote end reports transient errors
            if (plugin != null)
            {
                for (int attempt = 0; attempt < 3; attempt++)
                {
                    try
                    {
                        plugin.Click();
                        break;
                    }
                    catch (OpenQA.Selenium.WebDriverException)
                    {
                        // small delay and attempt to re-find the element before retrying
                        System.Threading.Thread.Sleep(200);
                        plugin = TryFind(() => _session.FindElementByXPath("//*[@Name='Correspondence (Alpha)']"))
                                 ?? TryFind(() => _session.FindElementsByClassName("NetUITWBtnMenuItem").FirstOrDefault());
                    }
                }
            }

            var actions = new Actions(_session);
            for (int i = 0; i < 5; i++) 
                
                actions.SendKeys(Keys.ArrowDown).Perform();

            var radio = TryFind(() => _session.FindElementByAccessibilityId("4"))
                        ?? TryFind(() => _session.FindElementsByClassName("iDocument pi text-sm pi-circle-off").FirstOrDefault());
            radio?.Click();
        }

        public void FillEmailFields(string to, string cc, string subject)
        {
            WindowsElement? toBox = TryFind(() => _session.FindElementByXPath("//Edit[@Name='To']"))
                                  ?? TryFind(() => _session.FindElementsByClassName("Edit").FirstOrDefault());
            toBox?.Clear();
            toBox?.SendKeys(to);

            if (!string.IsNullOrWhiteSpace(cc))
            {
                var ccBox = TryFind(() => _session.FindElementByXPath("//Edit[@Name='Cc']"));
                ccBox?.Clear();
                ccBox?.SendKeys(cc);
            }

            var sub = TryFind(() => _session.FindElementByXPath("//Edit[@ClassName='RichEdit20WPT' and @Name='Subject']"));
            sub?.Clear();
            sub?.SendKeys(subject);
        }

        public void ClickSendButton()
        {
            var sendBtn = TryFind(() => _session.FindElementByXPath("//Button[@Name='Send']"));
            sendBtn?.Click();
        }

        private static WindowsElement? TryFind(Func<WindowsElement?> finder)
        {
            try { return finder(); } catch { return null; }
        }
    }
}

