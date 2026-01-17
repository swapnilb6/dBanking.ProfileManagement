namespace dBanking.ProfileManagement.Core.RepositoryContracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using dBanking.ProfileManagement.Core.Entities;

    public interface IAuditRepository
    {
        Task AddAsync(AuditRecord record, CancellationToken ct);
        Task<IReadOnlyList<AuditRecord>> GetAsync(Guid customerId, string? entityFilter, int skip, int take, CancellationToken ct);
    }
}
