using System.Globalization;
using Microsoft.Extensions.Localization;

namespace TaskFlow.Localization;

public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly string _resourcesPath;

    public JsonStringLocalizerFactory(IHostEnvironment env)
    {
        _resourcesPath = Path.Combine(env.ContentRootPath, "Resources");
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        return new JsonStringLocalizer(_resourcesPath, culture);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        return new JsonStringLocalizer(_resourcesPath, culture);
    }
}
