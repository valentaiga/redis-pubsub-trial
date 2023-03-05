using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using RedisPubSub.Common.Models;
using RedisPubSub.Common.Options;
using RedisPubSub.Redis;

namespace RedisPubSub.Producer;

public class Producer : BackgroundService
{
    private readonly TextMapPropagator _propagator = Propagators.DefaultTextMapPropagator;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<Producer> _logger;
    private readonly IRedisMultiplexer _redisMultiplexer;
    private readonly string _channel;

    public Producer(
        ILogger<Producer> logger,
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
        
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var activity = _activitySource.StartActivity($"{_channel} Publish", ActivityKind.Producer)!)
            {
                activity.SetTag("redis.client_name", subscriber.Multiplexer.ClientName);
                activity.SetTag("redis.operations_count", subscriber.Multiplexer.OperationCount);

                using (_activitySource.StartActivity("Expected Delay"))
                {
                    await Task.Delay(300, stoppingToken);
                }

                activity.SetTag("my_tag", "WorkingTag");
                
                var msg = new PropagatedMessage<string>("Some Data");

                var contextToInject = activity.Context;
                _propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), msg, Setter);

                var json = JsonSerializer.Serialize(msg);
                activity.AddEvent(new ActivityEvent("DataSerialized"));

                await subscriber.PublishAsync(_channel, json);
                activity.AddEvent(new ActivityEvent("DataPublished"));

                _logger.LogInformation("Message sent: {Data}", msg.Data);
            }
            await Task.Delay(4700, stoppingToken);
        }
    }

    private void Setter(PropagatedMessage<string> msg, string key, string value)
    {
        try
        {
            msg.Props ??= new();
            msg.Props[key] = value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to inject a trace context");
        }
    }
}