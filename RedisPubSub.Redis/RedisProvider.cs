using Microsoft.Extensions.Options;
using RedisPubSub.Common.Options;
using StackExchange.Redis;

namespace RedisPubSub.Redis;

public class RedisProvider : IRedisProvider, IDisposable
{
    private readonly ConnectionMultiplexer _multiplexer;
    public RedisProvider(IOptions<RedisConfig> config)
    {
        _multiplexer = ConnectionMultiplexer.Connect(config.Value.ConnectionString);
    }

    public ConnectionMultiplexer Connect() 
        => _multiplexer;

    public ISubscriber GetSubscriber(string channel) 
        => _multiplexer.GetSubscriber(channel);

    public void Dispose()
    {
        _multiplexer?.Dispose();
    }
}
