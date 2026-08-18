
using System;
using System.IO;
using System.Text.Json;

namespace AlphaAutomation.Config
{
    /// <summary>
    /// Configuration for test execution. Values can be overridden via appsettings.json.
    /// </summary>
    public class RunConfig
    {
        /// <summary>
        /// Base URL of the application under test
        /// </summary>
        public string BaseUrl { get; set; } = "https://amlinkistpc.amwins.net/Submission/SUB_SubmissionFileDetails.aspx?SUBFILEID=15093384";

        /// <summary>
        /// Path to Windows Application Driver executable for Outlook automation
        /// </summary>
        public string WinAppDriverPath { get; set; } = @"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe";

        /// <summary>
        /// Maximum seconds to wait for element interactions
        /// </summary>
        public int TimeoutSeconds { get; set; } = 60;

        /// <summary>
        /// Whether to actually send test emails (true) or skip (false)
        /// </summary>
        public bool SendEmail { get; set; } = false;

        /// <summary>
        /// Email recipient address for test emails
        /// </summary>
        public string To { get; set; } = "rajesh.velu.selvi@amwins.com";

        /// <summary>
        /// Email subject prefix for Scenario 1
        /// </summary>
        public string SubjectScenario1 { get; set; } = "Dexian Automation Scenario_1";

        /// <summary>
        /// Email subject prefix for Scenario 2
        /// </summary>
        public string SubjectScenario2 { get; set; } = "Dexian Automation Scenario_2";

        /// <summary>
        /// Email subject prefix for Scenario 3
        /// </summary>
        public string SubjectScenario3 { get; set; } = "Dexian Automation Scenario_3";

        /// <summary>
        /// Email subject prefix for Scenario 4
        /// </summary>
        public string SubjectScenario4 { get; set; } = "Dexian Automation Scenario_4";

        /// <summary>
        /// Email subject prefix for Scenario 5
        /// </summary>
        public string SubjectScenario5 { get; set; } = "Dexian Automation Scenario_5";

        /// <summary>
        /// Email subject prefix for Scenario 6
        /// </summary>
        public string SubjectScenario6 { get; set; } = "Dexian Automation Scenario_6";

        /// <summary>
        /// Email subject prefix for Scenario 7
        /// </summary>
        public string SubjectScenario7 { get; set; } = "Dexian Automation Scenario_7";

        /// <summary>
        /// Generic email subject text used as a fallback when scenario-specific subjects are not set
        /// </summary>
        public string SubjectText { get; set; } = "Dexian Automation";

        /// <summary>
        /// Loads configuration from appsettings.json if it exists, otherwise uses default values
        /// </summary>
        public static RunConfig Load()
        {
            try
            {
                var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    var cfg = JsonSerializer.Deserialize<RunConfig>(json);
                    return cfg ?? new RunConfig();
                }
            }
            catch { }
            return new RunConfig();
        }
    }
}
