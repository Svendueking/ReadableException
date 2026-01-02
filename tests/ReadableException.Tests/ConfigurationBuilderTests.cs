using ReadableException.Configuration;
using Xunit;

namespace ReadableException.Tests;

public class ConfigurationBuilderTests
{
    [Fact]
    public void Build_CreatesConfiguration()
    {
        var config = new ConfigurationBuilder().Build();

        Assert.NotNull(config);
    }

    [Fact]
    public void FilterNamespace_AddsToFilteredNamespaces()
    {
        var config = new ConfigurationBuilder()
            .FilterNamespace("MyCustom.Framework.")
            .Build();

        Assert.Contains("MyCustom.Framework.", config.FilteredNamespaces);
    }

    [Fact]
    public void HighlightNamespace_AddsToHighlightedNamespaces()
    {
        var config = new ConfigurationBuilder()
            .HighlightNamespace("MyApp.Core.")
            .Build();

        Assert.Contains("MyApp.Core.", config.HighlightedNamespaces);
    }

    [Fact]
    public void FluentAPI_ChainsMultipleOperations()
    {
        var config = new ConfigurationBuilder()
            .FilterNamespace("System.")
            .HighlightNamespace("MyApp.")
            .WithMaxStackFrames(100)
            .WithShowFileInfo(false)
            .Build();

        Assert.Contains("System.", config.FilteredNamespaces);
        Assert.Contains("MyApp.", config.HighlightedNamespaces);
        Assert.Equal(100, config.MaxStackFrames);
        Assert.False(config.ShowFileInfo);
    }
}
