namespace MattEland.Jaimes.Core.Services;

public interface IEventsService
{
    T SendMessage<T>(T message) where T : class;
    TResult Request<TResult>();
}