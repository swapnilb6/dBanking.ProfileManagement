using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dBanking.ProfileManagement.Core.Entities
{
    public enum ActorRole { Customer = 0, CSA = 1, Supervisor = 2, System = 3 }
    public enum SourceChannel { MobileApp = 0, Web = 1, CSA = 2, System = 3 }
    public enum VerificationStatus
    {
        Pending = 0,
        Verified = 1,
        Expired = 2,
        Failed = 3,
        Cancelled = 4
    }

    public enum VerificationType
    {
        EmailLink = 0,
        SmsOtp = 1
    }

    public enum AddressType
    {
        Residential = 0,
        Mailing = 1,
        Work = 2
    }

    public enum ContactStatus
    {
        Unknown = 0,
        PendingVerification = 1,
        Verified = 2
    }
}
