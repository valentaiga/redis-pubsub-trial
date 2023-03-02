using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using RedisPubSub.Common.Options;

namespace RedisPubSub.Common.Extensions;

public static class AppConfigurationExtensions
{
    public static void ConfigureOpenTelemetry(this IServiceCollection services, IConfiguration configuration, string configSectionPath)
    {
        var otlmSettings = configuration.GetSection(configSectionPath).Get<OpenTelemetryConfig>()!;
        
        services.AddOptions<OpenTelemetryConfig>()
            .BindConfiguration(configSectionPath)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddOpenTelemetry()
            .WithTracing(traceBuilder =>
                traceBuilder
                    .AddSource(otlmSettings.ServiceName)
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(
                                serviceName: otlmSettings.ServiceName, 
                                serviceInstanceId: otlmSettings.ServiceInstanceId))
                    .AddRedisInstrumentation()
                    .AddConsoleExporter()
                    .AddJaegerExporter(options =>
                    {
                        options.Endpoint = new Uri(otlmSettings.ExporterEndpoint);
                    }));
    }
}