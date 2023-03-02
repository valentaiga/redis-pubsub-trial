using RedisPubSub.Common.Options;
using RedisPubSub.Producer;
using RedisPubSub.Redis;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddOptions<RedisConfig>()
            .BindConfiguration("Redis")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IRedisProvider, RedisProvider>();
        services.AddHostedService<Producer>();
    })
    .Build();

await host.RunAsync();