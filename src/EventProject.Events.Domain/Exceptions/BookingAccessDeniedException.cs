namespace EventProject.Events.Domain.Exceptions;

public class BookingAccessDeniedException : Exception
{
    public BookingAccessDeniedException()
    {
    }

    public BookingAccessDeniedException(string message) : base(message)
    {
    }

    public BookingAccessDeniedException(string message, Exception inner) : base(message, inner)
    {
    }
}