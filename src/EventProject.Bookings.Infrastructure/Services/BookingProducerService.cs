using System.Text.Json;
using Confluent.Kafka;
using EventProject.Bookings.Application.Abstractions.Services;
using EventProject.Bookings.Infrastructure.Options;
using EventProject.Shared;
using Microsoft.Extensions.Options;

namespace EventProject.Bookings.Infrastructure.Services;

public class BookingProducerService : IBookingProducerService, IDisposable
{
    private readonly IProducer<string, string> _producer;

    public BookingProducerService(IOptions<KafkaOptions> options)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task SendConfirm(Domain.Entities.Booking booking)
    {
        var bookingConfirmed = new BookingConfirmed
        {
            EventId = booking.EventId,
            BookingId = booking.Id,
            UserId = booking.UserId,
            Seats = 1,
            ConfirmedAt = booking.ProcessedAt ?? DateTime.UtcNow,
        };

        var result = await _producer.ProduceAsync(Constants.BOOKING_CONFIRMED_TOPIC_NAME, new Message<string, string>
        {
            Key = booking.EventId.ToString(),
            Value = JsonSerializer.Serialize(bookingConfirmed)
        });
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}