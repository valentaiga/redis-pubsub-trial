using RedisPubSub.Common.Options;
using RedisPubSub.Consumer;
using RedisPubSub.Redis;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddOptions<RedisConfig>()
            .BindConfiguration("Redis")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IRedisProvider, RedisProvider>();
        services.AddHostedService<Consumer>();
    })
    .Build();

host.Run();