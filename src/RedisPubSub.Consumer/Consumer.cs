using System.Text.Json;
using Microsoft.Extensions.Options;
using RedisPubSub.Common.Models;
using RedisPubSub.Common.Options;
using RedisPubSub.Redis;

#nullable disable
namespace RedisPubSub.Consumer;

public class Consumer : BackgroundService
{
    private readonly ILogger<Consumer> _logger;
    private readonly IRedisMultiplexer _redisMultiplexer;
    private readonly string _channel;

    public Consumer(ILogger<Consumer> logger, IOptions<RedisConfig> redisConfig, IRedisMultiplexer redisMultiplexer)
    {
        _logger = logger;
        _redisMultiplexer = redisMultiplexer;
        _channel = redisConfig.Value.Channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var multiplexer = await _redisMultiplexer.ConnectAsync();
        var subscriber = multiplexer.GetSubscriber(_channel);

        await subscriber.SubscribeAsync(_channel, (channel, jsonMsg) =>
        {
            var msg = JsonSerializer.Deserialize<Message<string>>(jsonMsg);
            _logger.LogInformation("Received message: {Channel}, {TraceId}, {Message}", channel, msg.TraceId, msg.Data);
        });
    }
}