using System.Text;
using RabbitMQ.Client;

namespace TaskFlow.Services;

public class RabbitMqService(IConfiguration config, ILogger<RabbitMqService> logger) : IRabbitMqService, IDisposable
{
    private readonly IConnection _connection = (new ConnectionFactory()
    {
        HostName = config["RabbitMQ:HostName"] ?? "localhost",
        Port = int.TryParse(config["RabbitMQ:Port"], out int port) ? port : 5672,
        UserName = config["RabbitMQ:UserName"] ?? "guest",
        Password = config["RabbitMQ:Password"] ?? "guest"
    }).CreateConnection();
    private readonly ILogger<RabbitMqService> _logger = logger;


    public void EnsureExchangeAndQueue(string exchange, string queue, string routingKey)
    {
        using IModel channel = _connection.CreateModel();
        channel.ExchangeDeclare(exchange, "topic", durable: true, autoDelete: false);
        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue, exchange, routingKey);
    }

    public void Publish(string exchange, string routingKey, string message)
    {
        using IModel channel = _connection.CreateModel();
        byte[] body = Encoding.UTF8.GetBytes(message);
        IBasicProperties props = channel.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";
        channel.BasicPublish(exchange, routingKey, props, body);
        _logger.LogInformation("Published to {Exchange} / {RoutingKey}", exchange, routingKey);
    }

    public void Dispose()
    {
        if (_connection.IsOpen)
            _connection.Close();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
