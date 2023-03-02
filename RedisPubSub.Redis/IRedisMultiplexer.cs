using StackExchange.Redis;

namespace RedisPubSub.Redis;

public interface IRedisMultiplexer
{
    ConnectionMultiplexer Connect();
    Task<ConnectionMultiplexer> ConnectAsync();
}