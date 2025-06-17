// BlazorToastExtensions.cs
using BlazorToast.Models;
using BlazorToast.Services;
using Microsoft.Extensions.DependencyInjection;

public static class BlazorToastExtensions
{
    public static IServiceCollection AddBlazorToast(this IServiceCollection services)
    {
        return services.AddSingleton<BlazorToastService>();
    }

    // Optional: Add configuration overload
    public static IServiceCollection AddBlazorToast(this IServiceCollection services, Action<ToastGlobalConfig> configure)
    {
        services.AddSingleton<BlazorToastService>();
        services.Configure(configure);
        return services;
    }
}