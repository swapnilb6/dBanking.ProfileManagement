using dBanking.ProfileManagement.Core.ServiceContracts;
using System;
using dBanking.ProfileManagement.Core.ServiceContracts;

namespace dBanking.ProfileManagement.Core
{
    public sealed class SystemClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}