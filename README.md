
# AlphaAutomationSolution

A Visual Studio solution using **MSTest**, **Selenium (Chrome)**, and **WinAppDriver** with a **Page Object Model**, **ExtentReports** HTML reporting, **spinner-safe waits**, and **screenshots** on failure.

## How to run
1. Open `AlphaAutomationSolution.sln` in Visual Studio 2022+.
2. Restore NuGet packages.
3. Ensure **WinAppDriver** is installed at:
   `C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe`
4. Update `appsettings.json` as needed (URL, recipients, timeouts, etc.).
5. Run tests from **Test Explorer**.

## Structure
- `Config/` – runtime configuration loader (`appsettings.json`).
- `Helpers/` – Root desktop, Outlook, and Chrome sessions.
- `Pages/Web` – web wizard page (`WebWizardPage`) with spinner-safe clicking.
- `Pages/Outlook` – Outlook compose actions (`OutlookComposePage`).
- `Reporting/` – ExtentReports manager.
- `Utilities/` – `WaitHelper` (spinner-safe), screenshot helpers.
- `Tests/` – `WebToOutlookFlowTests` with an end-to-end scenario.

## Output
- **HTML report**: `Artifacts/Reports/ExtentReport_*.html`
- **Screenshots on failure**: `Artifacts/Screenshots/*.png`

> Email sending is **disabled by default**. To enable, set `"SendEmail": true` in `appsettings.json`.
