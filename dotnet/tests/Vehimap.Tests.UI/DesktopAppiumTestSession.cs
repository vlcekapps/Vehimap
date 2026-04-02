using System.Net.Http;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace Vehimap.Tests.UI;

internal sealed class DesktopAppiumTestSession : IDisposable
{
    private readonly WindowsDriver _driver;
    private readonly string? _temporaryAppRoot;

    private DesktopAppiumTestSession(WindowsDriver driver, string? temporaryAppRoot)
    {
        _driver = driver;
        _temporaryAppRoot = temporaryAppRoot;
    }

    public static bool TryStart(out DesktopAppiumTestSession? session, out string reason)
    {
        session = null;
        if (!DesktopUiTestConfiguration.TryCreate(out var configuration, out reason))
        {
            return false;
        }

        try
        {
            var isolatedLaunch = CreateIsolatedLaunchCopy(configuration.AppPath);

            var options = new AppiumOptions();
            options.PlatformName = "Windows";
            options.AutomationName = "Windows";
            options.AddAdditionalAppiumOption("app", isolatedLaunch.AppPath);
            options.AddAdditionalAppiumOption("deviceName", "WindowsPC");
            options.AddAdditionalAppiumOption("ms:waitForAppLaunch", 15);

            var driver = new WindowsDriver(configuration.ServerUri, options, configuration.CommandTimeout);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            session = new DesktopAppiumTestSession(driver, isolatedLaunch.RootPath);
            session.WaitForElementByAccessibilityId("VehicleListBox");
            return true;
        }
        catch (Exception ex)
        {
            reason = ex.Message;
            session?.Dispose();
            session = null;
            return false;
        }
    }

    public IWebElement WaitForElementByAccessibilityId(string automationId, int timeoutSeconds = 12)
    {
        return WaitUntil(
            () => _driver.FindElement(MobileBy.AccessibilityId(automationId)),
            timeoutSeconds);
    }

    public void ClickByAccessibilityId(string automationId, int timeoutSeconds = 12)
    {
        WaitForElementByAccessibilityId(automationId, timeoutSeconds).Click();
    }

    public IWebElement WaitForElementByName(string name, int timeoutSeconds = 12)
    {
        return WaitUntil(
            () => _driver.FindElement(By.Name(name)),
            timeoutSeconds);
    }

    public string GetNameByAccessibilityId(string automationId, int timeoutSeconds = 12)
    {
        return WaitForElementByAccessibilityId(automationId, timeoutSeconds).GetAttribute("Name") ?? string.Empty;
    }

    public void SendKeysByAccessibilityId(string automationId, string text, int timeoutSeconds = 12)
    {
        WaitForElementByAccessibilityId(automationId, timeoutSeconds).SendKeys(text);
    }

    public void WaitForElementToDisappearByAccessibilityId(string automationId, int timeoutSeconds = 12)
    {
        WaitUntilMissing(
            () => _driver.FindElements(MobileBy.AccessibilityId(automationId)).Any(element => element.Displayed),
            timeoutSeconds);
    }

    public void Dispose()
    {
        try
        {
            _driver.Quit();
        }
        catch
        {
        }

        if (!string.IsNullOrWhiteSpace(_temporaryAppRoot))
        {
            try
            {
                Directory.Delete(_temporaryAppRoot, true);
            }
            catch
            {
            }
        }
    }

    private static (string AppPath, string RootPath) CreateIsolatedLaunchCopy(string sourceAppPath)
    {
        var sourceRoot = Path.GetDirectoryName(sourceAppPath)
            ?? throw new InvalidOperationException("Nelze určit zdrojovou složku publish buildu.");
        var targetRoot = Path.Combine(Path.GetTempPath(), "vehimap-appium", Guid.NewGuid().ToString("N"));

        CopyDirectory(sourceRoot, targetRoot);
        SeedPortableData(Path.Combine(targetRoot, "data"));

        return (Path.Combine(targetRoot, Path.GetFileName(sourceAppPath)), targetRoot);
    }

    private static void CopyDirectory(string sourceRoot, string targetRoot)
    {
        Directory.CreateDirectory(targetRoot);

        foreach (var directory in Directory.GetDirectories(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, directory);
            Directory.CreateDirectory(Path.Combine(targetRoot, relative));
        }

        foreach (var file in Directory.GetFiles(sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceRoot, file);
            var destination = Path.Combine(targetRoot, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination, true);
        }
    }

    private static void SeedPortableData(string dataPath)
    {
        if (Directory.Exists(dataPath))
        {
            Directory.Delete(dataPath, true);
        }

        Directory.CreateDirectory(dataPath);

        File.WriteAllText(Path.Combine(dataPath, "vehicles.tsv"), """
# Vehimap data v4
veh_1	Milena	Osobní vozidla	Rodinné auto	Škoda 120L	1AB2345	1988	43		08/2026	05/2025	06/2026
""");

        File.WriteAllText(Path.Combine(dataPath, "vehicle_meta.tsv"), """
# Vehimap meta v2
veh_1	Běžný provoz		benzín			
""");

        File.WriteAllText(Path.Combine(dataPath, "history.tsv"), "# Vehimap history v1\n");
        File.WriteAllText(Path.Combine(dataPath, "fuel.tsv"), "# Vehimap fuel v1\n");
        File.WriteAllText(Path.Combine(dataPath, "records.tsv"), "# Vehimap records v2\n");
        File.WriteAllText(Path.Combine(dataPath, "reminders.tsv"), "# Vehimap reminders v2\n");
        File.WriteAllText(Path.Combine(dataPath, "maintenance.tsv"), "# Vehimap maintenance v1\n");
        File.WriteAllText(Path.Combine(dataPath, "settings.ini"), """
[app]
technical_reminder_days=30
green_card_reminder_days=30
maintenance_reminder_days=21
maintenance_reminder_km=1000
show_dashboard_on_launch=0
""");
    }

    private static IWebElement WaitUntil(Func<IWebElement> factory, int timeoutSeconds)
    {
        var timeoutAt = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        Exception? lastError = null;

        while (DateTime.UtcNow < timeoutAt)
        {
            try
            {
                var element = factory();
                if (element.Displayed)
                {
                    return element;
                }
            }
            catch (Exception ex) when (ex is WebDriverException or InvalidOperationException)
            {
                lastError = ex;
            }

            Thread.Sleep(250);
        }

        throw new TimeoutException("Požadovaný UI prvek se v Appium session neobjevil.", lastError);
    }

    private static void WaitUntilMissing(Func<bool> predicate, int timeoutSeconds)
    {
        var timeoutAt = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < timeoutAt)
        {
            try
            {
                if (!predicate())
                {
                    return;
                }
            }
            catch (Exception ex) when (ex is WebDriverException or InvalidOperationException)
            {
                return;
            }

            Thread.Sleep(250);
        }

        throw new TimeoutException("Požadovaný UI prvek nezmizel ve stanoveném čase.");
    }
}
