using ToDoAI.IntegrationTests.Fixtures;

namespace ToDoAI.IntegrationTests.Collections;

[CollectionDefinition("Database")]
public sealed class DatabaseCollectionDefinition : ICollectionFixture<PostgresFixture> { }
