using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EventProject.Events.Application.Abstractions.Repositories;
using EventProject.Events.Infrastructure.Options;
using EventProject.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventProject.Events.Infrastructure.BackgroundServices;

public class ConsumerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ConsumerWorker> _logger;
    private readonly KafkaOptions _settings;

    public ConsumerWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        ILogger<ConsumerWorker> logger
    )
    {
        _scopeFactory = scopeFactory;
        _settings = options.Value;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => Consume(stoppingToken), stoppingToken);
    }

    private async void Consume(CancellationToken stoppingToken)
    {
        await CreateTopic();

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            // Все консьюмеры с одинаковым GroupId делят партиции топика между собой
            GroupId = "order-processing",
            // Earliest — при первом запуске читать с начала топика
            // (если у группы ещё нет сохранённого офсета)
            AutoOffsetReset = AutoOffsetReset.Earliest,
            // false — управляем коммитом офсета вручную (at-least-once)
            EnableAutoCommit = false,
            // false — управляем позицией смещения вручную
            EnableAutoOffsetStore = false
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();

        // Kafka назначит консьюмеру партиции в рамках перебалансировки группы
        consumer.Subscribe(Constants.BOOKING_CONFIRMED_TOPIC_NAME);

        _logger.LogInformation("Consumer запущен. Ожидание сообщений из топика...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);

                    var bookingConfirmed = JsonSerializer.Deserialize<BookingConfirmed>(consumeResult.Message.Value);

                    _logger.LogInformation(
                        "Сообщение обработано. " +
                        "TopicPartitionOffset={TopicPartitionOffset}, " +
                        "BookingId={BookingId}, " +
                        "EventId={EventId}",
                        consumeResult.TopicPartitionOffset,
                        bookingConfirmed?.BookingId,
                        bookingConfirmed?.EventId);

                    using var scope = _scopeFactory.CreateScope();
                    var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

                    var @event = await eventRepository.GetById(bookingConfirmed.EventId, stoppingToken);
                    @event.TryReserveSeats();

                    await eventRepository.SaveChanges(stoppingToken);

                    consumer.Commit(consumeResult);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Ошибка обработки сообщения.");
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Consumer остановлен штатно.");
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task CreateTopic()
    {
        try
        {
            var adminClient = new AdminClientBuilder(new AdminClientConfig
            {
                BootstrapServers = _settings.BootstrapServers
            }).Build();

            await adminClient.CreateTopicsAsync([
                new TopicSpecification
                {
                    Name = Constants.BOOKING_CONFIRMED_TOPIC_NAME,
                    NumPartitions = 1,
                    ReplicationFactor = 1
                }
            ]);
        }
        catch (CreateTopicsException ex)
        {
            // Если топик уже существует — игнорируем.
            if (ex.Results.Any(r => r.Error.Code != ErrorCode.TopicAlreadyExists))
                throw;
        }
    }
}