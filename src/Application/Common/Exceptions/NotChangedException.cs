namespace Application.Common.Exceptions;

public class NotChangedException : Exception
{
    public NotChangedException(string message) : base(message)
    {
    }
}