using System.Threading;
using System.Threading.Tasks;
using dBanking.ProfileManagement.Core.RepositoryContracts;
using dBanking.ProfileManagement.Infrastructure.DbContext;

namespace dBanking.ProfileManagement.Infrastructure.UnitOfWork
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly ProfileDbContext _db;

        public EfUnitOfWork(ProfileDbContext db) => _db = db;

        public Task<int> SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
    }
}
