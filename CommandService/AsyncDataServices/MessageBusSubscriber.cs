using CommandService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.Extensions.Configuration;
namespace CommandService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private IEventProcessor _eventProcessor;

        private IConnection _connection;

        private IChannel? _channel;

        public MessageBusSubscriber(IConfiguration configuration, IEventProcessor eventProcessor)
        {
            _configuration = configuration;
            _eventProcessor = eventProcessor;
            InitializeRabbitMQ().Wait();
        }

        private async Task InitializeRabbitMQ()
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

                // Declare exchange and queue asynchronously
                await _channel.ExchangeDeclareAsync(exchange: "trigger", type: ExchangeType.Fanout);
                await _channel.QueueDeclareAsync(
                    queue: "platforms",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );
                await _channel.QueueBindAsync(queue: "platforms", exchange: "trigger", routingKey: "");
                Console.WriteLine("--> Listening to message bus...");
                _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdownAsync;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to RabbitMQ: {ex.Message}");
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (ModuleHandle, ea) =>
            {
                Console.WriteLine("--> Event Received!");
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await _eventProcessor.ProcessEvent(message);
            };

            _channel.BasicConsumeAsync(queue: "platforms", autoAck: true, consumer: consumer);
            return Task.CompletedTask;

        }
        private Task RabbitMQ_ConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            Console.WriteLine("--> RabbitMQ Connection Shutdown");
            return Task.CompletedTask;
        }
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
