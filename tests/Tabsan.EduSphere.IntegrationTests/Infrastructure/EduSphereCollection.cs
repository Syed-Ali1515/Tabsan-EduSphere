using Xunit;

namespace Tabsan.EduSphere.IntegrationTests.Infrastructure;

/// <summary>
/// Shared xUnit collection fixture that provides a single <see cref="EduSphereWebFactory"/>
/// instance across all integration-test classes. This prevents multiple factories from
/// racing to create/drop the LocalDB test database simultaneously.
/// </summary>
[CollectionDefinition(Name)]
public sealed class EduSphereCollection : ICollectionFixture<EduSphereWebFactory>
{
    public const string Name = "EduSphere Integration Tests";
}
