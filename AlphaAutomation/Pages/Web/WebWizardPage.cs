using AlphaAutomation.Utilities;
using AventStack.ExtentReports.Model;
using AventStack.ExtentReports.Reporter.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace AlphaAutomation.Pages.Web
{
    public class WebWizardPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;

        public WebWizardPage(IWebDriver driver, TimeSpan timeout)
        {
            _driver = driver ?? throw new ArgumentNullException(nameof(driver));
            _wait = WaitHelper.CreateWait(_driver, timeout);


        }

        private By LnkCreateOutlookEmail => By.XPath("//a[contains(., 'OUTLOOK') and contains(., 'EMAIL')]");
        private By FrmWizard => By.Name("wndCorrespondenceWizard");
        private By BtnRadio1 => By.Id("templateListRadio_1");
        private By BtnNext => By.XPath("//a[text()='Next']");
        private By BtnFinish => By.XPath("//a[text()='Finish']");
        private By LnkFileCabinet => By.Id("lnkFileCabinet");
        private By Spinner => By.Id("cover-spin");

        public void GoTo(string url) => _driver.Navigate().GoToUrl(url);

        public void ClickCreateOutlookEmail()
        {
            WaitHelper.WaitForSpinnerToDisappear(_driver, TimeSpan.FromSeconds(10), Spinner);

            var createLink = _wait.Until(drv =>
            {
                var el = drv.FindElement(LnkCreateOutlookEmail);
                return (el.Displayed && el.Enabled) ? el : null;
            }) ?? throw new NoSuchElementException($"Element '{LnkCreateOutlookEmail}' was not found or not clickable.");
            Thread.Sleep(500);
            createLink.Click();
        }
        // brief pause to allow the wizard frame to load before interactions
        
        public void CompleteWizard()

        {
            WaitHelper.SwitchToFrame(_driver, _wait, FrmWizard);

            WaitHelper.WaitForSpinnerToDisappear(_driver, TimeSpan.FromSeconds(60), Spinner);
            Thread.Sleep(500);

            var radio1 = _wait.Until(drv =>
            {
                var el = drv.FindElement(BtnRadio1);
                return (el.Displayed && el.Enabled) ? el : null;
            }) ?? throw new NoSuchElementException($"Element '{BtnRadio1}' was not found or not clickable.");
            radio1.Click();
            Thread.Sleep(1000);
            var next = _wait.Until(drv =>
            {
                var el = drv.FindElement(BtnNext);
                return (el.Displayed && el.Enabled) ? el : null;
            }) ?? throw new NoSuchElementException($"Element '{BtnNext}' was not found or not clickable.");
            // Ensure any loading spinner has disappeared before attempting to click Next
            WaitHelper.WaitForSpinnerToDisappear(_driver, TimeSpan.FromSeconds(60), Spinner);
            Thread.Sleep(250);
            try
            {
                next.Click();
            }
            catch (ElementClickInterceptedException)
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", next);
            }


            var finish = _wait.Until(drv =>
            {
                var el = drv.FindElement(BtnFinish);
                return (el.Displayed && el.Enabled) ? el : null;
            }) ?? throw new NoSuchElementException($"Element '{BtnFinish}' was not found or not clickable.");
            // Ensure any loading spinner has disappeared before attempting to click Finish
            WaitHelper.WaitForSpinnerToDisappear(_driver, TimeSpan.FromSeconds(60), Spinner);
            Thread.Sleep(250);
            finish.Click();


            _driver.SwitchTo().DefaultContent();
            
            
        }

        public void Closebrowser()
        {
            _driver.Quit();
        }
        public void ClickFileCabinetButton()
        {
            var fileCabinet = _wait.Until(drv =>
            {
                var el = drv.FindElement(LnkFileCabinet);
                return (el.Displayed && el.Enabled) ? el : null;
            }) ?? throw new NoSuchElementException($"Element '{LnkFileCabinet}' was not found or not clickable.");
            fileCabinet.Click();

        }
        public void SwitchToNewTab()
        {
            // Capture a copy of the existing window handles so they don't change as the driver updates
            IReadOnlyCollection<string> originalHandles;
            try
            {
                originalHandles = _driver.WindowHandles.ToList();
            }
            catch (WebDriverException ex)
            {
                // If the browser session has been closed, surface a clearer error for diagnostics
                throw new InvalidOperationException("Unable to obtain window handles because the WebDriver session is not available. The browser may have closed or crashed.", ex);
            }

            // Wait until a new handle appears
            try
            {
                _wait.Until(drv =>
                {
                    try { return drv.WindowHandles.Count > originalHandles.Count; }
                    catch (WebDriverException) { return false; }
                });
            }
            catch (WebDriverTimeoutException)
            {
                // If no new handle appeared within the timeout, we will attempt a best-effort switch to the last handle
            }

            // Try to find a handle that wasn't present before
            string? newHandle = null;
            try
            {
                newHandle = _driver.WindowHandles.Except(originalHandles).FirstOrDefault();
            }
            catch (WebDriverException ex)
            {
                throw new InvalidOperationException("Failed to retrieve updated window handles; WebDriver session may be closed.", ex);
            }

            if (newHandle != null)
            {
                _driver.SwitchTo().Window(newHandle);
                return;
            }

            // Best-effort fallback: switch to the most recently opened handle if available
            IReadOnlyCollection<string>? allHandles = null;
            try
            {
                allHandles = _driver.WindowHandles;
            }
            catch (WebDriverException ex)
            {
                throw new InvalidOperationException("Failed to retrieve window handles for fallback switch; WebDriver session may be closed.", ex);
            }

            if (allHandles != null && allHandles.Count > 0)
            {
                _driver.SwitchTo().Window(allHandles.Last());
                return;
            }

            throw new InvalidOperationException("New tab did not open as expected.");
        }

        private static readonly Regex DuplicateSuffixRegex = new Regex(@"\s\d+$", RegexOptions.Compiled);

        public void ClickEmailGrid(string subject)
        {
            WaitHelper.WaitForUiReady(_driver);

            int maxAttempts = 3;
            IWebElement matchedIcon = null;
            string normalizedSubject = NormalizeFileName(subject);

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    

                    matchedIcon = _wait.Until(driver =>
                    {
                        try
                        {
                            var rows = driver.FindElements(
                                By.XPath("//tr[contains(@class,'k-table-row') and contains(@class,'k-master-row')]"));

                            foreach (var row in rows)
                            {
                                try
                                {
                                    // Filename lives in div.filenameLink > div.truncateCell
                                    var fileNameElement = row.FindElement(
                                        By.XPath(".//div[contains(@class,'filenameLink')]"));

                                    string fileText = (fileNameElement.Text ?? "").Trim();
                                    if (string.IsNullOrEmpty(fileText))
                                        continue;

                                    string normalizedFileName = NormalizeFileName(fileText);

                                    if (!normalizedFileName.Equals(normalizedSubject, StringComparison.OrdinalIgnoreCase))
                                        continue;

                                    // Email icon is identified by icon-file-color-email, not k-i-email
                                    var icon = row.FindElement(
                                        By.XPath(".//span[contains(@class,'icon-file-color-email')]"));

                                    if (icon.Displayed && icon.Enabled)
                                        return icon;
                                }
                                catch (NoSuchElementException) { continue; }
                                catch (StaleElementReferenceException) { continue; }
                            }

                            return null;
                        }
                        catch (NoSuchElementException)
                        {
                            return null;
                        }
                    });

                    if (matchedIcon != null)
                        break;
                }
                catch (WebDriverTimeoutException)
                {
                    // Ignore and retry
                }

                if (attempt < maxAttempts)
                {
                    _driver.Navigate().Refresh();
                    
                }
            }

            if (matchedIcon == null)
            {
                var rowTexts = _driver.FindElements(By.XPath("//tr[contains(@class,'k-table-row') and contains(@class,'k-master-row')]"))
                    .Select(r => (r.Text ?? "").Trim())
                    .Where(t => !string.IsNullOrEmpty(t))
                    .ToList();

                throw new NoSuchElementException(
                    $"No matching email found for subject: {subject}. " +
                    $"Rows found: {rowTexts.Count}. Raw row text:\n{string.Join("\n---\n", rowTexts)}");
            }

            matchedIcon.Click();
        }

        private static string NormalizeFileName(string fileText)
        {
            string name = (fileText ?? "").Trim();

            if (name.EndsWith(".msg", StringComparison.OrdinalIgnoreCase))
                name = name.Substring(0, name.Length - 4);

            name = DuplicateSuffixRegex.Replace(name, "").Trim();

            return name;
        }






    }




}


