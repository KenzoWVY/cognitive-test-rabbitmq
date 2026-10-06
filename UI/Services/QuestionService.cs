
using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace UI.Services;

public class QuestionService : IQuestionService, IAsyncDisposable
{
    private IConnection? _connection;
    private IChannel? _channel;

    public event Action<QuestionResponseMessage>? OnQuestionReceived;

    public async Task InitializeAsync()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        await _channel.QueueDeclareAsync(queue: "question_requests", durable: true, exclusive: false, autoDelete: false);
        await _channel.QueueDeclareAsync(queue: "question_results", durable: true, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var jsonString = Encoding.UTF8.GetString(ea.Body.ToArray());
            var response = JsonSerializer.Deserialize<QuestionResponseMessage>(jsonString);
            if (response != null)
            {
                OnQuestionReceived?.Invoke(response);
            }

            await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        await _channel.BasicConsumeAsync(queue: "question_results", autoAck: false, consumer: consumer);
    }

    public async Task RequestQuestionsAsync(int count, Guid sessionId)
    {
        if (_channel == null) return;

        string[] topics = ["Math", "Language"];
        string[] difficulties = ["Easy", "Medium", "Hard"];

        for (int i = 0; i < count; i++)
        {

            var request = new QuestionRequestMessage
            {
                SessionId = sessionId,
                RequestId = Guid.NewGuid(),
                Topic = topics[i % topics.Length],
                Difficulty = difficulties[i % difficulties.Length]
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request));
            await _channel.BasicPublishAsync("", "question_requests", body);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.CloseAsync();
        if (_connection != null) await _connection.CloseAsync();
    }
}
