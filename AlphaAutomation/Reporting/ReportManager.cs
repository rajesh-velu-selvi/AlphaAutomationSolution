using System;
using System.Diagnostics;
using System.IO;
using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;

namespace AlphaAutomation.Reporting
{
    public static class ReportManager
    {
        private static ExtentReports? _extent;  
        private static ExtentHtmlReporter? _html;
        private static string? _reportPath;

        public static void Init(string? reportFileName = null)
        {
            // Test output directory

            string reportsDir = @"C:\Users\901944\MyProjects\AlphaAutomationSolution\Reports";

            Directory.CreateDirectory(reportsDir);


            reportFileName ??=
                $"ExtentReport_{DateTime.Now:yyyyMMdd_HHmmss}.html";

            _reportPath = Path.Combine(reportsDir, reportFileName);

            _html = new ExtentHtmlReporter(_reportPath);

            _extent = new ExtentReports();
            _extent.AttachReporter(_html);

            _extent.AddSystemInfo("Framework", "MSTest");
            _extent.AddSystemInfo("OS", Environment.OSVersion.ToString());

            Console.WriteLine($"Extent Report Path: {_reportPath}");
        }

        public static ExtentTest CreateTest(string name)
        {
            if (_extent == null)
                Init();

            return _extent!.CreateTest(name);
        }

        public static void Flush()
        {
            try
            {
                _extent?.Flush();

                if (!string.IsNullOrEmpty(_reportPath) &&
                    File.Exists(_reportPath))
                {
                    Console.WriteLine($"Opening Report: {_reportPath}");

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _reportPath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Error while flushing/opening report: {ex.Message}");
            }
        }

        public static string? GetReportPath()
        {
            return _reportPath;
        }
    }
}