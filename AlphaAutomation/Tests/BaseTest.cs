using System;
using System.IO;
using AventStack.ExtentReports;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AlphaAutomation.Config;
using AlphaAutomation.Helpers;
using AlphaAutomation.Reporting;
using AlphaAutomation.Utilities;

namespace AlphaAutomation.Tests
{
    [TestClass]
    public class BaseTest
    {
        protected ExtentTest? _test;
        public TestContext? TestContext { get; set; }

        [AssemblyInitialize]
        public static void ClassInit(TestContext context)
        {
            var cfg = RunConfig.Load();
            BaseSession.StartRootDesktop(cfg.WinAppDriverPath);
            ReportManager.Init();
        }

        [AssemblyCleanup]
        public static void ClassCleanup()
        {
            BaseSession.QuitAll();
            ReportManager.Flush();
        }

        [TestInitialize]
        public void TestInit()
        {
            _test = ReportManager.CreateTest(TestContext!.TestName);
        }

        [TestCleanup]
        public void TestClean()
        {
            var outcome = TestContext!.CurrentTestOutcome;
            if (outcome != UnitTestOutcome.Passed)
            {
                var webPath = ScreenshotHelper.CaptureWeb(TestContext.TestName + "_web");
                var outPath = ScreenshotHelper.CaptureOutlook(TestContext.TestName + "_outlook");
                if (!string.IsNullOrEmpty(webPath))
                {
                    TestContext.AddResultFile(webPath);
                    _test?.AddScreenCaptureFromPath(Path.GetFullPath(webPath));
                }
                if (!string.IsNullOrEmpty(outPath))
                {
                    TestContext.AddResultFile(outPath);
                    _test?.AddScreenCaptureFromPath(Path.GetFullPath(outPath));
                }
                _test?.Fail($"Test failed: {outcome}");
            }
            else
            {
                _test?.Pass("Test passed");
            }
        }
    }
}

