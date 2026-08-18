
# AlphaAutomationSolution

AlphaAutomationSolution is a .NET 8 solution that contains automation services, UI/web automation helpers, and automated tests used for end-to-end and unit-level validation of the Alpha Automation platform.

This repository is organized for Visual Studio development and includes tests that use MSTest, Selenium (Chrome), and WinAppDriver in a Page Object Model style with HTML reporting and screenshot capture on failure.

## Table of contents
- About
- Features
- Solution structure
- Prerequisites
- Build
- Run
- Tests
- Reporting & artifacts
- Configuration
- Contributing
- License
- Contact

## About

The solution is implemented using .NET 8 and intended to be opened and developed using Visual Studio (recommended) or the .NET CLI. It includes automation helpers for desktop (WinAppDriver/Outlook) and web (Selenium/Chrome) test flows and a test suite for end-to-end scenarios.

## Features

- Page Object Model based test design
- MSTest test projects with end-to-end scenarios
- Selenium WebDriver (Chrome) and WinAppDriver integration
- ExtentReports HTML reporting and screenshots on failures
- Spinner-safe wait helpers to improve stability of UI interactions

## Solution structure (typical)

- `AlphaAutomationSolution.sln` — solution file
- `Config/` — runtime configuration (`appsettings.json`)
- `Helpers/` — session and driver helpers (Chrome, Outlook, desktop)
- `Pages/` — Page Object Model classes for web and desktop pages
- `Reporting/` — ExtentReports manager and reporter utilities
- `Utilities/` — wait helpers, screenshot utilities
- `AlphaAutomation.Tests/` (or `Tests/`) — automated test projects; contains classes such as `EndToEndMainTests.cs`
- `Artifacts/` — generated reports and screenshots (HTML, PNG)

Use Solution Explorer or `dotnet sln list` to inspect exact project names and locations in this repository.

## Prerequisites

- .NET 8 SDK (https://dotnet.microsoft.com)
- Visual Studio 2022/2026 (recommended) or Visual Studio Code with C# extensions
- Git
- Chrome browser (for Selenium tests)
- WinAppDriver (if running desktop/Outlook automation):
  `C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe`

Optional:
- dotnet-format for code formatting

## Build

Build the entire solution from repository root:

dotnet build AlphaAutomationSolution.sln

Or open `AlphaAutomationSolution.sln` in Visual Studio and build from the IDE.

## Run

To run a specific project via CLI:

dotnet run --project path/to/Your.Startup.Project.csproj

To run tests from Visual Studio, open Test Explorer and run tests or use the .NET CLI as described below.

## Tests

Run all tests from the solution root:

dotnet test AlphaAutomationSolution.sln

Run a single test project:

dotnet test path/to/testproject.csproj

Test files are typically located under the test project (for example `AlphaAutomation.Tests/EndToEndMainTests.cs`). Make sure required test dependencies (WinAppDriver, ChromeDriver) are present and configured before running UI tests.

## Reporting & artifacts

HTML reports and screenshots are typically written to the `Artifacts/` directory. Example outputs:

- `Artifacts/Reports/ExtentReport_*.html`
- `Artifacts/Screenshots/*.png`

Open the generated HTML report in a browser after a test run to review results.

## Configuration

Update `appsettings.json` (or other configuration files in `Config/`) to control URLs, timeouts, email settings, and other runtime parameters.

Note: Email sending is often disabled by default. To enable, change the `SendEmail` setting in the configuration to `true` if present.

## Contributing

Suggested workflow:

1. Fork the repository and create a branch: `git checkout -b feat/your-feature`
2. Make focused commits and keep changes small
3. Run tests locally and update them if needed
4. Push branch and open a pull request against `master`

Coding conventions:
- Follow repository `.editorconfig` if present
- Prefer small, test-covered changes

## License

See the `LICENSE` file in the repository root if present. If there is no license file, contact the repository owner before reusing code externally.

## Contact

Repository: https://github.com/rajesh-velu-selvi/AlphaAutomationSolution

For questions, open an issue on GitHub or contact the repository owner.

---

If you want the README adapted to list every project, the recommended startup project, or CI steps, tell me which details to include and I will update this README accordingly.

