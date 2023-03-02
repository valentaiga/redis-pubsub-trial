using System.ComponentModel.DataAnnotations;

#nullable disable
namespace RedisPubSub.Common.Options;

public class OpenTelemetryConfig
{
    [Required] public string ServiceName { get; init; }
    [Required] public string ExporterEndpoint { get; init; }
    public string Namespace => "RedisPubSub";
    public string ServiceInstanceId => Environment.MachineName;
}