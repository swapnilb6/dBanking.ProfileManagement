using AutoMapper;
using dBanking.ProfileManagement.Core.DTOs;
using dBanking.ProfileManagement.Core.Entities;
using Profile = AutoMapper.Profile;

namespace dBanking.ProfileManagement.Core.Mappers
{
    public class ProfileMappingProfile : Profile
    {
        public ProfileMappingProfile()
        {
            // ----------------------------
            // CONTACTS
            // ----------------------------


            CreateMap<dBanking.ProfileManagement.Core.Entities.Profile, ContactViewDto>()
                .ForMember(d => d.Email, m => m.MapFrom(s => s.Email))
                .ForMember(d => d.EmailStatus, m => m.MapFrom(s => s.EmailStatus ?? "Unverified"))
                .ForMember(d => d.PhoneE164, m => m.MapFrom(s => s.PhoneE164))
                .ForMember(d => d.PhoneStatus, m => m.MapFrom(s => s.PhoneStatus ?? "Unverified"))
                .ForMember(d => d.PendingEmail, m => m.MapFrom(s => s.PendingEmail))
                .ForMember(d => d.PendingPhoneE164, m => m.MapFrom(s => s.PendingPhoneE164));


            // No Contact ← DTO mapping because updates flow via services,
            // not direct entity replacement.


            // ----------------------------
            // ADDRESSES
            // ----------------------------
            CreateMap<Address, AddressDto>()
                .ForMember(d => d.AddressId, m => m.MapFrom(s => s.AddressId))
                .ForMember(d => d.AddressType, m => m.MapFrom(s => s.Type))
                .ForMember(d => d.Line1, m => m.MapFrom(s => s.Line1))
                .ForMember(d => d.Line2, m => m.MapFrom(s => s.Line2))
                .ForMember(d => d.City, m => m.MapFrom(s => s.City))
                .ForMember(d => d.StateProvince, m => m.MapFrom(s => s.StateProvince))
                .ForMember(d => d.PostalCode, m => m.MapFrom(s => s.PostalCode))
                .ForMember(d => d.CountryCode, m => m.MapFrom(s => s.CountryCode))
                .ForMember(d => d.IsPrimary, m => m.MapFrom(s => s.IsPrimary))
                .ForMember(d => d.EffectiveFrom, m => m.MapFrom(s => s.EffectiveFrom))
                .ForMember(d => d.EffectiveTo, m => m.MapFrom(s => s.EffectiveTo));


            // ----------------------------
            // PREFERENCES
            // ----------------------------

            CreateMap<dBanking.ProfileManagement.Core.Entities.Profile, PreferencesDto>()
                .ForMember(d => d.SmsEnabled, m => m.MapFrom(s => s.SmsEnabled ?? false))
                .ForMember(d => d.EmailEnabled, m => m.MapFrom(s => s.EmailEnabled ?? false))
                .ForMember(d => d.PushEnabled, m => m.MapFrom(s => s.PushEnabled ?? false))
                .ForMember(d => d.RegulatoryConsent, m => m.MapFrom(s => s.RegulatoryConsent ?? false))
                .ForMember(d => d.Language, m => m.MapFrom(s => s.Language))
                .ForMember(d => d.TimeZone, m => m.MapFrom(s => s.TimeZone))
                .ForMember(d => d.PreferencesUpdatedAt, m => m.MapFrom(s => s.PreferencesUpdatedAt));


            // Only update properties present in the request (partial update support)


            // ----------------------------
            // AUDIT
            // ----------------------------


            CreateMap<AuditRecord, AuditEntryDto>()
                .ForMember(d => d.AuditId, m => m.MapFrom(s => s.Id))
                .ForMember(d => d.Actor, m => m.MapFrom(s => s.Actor))
                .ForMember(d => d.Channel, m => m.MapFrom(s => s.Channel))
                .ForMember(d => d.ChangedAt, m => m.MapFrom(s => s.ChangedAt))
                .ForMember(d => d.OldValueJson, m => m.MapFrom(s => string.IsNullOrWhiteSpace(s.OldJson) ? "{}" : s.OldJson!))
                .ForMember(d => d.NewValueJson, m => m.MapFrom(s => string.IsNullOrWhiteSpace(s.NewJson) ? "{}" : s.NewJson!));
            // Remove the old mapping that referenced AuditId(Guid), ActorId, SourceChannel, Timestamp, etc. [1](https://eyindia-my.sharepoint.com/personal/swapnil_bawankar_in_ey_com/Documents/Microsoft%20Copilot%20Chat%20Files/ProfileManagment_Entities%26DTOS.txt)



            // ----------------------------
            // GENERIC RESULTS
            // ----------------------------

            CreateMap<VerificationStatus, string>()
                .ConvertUsing(src => src.ToString());

            // Could add more response mappings if needed
        }
    }
}
