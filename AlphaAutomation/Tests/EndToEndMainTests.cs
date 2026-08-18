using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlphaAutomation.Config;
using AlphaAutomation.Helpers;
using AlphaAutomation.Pages.Outlook;
using AlphaAutomation.Pages.Web;
using System.Threading;
using System.Drawing.Text;
[assembly: Parallelize(Workers = 1, Scope = ExecutionScope.MethodLevel)]
namespace AlphaAutomation.Tests
{


    [TestClass]
    public class EndToEndMainTests : BaseTest
    {

        // Adds a 1-minute gap after each test method in this class.
        // This keeps tests running one-by-one (assembly already sets Workers = 1).
        //[TestCleanup]
        //public void DelayAfterEachTest()
        //{
        //    // Use a blocking sleep so the test runner waits before starting next test.
        //    Thread.Sleep(TimeSpan.FromMinutes(1));
        //}

        [TestMethod]
        [TestCategory("Outlook")]
        [TestCategory("WizardFlow")]

        private void ExecuteEmailScenario(string subject, Action<OutlookComposePage>? attachmentAction = null)

        {
            var cfg = RunConfig.Load();

            // Start Outlook and web session once. Do not retry on failure; allow exceptions to surface to the test runner.
            BaseSession.LaunchOutlook(TimeSpan.FromSeconds(cfg.TimeoutSeconds));
            BaseSession.StartWeb();

            var web = new WebWizardPage(
                BaseSession.WebSession!,
                TimeSpan.FromSeconds(cfg.TimeoutSeconds));

            web.GoTo(cfg.BaseUrl);

            web.ClickCreateOutlookEmail();
            _test!.Info("Create Outlook Email Button Clicked From Submission Page");

            web.CompleteWizard();
            _test.Info("Email Template and Document Type Selected in Wizard");

            BaseSession.AttachToOutlookComposeWindow(
                TimeSpan.FromSeconds(cfg.TimeoutSeconds));

            var compose = new OutlookComposePage(
                BaseSession.OutlookSession!);

            compose.ClickPlugin();
            _test.Info("Correspondence Alpha Plugin Clicked in Outlook Compose Email Page");

            // Scenario-specific action
            attachmentAction?.Invoke(compose);
                compose.FillEmailFields(
                cfg.To,
                subject);

            _test.Info("Recipient Field and Subject Field Filled");

            if (cfg.SendEmail)
            {
                compose.ClickSendButton();
                _test.Info("Send Button Clicked and Email Sent to Recipient");
            }
        }
        private void AmlinkDocsVerification(string subject)
        {
            var cfg = RunConfig.Load();
            BaseSession.StartWeb();
            var web = new WebWizardPage(
                BaseSession.WebSession!,
                TimeSpan.FromSeconds(cfg.TimeoutSeconds));
            web.GoTo(cfg.BaseUrl);
            _test.Info("Submission Page Loaded");

            web.ClickFileCabinetButton();
            _test.Info("File Cabinet Button Clicked");

            web.SwitchToNewTab();
            _test.Info("Amlink Docs Opened in New Tab");

            web.ClickEmailGrid(subject);
            _test.Info("Email Grid Icon Click to Preview");
        }

        

        [TestMethod]
        [TestCategory("Scenario_1")]
        [Priority(1)]
        public void CreateOutlookEmail_WithoutAttachment()
        {
            var cfg = RunConfig.Load();
            var prefix = !string.IsNullOrWhiteSpace(cfg.SubjectScenario1)
                ? cfg.SubjectScenario1
                : (!string.IsNullOrWhiteSpace(cfg.SubjectText) ? cfg.SubjectText : "Dexian Automation Scenario_1");
            var subject = $"{prefix}_{DateTime.Now:yyyyMMddHHmmss}";
            ExecuteEmailScenario(subject);
            AmlinkDocsVerification(subject);
            _test!.Info("Email Verified in Amlink Docs");
        }

        [TestMethod]
        [TestCategory("Scenario_2")]
        [Priority(2)]
        public void CreateOutlookEmail_WithDocumentListAttachment()
        {
            var cfg = RunConfig.Load();
            var prefix = !string.IsNullOrWhiteSpace(cfg.SubjectScenario2)
                ? cfg.SubjectScenario2
                : (!string.IsNullOrWhiteSpace(cfg.SubjectText) ? cfg.SubjectText : "Dexian Automation Scenario_2");
            var subject = $"{prefix}_{DateTime.Now:yyyyMMddHHmmss}";
            ExecuteEmailScenario(
        subject, compose =>
        {
            compose.AttachDocumentFromList();
            _test!.Info("Attached Required Document from Document List");
        });
            AmlinkDocsVerification(subject);
            _test!.Info("Email Verified in Amlink Docs");
        }
        [TestMethod]
        [TestCategory("Scenario_3")]
        [Priority(3)]
        public void CreateOutlookEmail_WithDocumentLibraryAttachment()
        {
            var cfg = RunConfig.Load();
            var prefix = !string.IsNullOrWhiteSpace(cfg.SubjectScenario3)
                ? cfg.SubjectScenario3
                : (!string.IsNullOrWhiteSpace(cfg.SubjectText) ? cfg.SubjectText : "Dexian Automation Scenario_3");
            var subject = $"{prefix}_{DateTime.Now:yyyyMMddHHmmss}";
            ExecuteEmailScenario(
        subject, compose =>
        {
            compose.AttachDocumentfromLibrary();
            _test!.Info("Attached Required Document from Document Library");
        });
            AmlinkDocsVerification(subject);
            _test!.Info("Email Verified in Amlink Docs");
        }
        [TestMethod]
        [TestCategory("Scenario_4")]
        [Priority(4)]
        public void CreateOutlookEmail_WithPCAttachment()
        {
            var cfg = RunConfig.Load();
            var prefix = !string.IsNullOrWhiteSpace(cfg.SubjectScenario4)
                ? cfg.SubjectScenario4
                : (!string.IsNullOrWhiteSpace(cfg.SubjectText) ? cfg.SubjectText : "Dexian Automation Scenario_4");
            var subject = $"{prefix}_{DateTime.Now:yyyyMMddHHmmss}";
            ExecuteEmailScenario(
        subject, compose =>
        {
            compose.AttachDocumentFromPC();
            _test!.Info("Attached Required Document from PC");
        });
            AmlinkDocsVerification(subject);
            _test!.Info("Email Verified in Amlink Docs");
        }
        [TestMethod]
        [TestCategory("Scenario_5")]
        [Priority(5)]
        public void CreateOutlookEmail_ByEditingDocumentType()
        {
            var cfg = RunConfig.Load();
            var prefix = !string.IsNullOrWhiteSpace(cfg.SubjectScenario5)
                ? cfg.SubjectScenario5
                : (!string.IsNullOrWhiteSpace(cfg.SubjectText) ? cfg.SubjectText : "Dexian Automation Scenario_5");
            var subject = $"{prefix}_{DateTime.Now:yyyyMMddHHmmss}";
            ExecuteEmailScenario(
        subject, compose =>
        {
            // Wait for the plugin to load properly
            compose.EditDocumentType();
            _test!.Info("Selected Different Document Type From the Dropdown in Email");
        });
            AmlinkDocsVerification(subject);
            _test!.Info("Email Verified in Amlink Docs");

        }

        [TestMethod]
        [TestCategory("Scenario_6")]
        [Priority(6)]
        public void ImportEmailFromInboxToAmlinkDocsWithAttachment()
        {
            // Capture subject in a local variable so it can be reused later
            var subject = $"Dexian Automation Scenario_6_{DateTime.Now:yyyyMMddHHmmss}";
            ExecuteEmailScenario(subject, compose =>
            {
                compose.AttachMultipleDocumentsFromLibrary();
                _test!.Info("Attached Required Document from Document PC");
            });
            Thread.Sleep(TimeSpan.FromMinutes(1)); // Wait for the email to be sent and received
            var cfg = RunConfig.Load();
            BaseSession.AttachToOutlookMainWindow(subject, TimeSpan.FromSeconds(cfg.TimeoutSeconds));
            var inbox = new OutlookInboxPage(
                BaseSession.OutlookSession!);
            inbox.GoToInbox();
            inbox.OpenEmailBySubject(subject);
            _test!.Info("Email opened from Outlook Inbox");

            inbox.ClickImportAddinButton();
            _test.Info("Imported Email to Amlink Docs");

            inbox.SelectOneAttachmentToImport();
            _test.Info("Selected One Attachment to Import");

            inbox.ClickImportButton();
            _test.Info("Clicked Import Button to Import Email and Attachment to Amlink Docs");
            
            AmlinkDocsVerification(subject);
            _test!.Info("Email Verified in Amlink Docs");

        }

        [TestMethod]
        [TestCategory("Scenario_7")]
        [Priority(7)]
        public void ImportEmailFromInboxToAmlinkDocsWithoutAttachment()
        {
            // Capture subject in a local variable so it can be reused later
            var cfg = RunConfig.Load();
            var prefix = !string.IsNullOrWhiteSpace(cfg.SubjectScenario7)
                ? cfg.SubjectScenario7
                : (!string.IsNullOrWhiteSpace(cfg.SubjectText) ? cfg.SubjectText : "Dexian Automation Scenario_7");
            var subject = $"{prefix}_{DateTime.Now:yyyyMMddHHmmss}";
            ExecuteEmailScenario(subject, compose =>
            {
                compose.AttachMultipleDocumentsFromLibrary();
                _test!.Info("Attached Required Document from Document Library");
            });
            Thread.Sleep(TimeSpan.FromMinutes(1)); // Wait for the email to be sent and received
            
            BaseSession.AttachToOutlookMainWindow(subject, TimeSpan.FromSeconds(10));
            var inbox = new OutlookInboxPage(
                BaseSession.OutlookSession!);
            inbox.GoToInbox();
            inbox.OpenEmailBySubject(subject);
            _test!.Info("Selected Email having valid hook in Outlook Inbox");

            inbox.ClickImportAddinButton();
            _test.Info("Clicked Import Add-in Button");

            inbox.DeSelectAllAttachmentAndImport();
            _test.Info("De-Selected All Attachments from the Email");

            inbox.ClickImportButton();
            _test.Info("Clicked Import Button to Import Email to Amlink Docs");

            AmlinkDocsVerification(subject);
            _test!.Info("Email Verified in Amlink Docs");

        }
    }

}


