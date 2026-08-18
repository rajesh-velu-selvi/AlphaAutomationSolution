using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using System;
using System.Linq;
using System.Threading;


namespace AlphaAutomation.Pages.Outlook
{
    public class OutlookComposePage
    {
        private readonly WindowsDriver<WindowsElement> _session;
        private const int DefaultWaitTimeoutSeconds = 20;

        public OutlookComposePage(WindowsDriver<WindowsElement> outlookSession)
        {
            _session = outlookSession ?? throw new ArgumentNullException(nameof(outlookSession));
        }

        public void ClickPlugin()
        {
            var moreCommands = TryFind(() => _session.FindElementByXPath("//*[@Name='More Commands']"))
                               ?? TryFind(() => _session.FindElementsByClassName("NetUIOverflowAnchor").FirstOrDefault());
            if (moreCommands == null)
                throw new NoSuchElementException("More Commands element not found");

            // Attempt to open the menu and click the plugin with retries to handle transient UI flakiness
            const int maxAttempts = 3;
            Exception? lastEx = null;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    moreCommands.Click();
                    WaitForActionToComplete();

                    // Scenario One - Select Correspondence (Alpha) plugin
                    var plugin = TryFind(() => _session.FindElementByXPath("//*[@Name='Correspondence (Alpha)']"))
                                 ?? TryFind(() => _session.FindElementsByClassName("NetUITWBtnMenuItem").FirstOrDefault());
                    if (plugin != null)
                    {
                        plugin.Click();
                        WaitForActionToComplete();
                        Thread.Sleep(2000);
                        return;
                    }

                    lastEx = new NoSuchElementException($"Plugin element not found on attempt {attempt}");
                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                    Thread.Sleep(500);
                }
            }

            throw lastEx ?? new NoSuchElementException("Plugin element not found after retries");
        }

        //Scenario Two -Select Document from List
        public void AttachDocumentFromList()
        {

            var radio = TryFind(() => _session.FindElementByAccessibilityId("4"))
                        ?? TryFind(() => _session.FindElementsByClassName("iDocument pi text-sm pi-circle-off").FirstOrDefault());
            if (radio == null)
                throw new NoSuchElementException("Radio button element not found");
            WaitForActionToComplete();
            radio.Click();
            Thread.Sleep(2000);
            WaitForActionToComplete();
        }

        //Scenario Three -Select Document Library
        public void AttachDocumentfromLibrary()
        {
            var documentLibrary = TryFind(() => _session.FindElementByXPath("//*[contains(@Name,'Document Library')]//following::Button[2]"))
                                  ?? TryFind(() => _session.FindElementsByClassName("p-panel-header-icon p-link mr-2").FirstOrDefault());
            if (documentLibrary == null)
                throw new NoSuchElementException("Document library element not found");
            documentLibrary.Click();
            WaitForActionToComplete();
            Thread.Sleep(1000);

            var radio = TryFind(() => _session.FindElementByAccessibilityId("1003791"))
                        ?? TryFind(() => _session.FindElementsByClassName("iDocument pi text-sm pi-circle-off").FirstOrDefault());
            if (radio == null)
                throw new NoSuchElementException("Radio button element not found");
            radio.Click();
            Thread.Sleep(2000);
            WaitForActionToComplete();
        }

        // Scenario Six - Select Multiple Documents from Library
        public void AttachMultipleDocumentsFromLibrary()
        {
            var documentLibrary = TryFind(() => _session.FindElementByXPath("//*[contains(@Name,'Document Library')]//following::Button[2]"))
                                  ?? TryFind(() => _session.FindElementsByClassName("p-panel-header-icon p-link mr-2").FirstOrDefault());
            if (documentLibrary == null)
                throw new NoSuchElementException("Document library element not found");
            documentLibrary.Click();
            WaitForActionToComplete();
            Thread.Sleep(2000);

            var radio1 = TryFind(() => _session.FindElementByAccessibilityId("1003791"))
                        ?? TryFind(() => _session.FindElementsByClassName("iDocument pi text-sm pi-circle-off").FirstOrDefault());
            if (radio1 == null)
                throw new NoSuchElementException("Radio1 button element not found");
            radio1.Click();
            Thread.Sleep(2000); 
            WaitForActionToComplete();
            var radio2 = TryFind(() => _session.FindElementByAccessibilityId("1003792"))
                        ?? TryFind(() => _session.FindElementsByClassName("iDocument pi text-sm pi-circle-off").FirstOrDefault());
            if (radio2 == null)
                throw new NoSuchElementException("Radio2 button element not found");
            radio2.Click();
            Thread.Sleep(2000); WaitForActionToComplete();
            
        }

        //Scenario Four - Attach document from PC
        public void AttachDocumentFromPC()
        {
            var attachFromPC = TryFind(() => _session.FindElementByAccessibilityId("AttachFileSplit"))
                        ?? TryFind(() => _session.FindElementsByClassName("NetUIAnchor").FirstOrDefault());
            if ((attachFromPC == null))
                throw new NoSuchElementException("Attach from PC element not found");
            attachFromPC.Click();
            WaitForActionToComplete();
            var browseThisPC = TryFind(() => _session.FindElementByXPath("//*[contains(@Name,'Browse This PC')]"))
                        ?? TryFind(() => _session.FindElementsByClassName("NetUITWBtnMenuItem").FirstOrDefault());
            if ((browseThisPC == null))
                throw new NoSuchElementException("Browse This PC element not found");
            browseThisPC.Click();
            WaitForActionToComplete();
            _session.FindElementByXPath("//*[contains(@Name,'Documents')]").Click();
            WaitForActionToComplete();
            _session.FindElementByXPath("//*[contains(@Name,'Automation') and @ClassName='UIItem']").Click();
            WaitForActionToComplete();
            _session.FindElementByXPath("//*[contains(@Name,'Open') and @ClassName='Button']").Click();
            WaitForActionToComplete();
            _session.FindElementByXPath("//*[contains(@Name,'TestingDocument') and @ClassName='UIItem']").Click();
            WaitForActionToComplete();
            _session.FindElementByXPath("//*[contains(@Name,'Insert') and @ClassName='Button']").Click();

        }

        public void FillEmailFields(string to, string Subject)

        {
            if (string.IsNullOrWhiteSpace(to))
                throw new ArgumentException("To field cannot be empty", nameof(to));
            if (string.IsNullOrWhiteSpace(Subject))
                throw new ArgumentException("Subject field cannot be empty", nameof(Subject));
            WindowsElement? toBox = FindToFieldWithRetry();
            if (toBox == null)
                throw new NoSuchElementException("To field element not found");
            toBox.Clear();
            toBox.SendKeys(to);
            WaitForActionToComplete();

            var sub = TryFind(() => _session.FindElementByXPath("//Edit[@ClassName='RichEdit20WPT' and @Name='Subject']"));
            if (sub == null)
                throw new NoSuchElementException("Subject field element not found");
            sub.Clear();
            sub.SendKeys(Subject);
            WaitForActionToComplete();
        }
            public void ClickSendButton()
        {
            var sendBtn = TryFind(() => _session.FindElementByXPath("//Button[@Name='Send']"));
            if (sendBtn == null)
                throw new NoSuchElementException("Send button element not found");
            sendBtn.Click();
            WaitForActionToComplete();
        }

        private void WaitForActionToComplete()
        {
            Thread.Sleep(2000);
        }

        private WindowsElement? FindToFieldWithRetry(int retryCount = 5, int delayMs = 200)
        {
            for (int i = 0; i < retryCount; i++)
            {
                var toBox = TryFind(() => _session.FindElementByXPath("//Edit[@Name='To']"))
                          ?? TryFind(() => _session.FindElementsByClassName("Edit").FirstOrDefault());
                if (toBox != null)
                    return toBox;
                Thread.Sleep(delayMs);
            }
            return null;
        }
        public void EditDocumentType()

        {
            var actions = new Actions(_session);
            for (int i = 0; i < 5; i++)
            {
                actions.SendKeys(Keys.ArrowDown);
            }
            actions.Perform();
            WaitForActionToComplete();
            var docTypeEditButton = TryFind(() => _session.FindElementByClassName("pi pi-file-edit document-type-edit-icon"));
            if (docTypeEditButton == null)
                throw new NoSuchElementException("Document type edit button element not found");
            docTypeEditButton.Click();
            
            WaitForActionToComplete(); 
            Thread.Sleep(200);
            var desiredOption = TryFind(() => _session.FindElementByAccessibilityId("doc-type-dropdown-control_10"))
                                ?? TryFind(() => _session.FindElementsByClassName("p-dropdown-item p-focus").FirstOrDefault());
            if (desiredOption == null)
                throw new NoSuchElementException("Desired document type option element not found");
            desiredOption.Click();

            var saveButton = TryFind(() => _session.FindElementByAccessibilityId("documentTypeSaveButton"))
                             ?? TryFind(() => _session.FindElementsByClassName("p-button p-component p-button-sm").FirstOrDefault());
            if (saveButton == null)
                throw new NoSuchElementException("Save button element not found");
            saveButton.Click();
            WaitForActionToComplete();
            Thread.Sleep(2000);
            
        }

        

        private static WindowsElement? TryFind(Func<WindowsElement?> finder)
        {
            try { return finder(); } catch { return null; }
        }
    }
}

