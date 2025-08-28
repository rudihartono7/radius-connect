using Microsoft.AspNetCore.SpaServices;

namespace RadiusConnect.Api;
public static class SpaHelper
{
    private static int Port => 3000;
    private static Uri DevelopmentServerEndpoint { get; } = new Uri($"http://localhost:{Port}");

    public static void UseNuxtDevelopmentServer(this ISpaBuilder spa)
    {
        spa.UseProxyToSpaDevelopmentServer(() =>
        {
            return Task.FromResult(DevelopmentServerEndpoint);
        });
    }
}