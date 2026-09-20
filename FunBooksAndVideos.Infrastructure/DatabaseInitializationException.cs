namespace FunBooksAndVideos.Infrastructure;

// The database could not be reached or prepared at startup. The message is meant to be read as it is, with no stack trace.
public sealed class DatabaseInitializationException : Exception
{
    public DatabaseInitializationException(string server, Exception innerException)
        : base(
            $"Could not prepare the database on {server}. Is SQL Server running? Start it with 'docker compose up -d --wait', "
            + $"or check ConnectionStrings:Default. Reason: {FirstLine(innerException.Message)}",
            innerException)
    {
    }

    private static string FirstLine(string message)
    {
        return message.Split('\n', 2)[0].Trim();
    }
}
