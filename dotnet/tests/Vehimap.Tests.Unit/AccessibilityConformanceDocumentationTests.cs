// SPDX-License-Identifier: GPL-3.0-or-later
using Xunit;

namespace Vehimap.Tests.Unit;

public sealed class AccessibilityConformanceDocumentationTests
{
    [Fact]
    public void Accessibility_evidence_documents_define_acr_ready_but_not_certified_status()
    {
        var root = FindRepositoryRoot();
        var accessibilityRoot = Path.Combine(root, "dotnet", "docs", "accessibility");
        var requiredFiles = new[]
        {
            "README.md",
            "acr-vpat-int-draft.md",
            "wcag2ict-22-aa-matrix.md",
            "a11y-remediation-backlog.md",
            "manual-test-protocol.md"
        };

        foreach (var file in requiredFiles)
        {
            Assert.True(File.Exists(Path.Combine(accessibilityRoot, file)), $"Missing accessibility evidence document: {file}");
        }

        var readme = File.ReadAllText(Path.Combine(accessibilityRoot, "README.md"));
        var acrDraft = File.ReadAllText(Path.Combine(accessibilityRoot, "acr-vpat-int-draft.md"));
        var matrix = File.ReadAllText(Path.Combine(accessibilityRoot, "wcag2ict-22-aa-matrix.md"));
        var backlog = File.ReadAllText(Path.Combine(accessibilityRoot, "a11y-remediation-backlog.md"));
        var protocol = File.ReadAllText(Path.Combine(accessibilityRoot, "manual-test-protocol.md"));

        Assert.Contains("ACR-ready evidence draft", readme, StringComparison.Ordinal);
        Assert.Contains("VPAT 2.5Rev INT", readme, StringComparison.Ordinal);
        Assert.Contains("not a legal certification", acrDraft, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("WCAG 2.2", matrix, StringComparison.Ordinal);
        Assert.Contains("WCAG2ICT", matrix, StringComparison.Ordinal);
        Assert.Contains("Section 508", acrDraft, StringComparison.Ordinal);
        Assert.Contains("EN 301 549", acrDraft, StringComparison.Ordinal);
        Assert.Contains("TextBox UIA text fallback", backlog, StringComparison.Ordinal);
        Assert.Contains("Windows 11 + NVDA", protocol, StringComparison.Ordinal);
        Assert.DoesNotContain("certified", readme, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("certified", acrDraft, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "dotnet", "Vehimap.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Repository root was not found.");
    }
}
