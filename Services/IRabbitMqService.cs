namespace TaskFlow.Services;

public interface IRabbitMqService
{
    void Publish(string exchange, string routingKey, string message);
    void EnsureExchangeAndQueue(string exchange, string queue, string routingKey);
}
