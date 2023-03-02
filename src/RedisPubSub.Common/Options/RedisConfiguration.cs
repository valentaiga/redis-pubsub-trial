using System.ComponentModel.DataAnnotations;

#nullable disable
namespace RedisPubSub.Common.Options;

public class RedisConfiguration
{
    [Required] public string ConnectionString { get; init; }
    [Required] public string Channel { get; init; }
}