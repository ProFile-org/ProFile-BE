namespace Application.Common.Exceptions;

public class EntityNotAvailableException : Exception
{
    public EntityNotAvailableException(string message) : base(message)
    {
        
    }
}