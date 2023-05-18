namespace Application.Common.Exceptions;

public class NotAllowedException : Exception
{
    public NotAllowedException(string message) : base(message)
    {
    }
}