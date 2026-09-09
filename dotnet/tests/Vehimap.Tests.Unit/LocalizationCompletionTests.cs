// SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Vehimap.Application.Models;
using Vehimap.Application.Services;
using Vehimap.Desktop.Localization;
using Vehimap.Desktop.ViewModels;
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class LocalizationCompletionTests
{
    [Fact]
    public void Desktop_publish_checks_both_language_assemblies()
    {
        var project = XDocument.Load(Path.Combine(RepositoryRoot(), "dotnet", "src", "Vehimap.Desktop", "Vehimap.Desktop.csproj"));
        var target = project.Root!.Elements("Target").Single(element => (string?)element.Attribute("Name") == "VerifyPublishedLanguages");
        Assert.Equal("Publish", (string?)target.Attribute("AfterTargets"));
        var conditions = target.Elements("Error").Select(error => (string?)error.Attribute("Condition")).ToArray();
        Assert.Contains("!Exists('$(PublishDir)Vehimap.Application.dll')", conditions);
        Assert.Contains("!Exists('$(PublishDir)cs-CZ/Vehimap.Application.resources.dll')", conditions);
    }

    [Theory]
    [InlineData("en-US", "Strings.resx")]
    [InlineData("en-GB", "Strings.resx")]
    [InlineData("cs-CZ", "Strings.cs-CZ.resx")]
    [InlineData("cs", "Strings.cs-CZ.resx")]
    [InlineData("de-DE", "Strings.resx")]
    public void Every_compiled_translation_matches_its_source_catalog(string culture, string resourceFile)
    {
        var localizer = new ResourceAppLocalizer(CultureInfo.GetCultureInfo(culture));
        var resources = XDocument.Load(Path.Combine(RepositoryRoot(), "dotnet", "src",
            "Vehimap.Application", "Resources", resourceFile));
        var entries = resources.Root!.Elements("data").ToArray();
        Assert.NotEmpty(entries);
        Assert.Equal(entries.Length, entries.Select(entry => (string)entry.Attribute("name")!).Distinct().Count());
        foreach (var entry in entries)
        {
            var key = (string)entry.Attribute("name")!;
            var expected = (string)entry.Element("value")!;
            Assert.True(expected.ReplaceLineEndings("\n") == localizer.GetString(key).ReplaceLineEndings("\n"),
                $"Compiled translation mismatch: {culture}, {key}");
            // Matching placeholders alone would miss the same malformed format in both catalogs.
            Assert.NotNull(CompositeFormat.Parse(expected));
        }
    }

    [Fact]
    public void System_language_survives_explicit_language_changes_and_new_service_instances()
    {
        var service = new AppCultureService();
        var original = service.ResolveCulture(AppCultureService.SystemLanguage);
        try
        {
            foreach (var language in new[] { "en-US", "cs-CZ", "en-US" })
            {
                service.ApplyThreadCulture(new AppCulturePreferences(language, "culture", "culture"));
                Assert.Equal(original.Name, service.ResolveCulture(AppCultureService.SystemLanguage).Name);
                Assert.Equal(original.Name, new AppCultureService().ResolveCulture(AppCultureService.SystemLanguage).Name);
                Assert.Equal(original.Name, new AppLocaleDefaultsService().GetDefaultsForLanguage("system").Language);
                Assert.Equal(language, AppLocaleDefaultsService.GetCurrentCultureDefaults().Language);
            }

            service.ApplyThreadCulture(new AppCulturePreferences("system", "culture", "culture"));
            Assert.Equal(original.Name, CultureInfo.CurrentUICulture.Name);
        }
        finally
        {
            TestCultureInitializer.ResetToCzech();
        }
    }

    [Theory]
    [InlineData("cs-CZ", DataStoreHealthStatus.Healthy, "Stav: V pořádku")]
    [InlineData("cs-CZ", DataStoreHealthStatus.Warning, "Stav: Upozornění")]
    [InlineData("cs-CZ", DataStoreHealthStatus.Error, "Stav: Chyba")]
    [InlineData("en-US", DataStoreHealthStatus.Healthy, "Status: Healthy")]
    [InlineData("en-US", DataStoreHealthStatus.Warning, "Status: Warning")]
    [InlineData("en-US", DataStoreHealthStatus.Error, "Status: Error")]
    public void Copied_health_diagnostics_localize_status_not_file_paths(string language, DataStoreHealthStatus status, string expected)
    {
        DesktopLocalization.Configure(new AppCulturePreferences(language, "none", "comma"));
        try
        {
            var report = new DataStoreHealthReport(status, "", [], "user-data/vehimap.db", "user-data");
            var model = new DataStoreHealthDialogViewModel(report);
            Assert.Contains(expected, model.ClipboardText, StringComparison.Ordinal);
            Assert.Contains(report.DatabasePath, model.ClipboardText, StringComparison.Ordinal);
        }
        finally
        {
            TestCultureInitializer.ResetToCzech();
        }
    }

    [Fact]
    public void All_xaml_text_surfaces_use_localization_or_binding_not_a_pilot_allowlist()
    {
        var textProperties = new HashSet<string>(StringComparer.Ordinal)
        {
            "Text", "Content", "Header", "Title", "PlaceholderText", "Watermark", "ToolTip.Tip",
            "AutomationProperties.Name", "AutomationProperties.HelpText",
            "AutomationProperties.ItemType", "AutomationProperties.ItemStatus"
        };
        var sourceRoot = Path.Combine(RepositoryRoot(), "dotnet", "src");
        var failures = new List<string>();
        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*.axaml", SearchOption.AllDirectories)
                     .Where(path => !path.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj")))
        {
            var document = XDocument.Load(file, LoadOptions.SetLineInfo);
            foreach (var element in document.Descendants())
            {
                foreach (var attribute in element.Attributes().Where(attribute => textProperties.Contains(attribute.Name.LocalName)))
                {
                    Check(attribute.Value, attribute);
                }

                if (element.Name.LocalName == "Setter" && textProperties.Contains((string?)element.Attribute("Property") ?? ""))
                {
                    Check((string?)element.Attribute("Value") ?? "", element);
                }

                if (element.Name.LocalName is "TextBlock" or "Button" or "CheckBox" or "Label")
                {
                    foreach (var text in element.Nodes().OfType<XText>())
                    {
                        Check(text.Value, element);
                    }
                }
            }

            void Check(string value, XObject node)
            {
                var text = value.Trim();
                // The product name is deliberately identical; punctuation/numbers are not translations.
                if (!text.Any(char.IsLetter) || text == "Vehimap" || (text.StartsWith('{') && !text.StartsWith("{}", StringComparison.Ordinal)))
                {
                    return;
                }
                failures.Add($"{Path.GetRelativePath(sourceRoot, file)}:{((System.Xml.IXmlLineInfo)node).LineNumber}: {text}");
            }
        }
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }

    private static string RepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "dotnet", "Vehimap.sln")))
            {
                return directory.FullName;
            }
        }
        throw new DirectoryNotFoundException("Repository root not found.");
    }
}
