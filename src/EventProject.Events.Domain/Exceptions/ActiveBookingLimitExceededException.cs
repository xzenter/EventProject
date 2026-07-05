namespace EventProject.Events.Domain.Exceptions;

public class ActiveBookingLimitExceededException : Exception
{
    public ActiveBookingLimitExceededException()
    {
    }

    public ActiveBookingLimitExceededException(string message) : base(message)
    {
    }

    public ActiveBookingLimitExceededException(string message, Exception inner) : base(message, inner)
    {
    }
}