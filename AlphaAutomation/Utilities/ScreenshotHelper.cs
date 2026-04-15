
using System;
using System.IO;
using OpenQA.Selenium;
using AlphaAutomation.Helpers;

namespace AlphaAutomation.Utilities
{
    public static class ScreenshotHelper
    {
        private static readonly string ScreenshotsDir = Path.Combine("Artifacts", "Screenshots");

        private static string Sanitize(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }

        public static string? CaptureWeb(string baseName)
        {
            if (BaseSession.WebSession == null) return null;
            try
            {
                Directory.CreateDirectory(ScreenshotsDir);
                var ss = ((ITakesScreenshot)BaseSession.WebSession).GetScreenshot();
                string path = Path.Combine(ScreenshotsDir, $"WEB_{Sanitize(baseName)}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                ss.SaveAsFile(path, ScreenshotImageFormat.Png);
                return path;
            }
            catch { return null; }
        }

        public static string? CaptureOutlook(string baseName)
        {
            if (BaseSession.OutlookSession == null) return null;
            try
            {
                Directory.CreateDirectory(ScreenshotsDir);
                var ss = BaseSession.OutlookSession.GetScreenshot();
                string path = Path.Combine(ScreenshotsDir, $"OUTLOOK_{Sanitize(baseName)}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                ss.SaveAsFile(path, ScreenshotImageFormat.Png);
                return path;
            }
            catch { return null; }
        }
    }
}
