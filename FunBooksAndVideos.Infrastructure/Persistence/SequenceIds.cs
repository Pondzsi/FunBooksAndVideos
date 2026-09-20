using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace FunBooksAndVideos.Infrastructure.Persistence;

// Takes the next number from a SQL Server sequence. The number is not given back if the caller later fails, so IDs can have gaps.
internal static class SequenceIds
{
    public static async Task<long> NextAsync(AppDbContext context, string sequenceName, CancellationToken cancellationToken)
    {
        await context.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();
            command.CommandText = $"SELECT NEXT VALUE FOR [{sequenceName}]";

            return Convert.ToInt64(await command.ExecuteScalarAsync(cancellationToken));
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }
}
