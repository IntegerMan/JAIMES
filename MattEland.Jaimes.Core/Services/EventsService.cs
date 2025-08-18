using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.Logging;

namespace MattEland.Jaimes.Core.Services;

public class EventsService(ILogger<EventsService> logger) : IEventsService
{
    public T SendMessage<T>(T message) where T : class
    {
        logger.LogDebug("Sending {Type}: {Message}", typeof(T).Name, message);
        
        return WeakReferenceMessenger.Default.Send(message);
    }

    public TResult Request<TResult>() 
    {
        logger.LogDebug("Requesting {Type}", typeof(TResult).Name);

        return WeakReferenceMessenger.Default.Send<RequestMessage<TResult>>();
    }
}