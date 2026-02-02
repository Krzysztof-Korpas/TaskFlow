using System.Text;
using RabbitMQ.Client;

namespace TaskFlow.Services;

public class RabbitMqService : IRabbitMqService, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqService> _logger;

    public RabbitMqService(IConfiguration config, ILogger<RabbitMqService> logger)
    {
        _logger = logger;
        var factory = new ConnectionFactory
        {
            HostName = config["RabbitMQ:HostName"] ?? "localhost",
            Port = int.TryParse(config["RabbitMQ:Port"], out var p) ? p : 5672,
            UserName = config["RabbitMQ:UserName"] ?? "guest",
            Password = config["RabbitMQ:Password"] ?? "guest"
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void EnsureExchangeAndQueue(string exchange, string queue, string routingKey)
    {
        _channel.ExchangeDeclare(exchange, "topic", durable: true, autoDelete: false);
        _channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(queue, exchange, routingKey);
    }

    public void Publish(string exchange, string routingKey, string message)
    {
        var body = Encoding.UTF8.GetBytes(message);
        var props = _channel.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";
        _channel.BasicPublish(exchange, routingKey, props, body);
        _logger.LogInformation("Published to {Exchange} / {RoutingKey}", exchange, routingKey);
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
