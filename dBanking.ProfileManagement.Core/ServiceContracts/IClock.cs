using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dBanking.ProfileManagement.Core.ServiceContracts
{
    using System;
    public interface IClock
    {
        DateTimeOffset UtcNow { get; }
    }

}
