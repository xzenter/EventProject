using EventProject.Models;

namespace EventProject.Tests;

public class BookingTests
{
    // После вызова Confirm() бронь возвращает статус Confirmed и заполненный ProcessedAt.
    [Fact]
    public void Confirm_Should_Set_Status_To_Confirmed_And_ProcessedAt_To_NonNull()
    {
        // Arrange
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = BookingStatus.Pending,
            ProcessedAt = null
        };

        // Act
        booking.Confirm(DateTime.UtcNow);

        // Assert
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    // После вызова Reject() бронь возвращает статус Rejected и заполненный ProcessedAt.
    [Fact]
    public void Reject_Should_Set_Status_To_Rejected_And_ProcessedAt_To_NonNull()
    {
        // Arrange
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Status = BookingStatus.Pending,
            ProcessedAt = null
        };

        // Act
        booking.Reject(DateTime.UtcNow);

        // Assert
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }
}