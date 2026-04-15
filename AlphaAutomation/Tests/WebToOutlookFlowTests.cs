
using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlphaAutomation.Config;
using AlphaAutomation.Helpers;
using AlphaAutomation.Pages.Outlook;
using AlphaAutomation.Pages.Web;

namespace AlphaAutomation.Tests
{
    [TestClass]
    public class WebToOutlookFlowTests : BaseTest
    {
        [TestMethod]
        [TestCategory("Outlook")]
        [TestCategory("WizardFlow")]
        public void SendNewEmail_FromWebApp_SamePageFormWizard()
        {
            var cfg = RunConfig.Load();

            BaseSession.StartWeb();
            var web = new WebWizardPage(BaseSession.WebSession!, TimeSpan.FromSeconds(cfg.TimeoutSeconds));
            web.GoTo(cfg.BaseUrl);
            Thread.Sleep(1000);
            _test!.Info($"Navigated to {cfg.BaseUrl}");

            web.ClickCreateOutlookEmail();
            _test.Info("Clicked Create Outlook Email");
            web.CompleteWizard(cfg.DescriptionText, cfg.DocumentTypeText, cfg.DocumentTypeValueFallback);
            _test.Info("Completed web wizard form");

            BaseSession.AttachToOutlookComposeWindow(TimeSpan.FromSeconds(cfg.TimeoutSeconds));
            _test.Info("Attached to Outlook compose window");

            var compose = new OutlookComposePage(BaseSession.OutlookSession!);
            compose.ClickPlugin();
            _test.Info("Clicked plugin in Outlook");

            compose.FillEmailFields(cfg.Recipients.To, cfg.Recipients.Cc, cfg.Subject);
            _test.Info("Filled email fields");

            if (cfg.SendEmail)
            {
                compose.ClickSendButton();
                _test.Info("Sent email");
            }

            web.ClickFileCabinetButton();
            _test.Info("Clicked File Cabinet (if present)");

            web.SwitchToNewTab();
            _test.Info("Switched to new tab");

            web.AcceptAlert();
            _test.Info("Accepted alert (if present)");

            web.ClickEmailGrid();
            _test.Info("Clicked email grid");

        }
    }
}

