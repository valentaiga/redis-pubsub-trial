using RedisPubSub.Common.Options;
using RedisPubSub.Producer;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddOptions<RedisConfiguration>()
            .BindConfiguration("Redis")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddHostedService<Producer>();
    })
    .Build();

await host.RunAsync();