using System.Diagnostics;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RedisPubSub.Common.Models;
using RedisPubSub.Common.Options;
using RedisPubSub.Redis;

namespace RedisPubSub.Producer;

public class Producer : BackgroundService
{
    private readonly ActivitySource _activitySource;
    private readonly ILogger<Producer> _logger;
    private readonly IRedisMultiplexer _redisMultiplexer;
    private readonly string _channel;

    public Producer(ILogger<Producer> logger, IOptions<RedisConfig> redisConfig, IOptions<OpenTelemetryConfig> otlmConfig, IRedisMultiplexer redisMultiplexer)
    {
        _logger = logger;
        _redisMultiplexer = redisMultiplexer;
        _channel = redisConfig.Value.Channel;
        _activitySource = new ActivitySource(otlmConfig.Value.ServiceName);
    } 

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var multiplexer = await _redisMultiplexer.ConnectAsync();
        var subscriber = multiplexer.GetSubscriber(_channel);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = _activitySource.StartActivity()!;
            activity.SetTag("redis.client_name", subscriber.Multiplexer.ClientName);
            activity.SetTag("redis.operations_count", subscriber.Multiplexer.OperationCount);
            
            using (_activitySource.StartActivity("ExpectedDelay"))
            {
                await Task.Delay(300, stoppingToken);
            }

            activity.AddEvent(new ActivityEvent("DelayFinished"));
            
            activity.SetTag("my_tag", "WorkingTag");

            var msg = new Message<string>("Some Data", activity.TraceId.ToString());
            var json = Serialize(msg);
            activity.AddEvent(new ActivityEvent("DataSerialized"));
            
            await subscriber.PublishAsync(_channel, json);
            activity.AddEvent(new ActivityEvent("DataPublished"));

            _logger.LogInformation("Message sent: {TraceId}, {Data}", msg.TraceId, msg.Data);
            await Task.Delay(4700, stoppingToken);
        }
    }

    private static string Serialize<T>(T value) // just for OpenTelemetry tracking purpose
        => JsonSerializer.Serialize(value);
}