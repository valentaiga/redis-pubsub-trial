using Microsoft.Extensions.Options;
using RedisPubSub.Common.Options;
using StackExchange.Redis;

namespace RedisPubSub.Redis;

public class RedisMultiplexer : IRedisMultiplexer
{
    private readonly RedisConfig _config;
    
    public RedisMultiplexer(IOptions<RedisConfig> config)
    {
        _config = config.Value;
    }

    public ConnectionMultiplexer Connect()
        => ConnectionMultiplexer.Connect(_config.ConnectionString);
    
    public Task<ConnectionMultiplexer> ConnectAsync()
        => ConnectionMultiplexer.ConnectAsync(_config.ConnectionString);
}
