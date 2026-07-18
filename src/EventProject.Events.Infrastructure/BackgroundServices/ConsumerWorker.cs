using System.Text.Json;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EventProject.Events.Application.Abstractions.Repositories;
using EventProject.Events.Application.Abstractions.Services;
using EventProject.Events.Application.Caching;
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
    private readonly CacheTtlOptions _ttlOptions;

    public ConsumerWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaOptions> options,
        IOptions<CacheTtlOptions> ttlOptions,
        ILogger<ConsumerWorker> logger
    )
    {
        _scopeFactory = scopeFactory;
        _settings = options.Value;
        _ttlOptions = ttlOptions.Value;
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
            GroupId = _settings.ConsumerGroup,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
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

                    var existingEvent = await eventRepository.GetById(bookingConfirmed.EventId, stoppingToken);

                    if (existingEvent == null)
                    {
                        _logger.LogWarning(
                            "Событие не найдено. BookingId={BookingId}, EventId={EventId}",
                            bookingConfirmed.BookingId,
                            bookingConfirmed.EventId);
                        continue;
                    }

                    if (existingEvent.StartAt <= DateTime.UtcNow)
                    {
                        _logger.LogWarning(
                            "Событие уже началось. BookingId={BookingId}, EventId={EventId}, StartAt={StartAt}",
                            bookingConfirmed.BookingId,
                            bookingConfirmed.EventId,
                            existingEvent.StartAt);
                        continue;
                    }

                    if (existingEvent.AvailableSeats < bookingConfirmed.Seats)
                    {
                        _logger.LogWarning(
                            "Недостаточно мест. BookingId={BookingId}, EventId={EventId}, " +
                            "AvailableSeats={AvailableSeats}, Seats={Seats}",
                            bookingConfirmed.BookingId,
                            bookingConfirmed.EventId,
                            existingEvent.AvailableSeats,
                            bookingConfirmed.Seats);
                        continue;
                    }

                    existingEvent.TryReserveSeats(bookingConfirmed.Seats);

                    await eventRepository.SaveChanges(stoppingToken);

                    consumer.Commit(consumeResult);

                    var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();

                    // Обновить данные в кеше
                    await cacheService
                        .SetAsync(
                            CacheKeys.Event(existingEvent.Id),
                            existingEvent,
                            TimeSpan.FromMinutes(_ttlOptions.EventMinutes),
                            stoppingToken);
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