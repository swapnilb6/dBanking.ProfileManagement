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

            CreateMap<Profile, ContactViewDto>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Contact.Email))
                .ForMember(dest => dest.EmailStatus, opt => opt.MapFrom(src => src.Contact.EmailStatus.ToString()))
                .ForMember(dest => dest.PhoneE164, opt => opt.MapFrom(src => src.Contact.PhoneE164))
                .ForMember(dest => dest.PhoneStatus, opt => opt.MapFrom(src => src.Contact.PhoneStatus.ToString()))
                .ForMember(dest => dest.PendingEmail, opt => opt.MapFrom(src => src.Contact.PendingEmail))
                .ForMember(dest => dest.PendingPhoneE164, opt => opt.MapFrom(src => src.Contact.PendingPhoneE164));

            // No Contact ← DTO mapping because updates flow via services,
            // not direct entity replacement.


            // ----------------------------
            // ADDRESSES
            // ----------------------------

            CreateMap<Address, AddressDto>()
                .ForMember(dest => dest.AddressType, opt => opt.MapFrom(src => src.AddressType.ToString()));

            CreateMap<UpdateAddressRequestDto, Address>()
                .ForMember(dest => dest.AddressType, opt => opt.MapFrom(src => Enum.Parse<AddressType>(src.AddressType, true)))
                .ForMember(dest => dest.AddressId, opt => opt.MapFrom(src => src.AddressId))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore()) // preserve original timestamps
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());

            CreateMap<UpsertAddressRequestDto, Address>()
                .ForMember(dest => dest.AddressType, opt => opt.MapFrom(src => Enum.Parse<AddressType>(src.AddressType, true)))
                .ForMember(dest => dest.AddressId, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());


            // ----------------------------
            // PREFERENCES
            // ----------------------------

            CreateMap<Preferences, PreferencesDto>();

            CreateMap<UpdatePreferencesRequestDto, Preferences>()
                .ForAllMembers(opt => opt.Condition((src, dest, value) => value != null));
            // Only update properties present in the request (partial update support)


            // ----------------------------
            // AUDIT
            // ----------------------------

            CreateMap<AuditRecord, AuditEntryDto>()
                .ForMember(dest => dest.ActorRole,
                    opt => opt.MapFrom(src => src.ActorRole.ToString()))
                .ForMember(dest => dest.SourceChannel,
                    opt => opt.MapFrom(src => src.SourceChannel.ToString()));


            // ----------------------------
            // GENERIC RESULTS
            // ----------------------------

            CreateMap<VerificationStatus, string>()
                .ConvertUsing(src => src.ToString());

            // Could add more response mappings if needed
        }
    }
}
