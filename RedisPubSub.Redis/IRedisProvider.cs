using StackExchange.Redis;

namespace RedisPubSub.Redis;

public interface IRedisProvider
{
    ConnectionMultiplexer Connect();
    ISubscriber GetSubscriber(string channel);
}