using System.Text.Json;
using Microsoft.Extensions.Options;
using RedisPubSub.Common.Models;
using RedisPubSub.Common.Options;
using StackExchange.Redis;

#nullable disable
namespace RedisPubSub.Consumer;

public class Consumer : BackgroundService
{
    private readonly ILogger<Consumer> _logger;
    private readonly ConnectionMultiplexer _connection;
    private readonly string _channel;

    public Consumer(ILogger<Consumer> logger, IOptions<RedisConfiguration> redisConfig)
    {
        _logger = logger;
        _connection  = ConnectionMultiplexer.Connect(redisConfig.Value.ConnectionString);
        _channel = redisConfig.Value.Channel;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _connection.GetSubscriber(_channel);

        await subscriber.SubscribeAsync(_channel, (channel, jsonMsg) =>
        {
            var msg = JsonSerializer.Deserialize<Message<string>>(jsonMsg);
            _logger.LogInformation("Received message: {Channel}, {TraceId}, {Message}", channel, msg.TraceId, msg.Data);
        });
    }
}