namespace FunBooksAndVideos.Tests.Infrastructure;

// Like [Theory], but skipped when Docker is not running, because these tests start a SQL Server container.
public sealed class SqlServerTheoryAttribute : TheoryAttribute
{
    public SqlServerTheoryAttribute()
    {
        if (!SqlServerFactAttribute.DockerIsRunning)
        {
            Skip = "Docker is not running, so the SQL Server integration tests are skipped.";
        }
    }
}
