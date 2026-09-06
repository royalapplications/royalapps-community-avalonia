using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(RoyalApps.Community.Avalonia.Common.Tests.TestApplicationBuilder))]
[assembly: AvaloniaTestFramework]

namespace RoyalApps.Community.Avalonia.Common.Tests;

internal sealed class TestApplication : Application
{
}

public static class TestApplicationBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure<TestApplication>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
