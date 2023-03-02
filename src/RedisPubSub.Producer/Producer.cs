using System.Text.Json;
using Microsoft.Extensions.Options;
using RedisPubSub.Common.Models;
using RedisPubSub.Common.Options;
using StackExchange.Redis;

namespace RedisPubSub.Producer;

public class Producer : BackgroundService
{
    private readonly ILogger<Producer> _logger;
    private readonly ConnectionMultiplexer _connection;
    private readonly string _channel;

    public Producer(ILogger<Producer> logger, IOptions<RedisConfiguration> redisConfig)
    {
        _logger = logger;
        _connection  = ConnectionMultiplexer.Connect(redisConfig.Value.ConnectionString);
        _channel = redisConfig.Value.Channel;
    } 

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _connection.GetSubscriber(_channel);
        while (!stoppingToken.IsCancellationRequested)
        {
            var traceIdStab = Guid.NewGuid().ToString();
            
            var msg = new Message<string>(traceIdStab, traceIdStab);
            var json = JsonSerializer.Serialize(msg);
            
            await subscriber.PublishAsync(_channel, json);
            
            _logger.LogInformation("Message sent: {TraceId}, {Data}", msg.TraceId, msg.Data);
            await Task.Delay(5000, stoppingToken);
        }
    }
}