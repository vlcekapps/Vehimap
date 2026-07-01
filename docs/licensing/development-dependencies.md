# Development-only dependency notices

Generated: **2026-07-01**

These packages are used by test projects or local development tooling. They are not intended to be shipped in normal end-user Vehimap desktop releases. If a future release ships test tooling, Appium support, screenshots, sample data or other development artifacts, re-check these notices.

| Package | Version observed | License from NuGet metadata | Scope |
|---|---:|---|---|
| Appium.WebDriver | 8.2.0 | Apache-2.0 | UI smoke tests only. |
| Microsoft.CodeCoverage | 17.11.1 | MIT | Transitive test infrastructure. |
| Microsoft.NET.Test.Sdk | 17.11.1 | MIT | Unit/UI test projects. |
| Microsoft.TestPlatform.ObjectModel | 17.11.1 | MIT | Transitive test infrastructure. |
| Microsoft.TestPlatform.TestHost | 17.11.1 | MIT | Transitive test infrastructure. |
| Newtonsoft.Json | 13.0.1 | MIT | Transitive test/Appium/Selenium dependency in test projects. |
| Selenium.WebDriver | 4.36.0 | Apache-2.0 | Transitive UI test dependency. |
| System.Drawing.Common | 8.0.10 | MIT | Transitive UI test dependency. |
| xunit | 2.9.0 | Apache-2.0 | Test framework. |
| xunit.abstractions | 2.0.3 | Apache-2.0 | Transitive xUnit package. |
| xunit.analyzers | 1.15.0 | Apache-2.0 | Test analyzer package. |
| xunit.assert | 2.9.0 | Apache-2.0 | Transitive xUnit package. |
| xunit.core | 2.9.0 | Apache-2.0 | Transitive xUnit package. |
| xunit.extensibility.core | 2.9.0 | Apache-2.0 | Transitive xUnit package. |
| xunit.extensibility.execution | 2.9.0 | Apache-2.0 | Transitive xUnit package. |
| xunit.runner.visualstudio | 2.8.2 | MIT | Test runner package with private assets. |

The current full command output is stored in `dotnet-list-package-include-transitive.txt`.
