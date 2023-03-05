namespace RedisPubSub.Common.Models;

public class PropagatedMessage<T>
{
    public PropagatedMessage(T? data)
    {
        Data = data;
    }
    
    public T? Data { get; init; }
    public Dictionary<string, string>? Props { get; set; }
};