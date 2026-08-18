using AlphaAutomation.Helpers;
using Castle.Components.DictionaryAdapter;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml.Linq;

namespace AlphaAutomation.Pages.Outlook
{
    /// <summary>
    /// Page object for the Outlook Inbox — finds and opens a specific email.
    /// </summary>
    public class OutlookInboxPage
    {
        private readonly WindowsDriver<WindowsElement> _session;
        private readonly TimeSpan _timeout;

        public OutlookInboxPage(WindowsDriver<WindowsElement> desktopSession, TimeSpan timeout)
        {
            _session = desktopSession ?? throw new ArgumentNullException(nameof(desktopSession));
            _timeout = timeout;
        }

        public OutlookInboxPage(WindowsDriver<WindowsElement> windowsDriver)
        {
            _session = windowsDriver ?? throw new ArgumentNullException(nameof(windowsDriver));
            _timeout = TimeSpan.FromSeconds(30);
        }

        // ── Public API ────────────────────────────────────────────────────────

        // Use BaseSession.AttachToOutlookMainWindow(timeout) to attach a WindowsDriver to
        // the running Outlook main window. Having the helper in BaseSession centralizes
        // session management and avoids duplicate logic.

        /// <summary>
        /// Navigate to the Inbox folder in the folder pane.
        /// </summary>
        public void GoToInbox()
        {
            try
            {
                // Click "Inbox" in the folder tree on the left pane
                var inbox = TryFind(() =>
                    _session.FindElementByXPath("//TreeItem[starts-with(@Name,'Inbox')]"));

                inbox?.Click();
                Pause(800);
            }
            catch { /* already on inbox */ }
        }

        /// <summary>
        /// Find and double-click the first email whose subject contains
        /// <paramref name="subjectKeyword"/> (case-insensitive).
        /// </summary>
        public void OpenEmailBySubject(string subject)
        {
            // Reduced wait for test debugging: original delay was 1 minute
            Thread.Sleep(TimeSpan.FromSeconds(2)); // Wait for the email list to load
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Subject cannot be empty.", nameof(subject));

            var watch = Stopwatch.StartNew();

            while (watch.Elapsed < _timeout)
            {
                try
                {
                    // Email rows in Outlook mail list are ListItem elements
                    var emailRows = _session.FindElementsByClassName("LeafRow");

                    if (emailRows == null || emailRows.Count == 0)
                        emailRows = _session.FindElementsByXPath("//ListItem");

                    foreach (var row in emailRows)
                    {
                        string name = (row.GetAttribute("Name") ?? "").Trim();

                        if (name.Contains(subject, StringComparison.OrdinalIgnoreCase))
                        {
                            // Double-click to open the email
                            var actions = new Actions(_session);
                            actions.Click(row).Perform();
                            Console.WriteLine(
                                $"[{DateTime.Now:HH:mm:ss}] Opened email: \"{name}\"");
                            Pause(1500);

                            // After opening the item, attach a new Outlook session for the
                            // opened mail inspector/compose window so subsequent actions
                            // operate against the email window instead of the main Outlook UI.
                            try
                            {
                                BaseSession.AttachToOutlookMainWindow(subject, _timeout);
                            }
                            catch (Exception ex)
                            {
                                // Log and continue — callers may still attempt to attach
                                // explicitly if needed.
                                Console.WriteLine($"Failed to attach to opened message window: {ex.Message}");
                            }

                            return;
                        }
                    }
                }
                catch { /* keep polling */ }

                Thread.Sleep(500);
            }

            throw new TimeoutException(
                $"Email with subject containing '{subject}' not found in Inbox within timeout.");
        }

       
        public void ClickImportAddinButton()
        {
            var session = BaseSession.OutlookSession ?? _session;
            if (session == null)
                throw new InvalidOperationException("No Outlook session available to click import add-in. Ensure Outlook is running and attached via BaseSession.");

            var moreCommands = FindWithRetry(
                () => TryFind(() => session.FindElementByXPath("//*[@Name='More Commands, Message']"))
                      ?? TryFind(() => session.FindElementsByClassName("NetUIOverflowAnchor").FirstOrDefault()),
                elementDescription: "More Commands element",
                maxAttempts: 3,
                retryDelayMs: 1000);

            if (moreCommands == null)
                throw new NoSuchElementException("More Commands element not found after retries");
            moreCommands.Click();
            Thread.Sleep(500);

            var importAddin = FindWithRetry(
                () => TryFind(() => session.FindElementByXPath("//*[@Name='Full Import (Alpha)']"))
                      ?? TryFind(() => session.FindElementsByClassName("NetUITWBtnMenuItem").FirstOrDefault()),
                elementDescription: "Import Add-in element",
                maxAttempts: 3,
                retryDelayMs: 1000);

            if (importAddin == null)
                throw new NoSuchElementException("Import Add-in element not found after retries");
            importAddin.Click();

            Thread.Sleep(2000);
        }

        /// <summary>
        /// Retries a WinAppDriver element lookup a fixed number of times, with a delay between attempts.
        /// Logs each failed attempt so retries are visible in test output/logs.
        /// </summary>
        private WindowsElement FindWithRetry(
            Func<WindowsElement> findFunc,
            string elementDescription,
            int maxAttempts = 3,
            int retryDelayMs = 1000)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var element = findFunc();
                if (element != null)
                    return element;

                Console.WriteLine($"[Retry] {elementDescription} not found on attempt {attempt}/{maxAttempts}.");

                if (attempt < maxAttempts)
                    Thread.Sleep(retryDelayMs);
            }

            return null;
        }

        public void SelectOneAttachmentToImport()
        {
            var session = BaseSession.OutlookSession ?? _session;
            if (session == null)
                throw new InvalidOperationException("No Outlook session available to click attachment checkbox. Ensure Outlook is running and attached via BaseSession.");
            var SelectAllCheckbox = TryFind(() => session.FindElementByAccessibilityId("selectAllAttachments")) 
                ?? TryFind(() => _session.FindElementsByClassName("p-checkbox-input").FirstOrDefault());
            if (SelectAllCheckbox == null)
                throw new NoSuchElementException($"Select All checkbox not found");
            SelectAllCheckbox.Click();
            Thread.Sleep(500);

            var attachmentCheckboxChecked = TryFind(() => session.FindElementByAccessibilityId("attachment_1"))
                ?? TryFind(() => _session.FindElementsByClassName("p-checkbox-input").FirstOrDefault());
            if (attachmentCheckboxChecked == null)
                throw new NoSuchElementException($"Attachment checkbox with not found");
            attachmentCheckboxChecked.Click();
            Thread.Sleep(500);
        }

        public void DeSelectAllAttachmentAndImport()
        { 
             var session = BaseSession.OutlookSession ?? _session;
            if (session == null)
                throw new InvalidOperationException("No Outlook session available to click attachment checkbox. Ensure Outlook is running and attached via BaseSession.");
        var SelectAllCheckbox = TryFind(() => session.FindElementByAccessibilityId("selectAllAttachments"))
            ?? TryFind(() => _session.FindElementsByClassName("p-checkbox-input").FirstOrDefault());
            if (SelectAllCheckbox == null)
                throw new NoSuchElementException($"Select All checkbox not found");
        SelectAllCheckbox.Click();
            Thread.Sleep(2000);
            }

        public void ClickImportButton()
        {
            var session = BaseSession.OutlookSession ?? _session;
            if (session == null)
                throw new InvalidOperationException("No Outlook session available to click import button. Ensure Outlook is running and attached via BaseSession.");
            var importButton = TryFind(() => session.FindElementByName("Import"))
                ?? TryFind(() => _session.FindElementsByClassName("p-button p-component p-button-sm p-button-primary").FirstOrDefault());
            if (importButton == null)
                throw new NoSuchElementException($"Import button not found");
            importButton.Click();
            Thread.Sleep(5000);
            var importSuccessMessage = TryFind(() => session.FindElementByXPath("//*[@Name='This email has been successfully archived to the designated document management system.']"));
            if (importSuccessMessage != null)
            {
                string messageText = importSuccessMessage.GetAttribute("Name");
                Console.WriteLine($"Success message found: {messageText}");
                // or for MSTest logging:
                // TestContext.WriteLine($"Success message found: {messageText}");
            }
            else
            {
                Console.WriteLine("Success message element was NOT found.");
            }
            var minimizeButton = TryFind(() => _session.FindElementByName("Minimize"))
                ?? TryFind(() => _session.FindElementsByClassName("NetUIAppFrameHelper").FirstOrDefault());
            try
            {
                minimizeButton.Click();
            }
            catch (WebDriverException ex)
            {
                // Element exists but may not be interactable; log and continue
                Console.WriteLine($"Could not click minimize button: {ex.Message}");
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static void Pause(int ms = 500) => Thread.Sleep(ms);

        private static WindowsElement? TryFind(Func<WindowsElement?> finder)
        {
            try { return finder(); } catch { return null; }
        }
    }
}