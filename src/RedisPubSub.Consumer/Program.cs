using RedisPubSub.Common.Options;
using RedisPubSub.Consumer;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddOptions<RedisConfiguration>()
            .BindConfiguration("Redis")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddHostedService<Consumer>();
    })
    .Build();

host.Run();