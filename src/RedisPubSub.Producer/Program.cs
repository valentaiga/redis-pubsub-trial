using RedisPubSub.Common.Extensions;
using RedisPubSub.Common.Options;
using RedisPubSub.Producer;
using RedisPubSub.Redis;
using StackExchange.Redis;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddOptions<RedisConfig>()
            .BindConfiguration("Redis")
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IRedisProvider, RedisProvider>();
        services.AddTransient<IConnectionMultiplexer>(src => src.GetRequiredService<IRedisProvider>().Connect());

        services.ConfigureOpenTelemetry(context.Configuration, "OpenTelemetry");

        services.AddHostedService<Producer>();
    })
    .Build();

await host.RunAsync();