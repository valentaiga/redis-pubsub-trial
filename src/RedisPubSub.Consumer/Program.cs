using RedisPubSub.Common.Extensions;
using RedisPubSub.Common.Options;
using RedisPubSub.Consumer;
using RedisPubSub.Redis;
using StackExchange.Redis;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddOptions<RedisConfig>()
            .BindConfiguration("Redis")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IRedisMultiplexer, RedisMultiplexer>();
        services.AddTransient<IConnectionMultiplexer>(src => src.GetRequiredService<IRedisMultiplexer>().Connect());

        services.ConfigureOpenTelemetry(context.Configuration, "OpenTelemetry");

        services.AddHostedService<Consumer>();
    })
    .Build();

host.Run();