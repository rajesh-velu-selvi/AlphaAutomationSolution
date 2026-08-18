# AlphaAutomation - Project Setup & Run Guide

This document provides step-by-step instructions for setting up and running the AlphaAutomation test project on a new machine.

---

## 📋 Table of Contents

1. [System Requirements](#system-requirements)
2. [Prerequisites Installation](#prerequisites-installation)
3. [Project Setup](#project-setup)
4. [Configuration](#configuration)
5. [Running Tests](#running-tests)
6. [Project Structure](#project-structure)
7. [Troubleshooting](#troubleshooting)
8. [Key Automation Scenarios](#key-automation-scenarios)

---

## 🖥️ System Requirements

### Hardware
- **OS**: Windows 10 or Windows 11 (Desktop/Server editions)
- **RAM**: Minimum 8 GB (16 GB recommended)
- **Disk Space**: At least 2 GB free

### Software
- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Visual Studio 2022/2026** (Community, Professional, or Enterprise)
  - Or use VS Code with C# extension
- **Microsoft Outlook** (Desktop version - required for testing)
- **Google Chrome** (Latest version)
- **Git** (for cloning the repository)

---

## 🔧 Prerequisites Installation

### Step 1: Install .NET 8 SDK
1. Download from [Microsoft .NET Download Page](https://dotnet.microsoft.com/download/dotnet/8.0)
2. Run the installer and follow the prompts
3. Verify installation:
   ```powershell
   dotnet --version
   # Should output: 8.0.x or higher
   ```

### Step 2: Install Visual Studio 2022/2026
1. Download from [Visual Studio Download](https://visualstudio.microsoft.com/downloads/)
2. Run the installer
3. Select workload: **".NET desktop development"**
4. Include these optional components:
   - .NET 8.0 SDK
   - Test Adapter for MSTest
5. Complete the installation

### Step 3: Install Windows Application Driver (WinAppDriver)
Required for Outlook automation via Appium.

1. Download from [Windows Application Driver GitHub](https://github.com/microsoft/WinAppDriver/releases)
   - Download: `WindowsApplicationDriver_1.2.1.msi` (latest version)
2. Run the installer
3. Default installation path: `C:\Program Files (x86)\Windows Application Driver\`
4. Verify installation - WinAppDriver.exe should be at:
   ```
   C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe
   ```

### Step 4: Install/Verify Chrome Browser
1. Download from [Google Chrome](https://www.google.com/chrome/)
2. Install to default location
3. Verify you have the latest version (automated tests require Chrome driver compatibility)

### Step 5: Install Git
1. Download from [Git Official](https://git-scm.com/download/win)
2. Install with default options
3. Verify installation:
   ```powershell
   git --version
   ```

---

## 📥 Project Setup

### Step 1: Clone the Repository
```powershell
# Navigate to your projects directory
cd C:\Users\YourUsername\MyProjects

# Clone the repository
git clone https://github.com/rajesh-velu-selvi/AlphaAutomationSolution.git

# Navigate to project
cd AlphaAutomationSolution
```

### Step 2: Restore NuGet Packages
Using Visual Studio:
1. Open `AlphaAutomationSolution.sln`
2. Right-click Solution → **Restore NuGet Packages**

OR using PowerShell:
```powershell
cd AlphaAutomationSolution
dotnet restore
```

### Step 3: Build the Project
Using Visual Studio:
1. Build → **Build Solution** (Ctrl + Shift + B)

OR using PowerShell:
```powershell
dotnet build
```

You should see: `Build succeeded` message.

---

## ⚙️ Configuration

### Key Configuration File
The project uses `appsettings.json` for configuration:
```
AlphaAutomation/appsettings.json
```

### Edit Configuration Parameters

1. **Open** `appsettings.json` in Visual Studio
2. **Modify as needed**:

```json
{
  "BaseUrl": "https://your-application-url-here",
  "WinAppDriverPath": "C:\\Program Files (x86)\\Windows Application Driver\\WinAppDriver.exe",
  "TimeoutSeconds": 60,
  "SendEmail": true,
  "SubjectText": "Your Email Subject",
  "To": "recipient@example.com",
  "Subject": "Automation Testing of Scenario",
  "SubjectScenario1": "Automation Scenario_1",
  ...
}
```

### Important Configuration Parameters

| Parameter | Description | Example |
|-----------|-------------|---------|
| `BaseUrl` | Application URL to test | `https://amlinkistpc.amwins.net/...` |
| `TimeoutSeconds` | Wait timeout for elements (in seconds) | `60` |
| `SendEmail` | Whether to send actual emails | `true` or `false` |
| `To` | Email recipient address | `test@example.com` |
| `WinAppDriverPath` | Path to WinAppDriver executable | `C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe` |

### Outlook Setup Requirements

Before running tests, ensure Outlook is configured:
1. Open Microsoft Outlook
2. Ensure at least one email account is configured and synced
3. Keep Outlook running while tests execute (tests automate Outlook)

---

## 🧪 Running Tests

### Method 1: Using Visual Studio Test Explorer (Recommended)

1. Open Visual Studio
2. Go to **Test** → **Test Explorer** (or Ctrl + E, T)
3. Tests will appear in the Explorer panel
4. Select tests to run:
   - Click a test method name and press **Ctrl + R**
   - Or right-click and select **Run Test(s)**
5. View results in **Test Explorer** panel

### Method 2: Run All Tests
```powershell
cd AlphaAutomationSolution
dotnet test
```

### Method 3: Run Specific Test
```powershell
# Run a specific test by name
dotnet test --filter "FullyQualifiedName~ImportEmailFromInboxToAmlinkDocsWithAttachment"
```

### Method 4: Using Test Logger
```powershell
# Run with detailed logging
dotnet test --logger "console;verbosity=detailed"
```

### Test Categories Available
Tests are organized by category. You can filter by category:
```powershell
dotnet test --filter "TestCategory=Scenario_6"
```

Available categories:
- `Outlook` - Outlook-related tests
- `WizardFlow` - Wizard workflow tests
- `Scenario_1` through `Scenario_7` - Specific scenarios

---

## 📁 Project Structure

```
AlphaAutomationSolution/
├── AlphaAutomation/
│   ├── Config/
│   │   └── RunConfig.cs                 # Configuration class
│   ├── Helpers/
│   │   └── BaseSession.cs               # Session management
│   ├── Pages/
│   │   ├── Outlook/
│   │   │   ├── OutlookComposePage.cs    # Outlook compose UI
│   │   │   └── OutlookInboxPage.cs      # Outlook inbox UI
│   │   └── Web/
│   │       └── WebWizardPage.cs         # Web wizard UI
│   ├── Reporting/
│   │   └── ReportManager.cs             # Test reporting
│   ├── Tests/
│   │   ├── BaseTest.cs                  # Base test class
│   │   └── EndToEndMainTests.cs         # Main test scenarios
│   ├── Utilities/
│   │   ├── ScreenshotHelper.cs          # Screenshot capture
│   │   └── WaitHelper.cs                # Wait utilities
│   ├── appsettings.json                 # Configuration file
│   └── AlphaAutomation.csproj           # Project file
│
├── AlphaAutomationSolution.sln          # Solution file
├── SETUP_GUIDE.md                       # This file
└── README.md                            # Project overview
```

### Key Classes

| Class | Purpose |
|-------|---------|
| `RunConfig` | Loads and stores configuration settings |
| `BaseSession` | Manages Selenium/Appium sessions |
| `BaseTest` | Base class for all test methods |
| `OutlookInboxPage` | Page object for Outlook Inbox |
| `OutlookComposePage` | Page object for Outlook Compose |
| `WebWizardPage` | Page object for Web Wizard |
| `ReportManager` | Generates test reports |

---

## 🔍 Key Automation Scenarios

### Scenario 6: Import Email with Attachment
- **Test Method**: `ImportEmailFromInboxToAmlinkDocsWithAttachment`
- **What it does**:
  1. Creates and sends email with attachment via web wizard
  2. Waits 1 minute for email delivery
  3. Opens email in Outlook Inbox
  4. Imports email to Amlink Docs
  5. Selects attachment for import
  6. Verifies email and attachment in Amlink Docs

### Scenario 7: Import Email without Attachment
- **Test Method**: `ImportEmailFromInboxToAmlinkDocsWithoutAttachment`
- **Similar to Scenario 6 but without attachments**

---

## 🐛 Troubleshooting

### Issue: "Chrome failed to start: crashed"
**Solution**:
1. Update Chrome browser to latest version
2. Update Selenium.WebDriver.ChromeDriver NuGet package
3. Delete ChromeDriver cache: `%TEMP%\ChromeDriver`
4. Restart Visual Studio and run tests again

### Issue: "Windows Application Driver not found"
**Solution**:
1. Verify WinAppDriver is installed at: `C:\Program Files (x86)\Windows Application Driver\WinAppDriver.exe`
2. Update `WinAppDriverPath` in `appsettings.json` to match your installation
3. Start WinAppDriver manually before running tests (usually runs automatically)

### Issue: "Outlook process not found"
**Solution**:
1. Ensure Microsoft Outlook is installed (not Outlook web)
2. Start Outlook before running tests
3. Ensure at least one email account is configured in Outlook
4. Check `BaseSession.LaunchOutlook()` timeout setting

### Issue: "Element is not pointer- or keyboard interactable"
**Solution**:
1. Increase `TimeoutSeconds` in `appsettings.json`
2. Ensure UI elements have loaded properly
3. Check browser window size (may be hidden off-screen)
4. Scroll to element before clicking (included in latest code fix)

### Issue: "element not found" errors
**Solution**:
1. Verify the element XPath/selector is correct
2. Increase timeout value or add explicit waits
3. Check if UI has changed in the application
4. Use browser DevTools (F12) to inspect element locators

### Issue: Tests hang or timeout
**Solution**:
1. Check if external applications are responsive (Outlook, Chrome)
2. Verify internet connection (required for web testing)
3. Check for modal dialogs blocking interaction
4. Review test logs for specific failure point
5. Increase `TimeoutSeconds` if needed

---

## 🚀 Quick Start Checklist

- [ ] .NET 8 SDK installed
- [ ] Visual Studio 2022/2026 installed
- [ ] Windows Application Driver installed
- [ ] Chrome browser installed
- [ ] Repository cloned
- [ ] NuGet packages restored (`dotnet restore`)
- [ ] Project builds successfully (`dotnet build`)
- [ ] `appsettings.json` configured with your settings
- [ ] Outlook installed and configured
- [ ] Run first test via Test Explorer

---

## 📞 Support & Debugging

### Enable Detailed Logging
For more verbose test output, modify test execution:
1. In Visual Studio: **Test** → **Test Settings** → **Configure Run Settings**
2. Or add to `appsettings.json`:
   ```json
   "EnableDetailedLogging": true
   ```

### View Test Results
Results appear in:
- **Test Explorer** (Visual Studio)
- **Output Window** (View → Output)
- **Test Results Window** (Test → Windows → Test Results)

### Screenshots & Reports
- Screenshots are saved to: `bin/Debug/Screenshots/`
- Test reports are generated automatically in test output directory

---

## 📝 Notes for New Team Members

1. **First Run**: The first test run may take 5-10 minutes as resources load
2. **Email Testing**: Tests create real emails; have a test email account
3. **Parallel Execution**: Tests run sequentially (not in parallel) - see `appsettings.json`
4. **Screenshots**: Failed tests automatically capture screenshots for debugging
5. **Git**: Always pull latest code before running tests:
   ```powershell
   git pull origin master
   ```

---

## 📚 Additional Resources

- [Selenium Documentation](https://www.selenium.dev/documentation/)
- [Appium Documentation](http://appium.io/docs/en/about-appium/intro/)
- [Windows Application Driver GitHub](https://github.com/microsoft/WinAppDriver)
- [.NET 8 Documentation](https://learn.microsoft.com/en-us/dotnet/fundamentals/)
- [MSTest Documentation](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mstest)

---

**Last Updated**: January 2025
**Project Framework**: .NET 8
**Test Framework**: MSTest
**UI Automation**: Selenium WebDriver + Appium
