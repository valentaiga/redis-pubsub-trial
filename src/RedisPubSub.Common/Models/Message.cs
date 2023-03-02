namespace RedisPubSub.Common.Models;

public record Message<T>(T? Data, string TraceId);