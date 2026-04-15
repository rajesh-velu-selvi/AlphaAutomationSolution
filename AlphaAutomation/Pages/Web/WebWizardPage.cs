using AlphaAutomation.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Collections.Generic;

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
        private By BtnNext => By.XPath("//a[text()='Next']");
        private By TxtDescription => By.Id("description");
        private By SelDocumentType => By.Id("selDocumentType");
        private By BtnFinish => By.XPath("//a[text()='Finish']");
        private By LnkFileCabinet => By.Id("lnkFileCabinet");
        private By Spinner => By.Id("cover-spin");

        public void GoTo(string url) => _driver.Navigate().GoToUrl(url);

        public void ClickCreateOutlookEmail()
        {
            WaitHelper.WaitForSpinnerToDisappear(_driver, TimeSpan.FromSeconds(60), Spinner);

            var createLink = _wait.Until(drv =>
            {
                var el = drv.FindElement(LnkCreateOutlookEmail);
                return (el.Displayed && el.Enabled) ? el : null;
            }) ?? throw new NoSuchElementException($"Element '{LnkCreateOutlookEmail}' was not found or not clickable.");

            createLink.Click();
        }

        public void CompleteWizard(string descriptionText, string documentTypeText, string documentValueFallback)
        {
            WaitHelper.SwitchToFrame(_driver, _wait, FrmWizard);

            WaitHelper.WaitForSpinnerToDisappear(_driver, TimeSpan.FromSeconds(60), Spinner);
            var next = _wait.Until(drv =>
            {
                var el = drv.FindElement(BtnNext);
                return (el.Displayed && el.Enabled) ? el : null;
            }) ?? throw new NoSuchElementException($"Element '{BtnNext}' was not found or not clickable.");
            next.Click();

            var desc = _wait.Until(drv =>
            {
                var el = drv.FindElement(TxtDescription);
                return (el.Displayed && el.Enabled) ? el : null;
            }) ?? throw new NoSuchElementException($"Element '{TxtDescription}' was not found or not interactable.");
            desc.Clear();
            desc.SendKeys(descriptionText);

            var dropdown = _wait.Until(drv =>
            {
                var el = drv.FindElement(SelDocumentType);
                return (el.Displayed && el.Enabled) ? el : null;
            }) ?? throw new NoSuchElementException($"Element '{SelDocumentType}' was not found or not interactable.");

            if (string.Equals(dropdown.TagName, "select", StringComparison.OrdinalIgnoreCase))
            {
                var select = new SelectElement(dropdown);

                // Wait for options to populate
                try
                {
                    _wait.Until(drv => select.Options != null && select.Options.Count > 0);
                }
                catch (WebDriverTimeoutException)
                {
                    throw new NoSuchElementException($"Select element '{SelDocumentType}' has no options after waiting.");
                }

                // Try select by visible text first, then by value fallback. If neither found, include available options in the exception.
                try
                {
                    select.SelectByText(documentTypeText);
                }
                catch (NoSuchElementException)
                {
                    try
                    {
                        select.SelectByValue(documentValueFallback);
                    }
                    catch (NoSuchElementException)
                    {
                        var available = string.Join(",", select.Options.Select(o => (o.GetAttribute("value") ?? "") + ":" + o.Text));
                        throw new NoSuchElementException($"Unable to select document type by text '{documentTypeText}' or value '{documentValueFallback}'. Available options: {available}");
                    }
                }
            }
            else
            {
                dropdown.Click();
            }

            var finish = _wait.Until(drv =>
            {
                var el = drv.FindElement(BtnFinish);
                return (el.Displayed && el.Enabled) ? el : null;
            }) ?? throw new NoSuchElementException($"Element '{BtnFinish}' was not found or not clickable.");

            // Wait for any overlaying disabled list items to disappear which have been observed to intercept clicks
            try
            {
                var overlayLocator = By.XPath("//li[@aria-hidden='false' and contains(@class,'disabled')]");
                _wait.Until(drv => drv.FindElements(overlayLocator).Count == 0);
            }
            catch (WebDriverTimeoutException)
            {
                // proceed to click anyway; fallback will handle interception
            }

            // Try a normal click, but if another element intercepts it, fall back to a JS click (scroll into view then click)
            try
            {
                finish.Click();
            }
            catch (ElementClickInterceptedException)
            {
                try
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", finish);
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", finish);
                }
                catch
                {
                    // If JS click also fails, rethrow the original exception to preserve stack trace for diagnostics
                    throw;
                }
            }

            _driver.SwitchTo().DefaultContent();
        }

        public void ClickFileCabinetButton()
        {
            var fileCabinet = _wait.Until(drv =>
            {
                var el = drv.FindElement(LnkFileCabinet);
                return (el.Displayed && el.Enabled) ? el : null;
            }) ?? throw new NoSuchElementException($"Element '{LnkFileCabinet}' was not found or not clickable.");

            // Override page alert/confirm handlers so that any confirm that appears when opening the file cabinet is auto-accepted
            try
            {
                if (_driver is IJavaScriptExecutor js)
                {
                    js.ExecuteScript("window.confirm = function(){return true;}; window.alert = function(){}; window.onbeforeunload = null;");
                }
            }
            catch
            {
                // best-effort; if JS execution fails, continue and rely on AcceptAlertIfPresent
            }

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
                // Accept any confirm/alert that appears immediately after switching to the new tab
                AcceptAlert();
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
                // Accept any confirm/alert that appears immediately after switching to the new tab
                AcceptAlert();
                return;
            }

            throw new InvalidOperationException("New tab did not open as expected.");
        }
        public void AcceptAlert()
        {
            WaitHelper.AcceptAlertIfPresent(_driver, 10);
        }
        public void ClickEmailGrid()
        {

            var emailGrid = _wait.Until(drv =>
            {
                var el = drv.FindElement(By.XPath("(//span[@title='Open File In Native App'])[1]"));
                return (el.Displayed && el.Enabled) ? el : null;
            }) ?? throw new NoSuchElementException($"Element 'emailGrid' was not found or not clickable.");
            emailGrid.Click();

        }

    }
}

