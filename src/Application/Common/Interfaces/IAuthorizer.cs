namespace Application.Common.Interfaces;

public interface IAuthorizer <T>
{
    IEnumerable<IAuthorizationRequirement> Requirements { get; }
    void BuildPolicy(T instance);
}