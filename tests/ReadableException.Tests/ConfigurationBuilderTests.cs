using NUnit.Framework;
using ReadableException.Configuration;

namespace ReadableException.Tests;

[TestFixture]
public class ConfigurationBuilderTests
{
[Test]
    public void BuildCreatesConfiguration()
    {
        AnalyzerConfiguration config = new ConfigurationBuilder().Build();

        Assert.That(config, Is.Not.Null);
    }

[Test]
    public void FilterNamespaceAddsToFilteredNamespaces()
    {
        AnalyzerConfiguration config = new ConfigurationBuilder()
            .FilterNamespace("MyCustom.Framework.")
            .Build();

        Assert.That(config.FilteredNamespaces, Does.Contain("MyCustom.Framework."));
    }

[Test]
    public void HighlightNamespaceAddsToHighlightedNamespaces()
    {
        AnalyzerConfiguration config = new ConfigurationBuilder()
            .HighlightNamespace("MyApp.Core.")
            .Build();

        Assert.That(config.HighlightedNamespaces, Does.Contain("MyApp.Core."));
    }

[Test]
    public void FluentAPIChainsMultipleOperations()
    {
        AnalyzerConfiguration config = new ConfigurationBuilder()
            .FilterNamespace("System.")
            .HighlightNamespace("MyApp.")
            .WithMaxStackFrames(100)
            .WithShowFileInfo(false)
            .Build();

        Assert.That(config.FilteredNamespaces, Does.Contain("System."));
        Assert.That(config.HighlightedNamespaces, Does.Contain("MyApp."));
        Assert.That(config.MaxStackFrames, Is.EqualTo(100));
        Assert.That(config.ShowFileInfo, Is.False);
    }
}
