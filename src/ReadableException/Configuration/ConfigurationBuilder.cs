namespace ReadableException.Configuration;

public class ConfigurationBuilder
{
    private readonly AnalyzerConfiguration _configuration;

    public ConfigurationBuilder()
    {
        _configuration = new AnalyzerConfiguration();
    }

    public ConfigurationBuilder(AnalyzerConfiguration baseConfiguration)
    {
        _configuration = baseConfiguration;
    }

    public ConfigurationBuilder FilterNamespace(string namespacePrefix)
    {
        _configuration.FilteredNamespaces.Add(namespacePrefix);
        return this;
    }

    public ConfigurationBuilder HighlightNamespace(string namespacePrefix)
    {
        _configuration.HighlightedNamespaces.Add(namespacePrefix);
        return this;
    }

    public ConfigurationBuilder FilterClass(string className)
    {
        _configuration.FilteredClassNames.Add(className);
        return this;
    }

    public ConfigurationBuilder HighlightClass(string className)
    {
        _configuration.HighlightedClassNames.Add(className);
        return this;
    }

    public ConfigurationBuilder WithFilterFrameworkCalls(bool filter)
    {
        _configuration.FilterFrameworkCalls = filter;
        return this;
    }

    public ConfigurationBuilder WithHighlightApplicationCode(bool highlight)
    {
        _configuration.HighlightApplicationCode = highlight;
        return this;
    }

    public ConfigurationBuilder WithMaxStackFrames(int max)
    {
        _configuration.MaxStackFrames = max;
        return this;
    }

    public ConfigurationBuilder WithShowFileInfo(bool show)
    {
        _configuration.ShowFileInfo = show;
        return this;
    }

    public AnalyzerConfiguration Build()
    {
        return _configuration;
    }
}
