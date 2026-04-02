using System.Net.Http;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace Vehimap.Tests.UI;

internal sealed class DesktopAppiumTestSession : IDisposable
{
    private readonly WindowsDriver _driver;

    private DesktopAppiumTestSession(WindowsDriver driver)
    {
        _driver = driver;
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
            var options = new AppiumOptions();
            options.PlatformName = "Windows";
            options.AutomationName = "Windows";
            options.AddAdditionalAppiumOption("app", configuration.AppPath);
            options.AddAdditionalAppiumOption("deviceName", "WindowsPC");
            options.AddAdditionalAppiumOption("ms:waitForAppLaunch", 15);

            var driver = new WindowsDriver(configuration.ServerUri, options, configuration.CommandTimeout);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            session = new DesktopAppiumTestSession(driver);
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

    public void Dispose()
    {
        try
        {
            _driver.Quit();
        }
        catch
        {
        }
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
}
