using FunBooksAndVideos.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FunBooksAndVideos.Tests.Infrastructure;

public class MigrationsTests
{
    // Runs without a database: it only compares the model with the committed migrations.
    [Fact]
    public void The_committed_migrations_match_the_current_model()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlServer("Server=localhost;Database=unused").Options;
        using var context = new AppDbContext(options);

        Assert.False(context.Database.HasPendingModelChanges());
    }
}
