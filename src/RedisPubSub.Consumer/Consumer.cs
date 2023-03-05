using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RedisPubSub.Common.Models;
using RedisPubSub.Common.Options;
using RedisPubSub.Redis;
using StackExchange.Redis;

#nullable disable
namespace RedisPubSub.Consumer;

public class Consumer : BackgroundService
{
    private readonly TextMapPropagator _propagator = Propagators.DefaultTextMapPropagator;
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
        var msg = JsonSerializer.Deserialize<PropagatedMessage<string>>(args.Message);

        var parentContext = _propagator.Extract(default, msg, Getter);
        Baggage.Current = parentContext.Baggage;
        
        using var activity = _activitySource.StartActivity($"{args.Channel} Receive", ActivityKind.Consumer, parentContext.ActivityContext)!;

        await DoSomeWork();

        _logger.LogInformation("Received message: {Channel}, {Message}", args.Channel, msg.Data);
    }

    private IEnumerable<string> Getter(PropagatedMessage<string> msg, string key)
    {
        try
        {
            if (msg.Props is not null 
                && msg.Props.TryGetValue(key, out var value))
            {
                return new[] { value };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract a trace context");
        }

        return Enumerable.Empty<string>();
    }
    
    private Task DoSomeWork()
    {
        using var activity = _activitySource.StartActivity("Fast Operation");
        return Task.Delay(10);
    }
}