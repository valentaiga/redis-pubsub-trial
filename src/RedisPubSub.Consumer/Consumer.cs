using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RedisPubSub.Common.Models;
using RedisPubSub.Common.Options;
using RedisPubSub.Redis;
using StackExchange.Redis;

#nullable disable
namespace RedisPubSub.Consumer;

public class Consumer : BackgroundService
{
    private readonly ILogger<Consumer> _logger;
    private readonly IRedisMultiplexer _redisMultiplexer;
    private readonly string _channel;
    private readonly ActivitySource _activitySource;

    public Consumer(
        ILogger<Consumer> logger,
        IOptions<RedisConfig> redisConfig,
        IRedisMultiplexer redisMultiplexer,
        ActivitySource activitySource)
    {
        _logger = logger;
        _redisMultiplexer = redisMultiplexer;
        _channel = redisConfig.Value.Channel;
        _activitySource = activitySource;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var multiplexer = await _redisMultiplexer.ConnectAsync();
        var subscriber = multiplexer.GetSubscriber(_channel);

        var queue = await subscriber.SubscribeAsync(_channel);
        
        queue.OnMessage(MessageHandler);
        
        await Task.Delay(-1, stoppingToken);
    }

    private async Task MessageHandler(ChannelMessage args)
    {
        using var activity = _activitySource.StartActivity("Consumer Processing")!;
        var msg = JsonSerializer.Deserialize<Message<string>>(args.Message);
        activity.SetParentId(msg.TraceId);
        await DoSomeWork();

        _logger.LogInformation("Received message: {Channel}, {TraceId}, {Message}", args.Channel, msg.TraceId, msg.Data);
    }

    private Task DoSomeWork()
    {
        using var activity = _activitySource.StartActivity("Consumer Work");
        return Task.Delay(10);
    }
}