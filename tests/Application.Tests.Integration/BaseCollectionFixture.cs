using Xunit;

namespace Application.Tests.Integration;

[CollectionDefinition(nameof(BaseCollectionFixture))]
public class BaseCollectionFixture : ICollectionFixture<CustomApiFactory>
{
    
}