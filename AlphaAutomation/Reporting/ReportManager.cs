
using System;
using System.IO;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace AlphaAutomation.Reporting
{
    public static class ReportManager
    {
        private static ExtentReports? _extent;
        private static ExtentHtmlReporter? _html;
        private static readonly string ReportsDir = Path.Combine("Artifacts", "Reports");

        public static void Init(string? reportFileName = null)
        {
            Directory.CreateDirectory(ReportsDir);
            reportFileName ??= $"ExtentReport_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            var reportPath = Path.Combine(ReportsDir, reportFileName);
            _html = new ExtentHtmlReporter(reportPath);
            _extent = new ExtentReports();
            _extent.AttachReporter(_html);
            _extent.AddSystemInfo("Framework", "MSTest");
            _extent.AddSystemInfo("OS", Environment.OSVersion.ToString());
        }

        public static ExtentTest CreateTest(string name)
        {
            if (_extent == null) Init();
            return _extent!.CreateTest(name);
        }

        public static void Flush()
        {
            try { _extent?.Flush(); } catch { }
        }
    }
}
