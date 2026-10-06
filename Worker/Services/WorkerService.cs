using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Worker.Services;

public class WorkerService : BackgroundService
{
    private readonly ILogger<WorkerService> _logger;
    private readonly OllamaService _ollamaService;
    private IConnection? _connection;
    private IChannel? _channel;

    public WorkerService(ILogger<WorkerService> logger, OllamaService ollamaService)
    {
        _logger = logger;
        _ollamaService = ollamaService;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = await factory.CreateConnectionAsync(cancellationToken: cancellationToken);
        _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(queue: "question_requests", durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
        await _channel.QueueDeclareAsync(queue: "question_results", durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: cancellationToken);

        _logger.LogInformation("WorkerService started and connected to RabbitMQ.");

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel!);

        consumer.ReceivedAsync += async (sender, ea) =>
        {
            try
            {
                var jsonString = Encoding.UTF8.GetString(ea.Body.ToArray());
                var request = JsonSerializer.Deserialize<QuestionRequestMessage>(jsonString);

                if (request != null)
                {
                    _logger.LogInformation("Processing request {Id}", request.RequestId);

                    var response = await _ollamaService.GenerateQuestionAsync(request);
                    var responseBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));

                    await _channel!.BasicPublishAsync(
                        exchange: "",
                        routingKey: "question_results",
                        body: responseBody,
                        cancellationToken: cancellationToken);

                    _logger.LogInformation("Finished request {Id}", request.RequestId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message.");
            }
            finally
            {
                await _channel!.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: cancellationToken);
            }
        };

        await _channel!.BasicConsumeAsync(queue: "question_requests", autoAck: false, consumer: consumer, cancellationToken: cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_channel != null) await _channel.CloseAsync(cancellationToken);
        if (_connection != null) await _connection.CloseAsync(cancellationToken);

        await base.StartAsync(cancellationToken);
    }
}