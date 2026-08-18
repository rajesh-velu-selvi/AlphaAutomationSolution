# AlphaAutomationSolution - Step-by-step setup and run

This document provides step-by-step instructions to set up the development environment, run tests, and publish artifacts for the AlphaAutomationSolution repository.

Target environment assumptions
- OS: Windows 10/11 (desktop automation and WinAppDriver require Windows)
- .NET: .NET 8 SDK installed
- IDE: Visual Studio 2022/2026 (recommended) or VS Code with C# extensions

1) Clone repository

PowerShell:

git clone https://github.com/rajesh-velu-selvi/AlphaAutomationSolution.git
cd AlphaAutomationSolution

2) Install prerequisites

- Install .NET 8 SDK: https://dotnet.microsoft.com/download
- Install Visual Studio with .NET workload (or VS Code + C# extension)
- Install Chrome browser (for Selenium tests)
- Install WinAppDriver (if you will run desktop/Outlook tests):
  - Download from https://github.com/microsoft/WinAppDriver/releases
  - Recommended install location: C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe
- Install ChromeDriver that matches your Chrome version, or configure the test helpers to download/manage it automatically
- If tests use Outlook automation, ensure Microsoft Outlook is installed and configured for the test account

3) Open solution and restore packages

- Open `AlphaAutomationSolution.sln` in Visual Studio and allow NuGet to restore, or run from PowerShell:

dotnet restore AlphaAutomationSolution.sln

4) Configure runtime settings

- Locate `appsettings.json` or configuration files in the `Config/` folder (if present).
- Update values such as test URL, timeouts, email recipients, SendEmail flag, and paths to external tools (WinAppDriver, ChromeDriver) if required.

Example: ensure WinAppDriver path is correct in your config or environment.

5) Start required external services

- WinAppDriver: run it before running desktop tests:

"C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe"

- ChromeDriver: ensure it is available on PATH or in the test project's expected location.

6) Build the solution

From PowerShell at repo root:

dotnet build AlphaAutomationSolution.sln -c Release

Or build inside Visual Studio (Build -> Build Solution).

7) Run tests

- From Visual Studio: open Test Explorer and run the tests you need (recommended for debugging UI flows).
- From PowerShell: run all tests

dotnet test AlphaAutomationSolution.sln -c Release

Notes:
- UI tests may require running one test at a time or disabling parallelization due to shared UI state.
- Make sure WinAppDriver is running and Chrome/Outlook are available before running UI tests.

8) View test artifacts and reports

- After test run, open the HTML report(s) located in `Artifacts/Reports/` (e.g., ExtentReport_*.html).
- View screenshots in `Artifacts/Screenshots/` for any failed tests.

9) Publish an executable (self-contained) if you want to run without installing .NET

Warning: publishing a self-contained app bundles the .NET runtime but does NOT include external apps like Chrome, WinAppDriver, or Outlook. Those must still be installed on the target machine.

Example: publish a single-file Windows x64 executable (change project path to the desired startup project):

dotnet publish path\to\Your.Startup.Project.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishTrimmed=false -o ./publish

After publishing, copy the entire `publish` folder to the target machine. Ensure external dependencies are installed there.

10) Packaging and installer recommendations

- For distributing to non-developer machines, create an installer (MSI or installer tool like WiX, Inno Setup, or MSIX) that:
  - Installs your published files
  - Verifies/installs prerequisites (WinAppDriver, Chrome, ChromeDriver, Outlook)
  - Creates shortcuts and configures service/user permissions as needed

11) Troubleshooting common issues

- "WinAppDriver connection refused" — ensure WinAppDriver.exe is running and listening on default ports (4723). Check firewall rules.
- "ChromeDriver mismatch" — ensure ChromeDriver version matches installed Chrome. Update driver or Chrome accordingly.
- "Tests fail only in CI" — check for missing prerequisites, display/session issues (headless vs interactive), and environment variables.
- "Missing appsettings.json" — verify the file is included in publish output (set CopyToOutputDirectory if needed).

12) CI/CD notes (brief)

- In CI, install .NET SDK, Chrome, and a compatible ChromeDriver. Running WinAppDriver and Outlook in CI is typically not supported unless you have interactive Windows runners.
- Prefer running UI tests on dedicated Windows VMs or Azure DevOps self-hosted Windows agents with desktop access.

