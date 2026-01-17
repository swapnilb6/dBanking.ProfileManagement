namespace dBanking.ProfileManagement.Core.ServiceContracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using dBanking.ProfileManagement.Core.DTOs;

    public interface IAuditService
    {
        Task<IReadOnlyList<AuditEntryDto>> GetAsync(Guid customerId, string? entityFilter, int skip, int take, CancellationToken ct);
    }
}
