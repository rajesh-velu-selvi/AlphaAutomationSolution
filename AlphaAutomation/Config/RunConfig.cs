
using System;
using System.IO;
using System.Text.Json;

namespace AlphaAutomation.Config
{
    public class RunConfig
    {
        public string BaseUrl { get; set; } = "https://amlinkistpc.amwins.net/Submission/SUB_SubmissionFileDetails.aspx?SUBFILEID=14112217";
        public string WinAppDriverPath { get; set; } = @"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe";
        public int TimeoutSeconds { get; set; } = 60;
        public bool SendEmail { get; set; } = false;
        public string DescriptionText { get; set; } = "Dexian POC Testing";
        public string DocumentTypeText { get; set; } = "ACT - Accounting";
        public string DocumentTypeValueFallback { get; set; } = "148";
        public RecipientsConfig Recipients { get; set; } = new RecipientsConfig();
        public string Subject { get; set; } = "This Email is being sent for the purpose of Automation Testing";

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

    public class RecipientsConfig
    {
        public string To { get; set; } = "rajesh.velu.selvi@amwins.com";
        public string Cc { get; set; } = "rajesh.velu.selvi@amwins.com";
    }
}
