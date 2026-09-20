using System.Diagnostics;

namespace FunBooksAndVideos.Tests.Infrastructure;

// Like [Fact], but skipped when Docker is not running, because these tests start a SQL Server container.
public sealed class SqlServerFactAttribute : FactAttribute
{
    private static readonly Lazy<bool> DockerCheck = new(CheckDocker);

    public SqlServerFactAttribute()
    {
        if (!DockerIsRunning)
        {
            Skip = "Docker is not running, so the SQL Server integration tests are skipped.";
        }
    }

    public static bool DockerIsRunning => DockerCheck.Value;

    private static bool CheckDocker()
    {
        try
        {
            var startInfo = new ProcessStartInfo("docker", "version --format {{.Server.Version}}")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using var process = Process.Start(startInfo)!;

            return process.WaitForExit(15_000) && process.ExitCode == 0;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
