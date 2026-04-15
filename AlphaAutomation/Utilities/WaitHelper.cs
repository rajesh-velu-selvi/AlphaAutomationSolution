using System;
using System.Linq;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace AlphaAutomation.Utilities
{
    public static class WaitHelper
    {
        public static WebDriverWait CreateWait(IWebDriver driver, TimeSpan timeout)
        {
            var wait = new WebDriverWait(driver, timeout);
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            wait.PollingInterval = TimeSpan.FromMilliseconds(100);
            return wait;
        }

        public static void WaitForUiReady(IWebDriver driver)
        {
            try
            {
                if (driver is IJavaScriptExecutor js)
                {
                    var wait = CreateWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until(drv => js.ExecuteScript("return document.readyState")?.ToString() == "complete");
                }
            }
            catch
            {
                // best-effort only; swallow to avoid breaking callers
            }
        }

        public static void SafeClick(IWebDriver driver, WebDriverWait wait, By locator)
        {
            var element = wait.Until(drv => drv.FindElement(locator));
            const int attempts = 3;
            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    element.Click();
                    return;
                }
                catch (ElementClickInterceptedException)
                {
                    Thread.Sleep(500);
                    element = driver.FindElement(locator);
                }
                catch (StaleElementReferenceException)
                {
                    element = driver.FindElement(locator);
                }
            }
            // final attempt, let exceptions bubble if it still fails
            element.Click();
        }

        public static void SafeSendKeys(IWebDriver driver, WebDriverWait wait, By locator, string text)
        {
            var element = wait.Until(drv => drv.FindElement(locator));
            try
            {
                element.Clear();
            }
            catch (Exception) { /* ignore clear failures and attempt to send keys */ }
            element.SendKeys(text);
        }

        public static void SwitchToFrame(IWebDriver driver, WebDriverWait wait, By frame)
        {
            var frameElement = wait.Until(drv => drv.FindElement(frame));
            driver.SwitchTo().Frame(frameElement);
        }

        public static void AcceptAlertIfPresent(IWebDriver driver, int timeoutSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(drv => IsAlertPresent(drv));

                driver.SwitchTo().Alert().Accept();
            }

            catch (WebDriverTimeoutException)
            {
                // ✅ No alert appeared – safe to continue
            }

        }

        private static bool IsAlertPresent(IWebDriver driver)
        {
            try
            {
                driver.SwitchTo().Alert();
                return true;
            }
            catch (NoAlertPresentException)
            {
                return false;
            }
        }


        // Added to fix CS0117: method used by WebWizardPage
        public static void WaitForSpinnerToDisappear(IWebDriver driver, TimeSpan timeout, By spinnerLocator)
        {
            var wait = CreateWait(driver, timeout);
            try
            {
                wait.Until(drv =>
                {
                    try
                    {
                        var elems = drv.FindElements(spinnerLocator);
                        // If no elements or no displayed spinner, consider spinner gone
                        return elems == null || elems.Count == 0 || !elems.Any(e => e.Displayed);
                    }
                    catch (StaleElementReferenceException)
                    {
                        // If elements become stale, consider that spinner refreshed/removed; keep waiting by returning false
                        return false;
                    }
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                throw new WebDriverTimeoutException($"Spinner '{spinnerLocator}' did not disappear within {timeout}.", ex);
            }
        }
    }
}