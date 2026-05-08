using PlatformService.Dtos;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using System.Text;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient, IAsyncDisposable
    {
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IChannel? _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Async initialization instead of blocking in constructor
        public async Task InitializeAsync()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };

            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);

                _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdownAsync;

                Console.WriteLine("--> Connected to Message Bus");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not connect to the Message Bus: {ex.Message}");
            }
        }

        public async Task PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);

            if (_connection != null && _connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
                await SendMessage(message);
            }
            else
            {
                Console.WriteLine("--> RabbitMQ Connection is closed, not sending");
            }
        }

        private async Task SendMessage(string message)
        {
            if (_channel == null) return;

            var body = Encoding.UTF8.GetBytes(message);

            await _channel.BasicPublishAsync(
                exchange: "trigger",
                routingKey: "",
                mandatory: false,
                body: new ReadOnlyMemory<byte>(body)
            );

            Console.WriteLine($"--> We have sent {message}");
        }

        private Task RabbitMQ_ConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ Connection Shutdown");
            return Task.CompletedTask;
        }

        // Graceful cleanup
        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("MessageBus Disposed");
            if (_channel != null)
            {
                await _channel.CloseAsync();
                _channel = null;
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection = null;
            }
        }
    }
}
