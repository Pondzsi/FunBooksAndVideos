namespace FunBooksAndVideos.Application.Common;

public interface IUnitOfWork
{
    // Persists everything staged or changed since the last commit, all or nothing.
    Task CommitAsync(CancellationToken cancellationToken);
}
