namespace dBanking.ProfileManagement.Core.RepositoryContracts
{
    using System.Threading;
    using System.Threading.Tasks;
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken ct);
    }
}
