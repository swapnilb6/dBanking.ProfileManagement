namespace dBanking.ProfileManagement.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using dBanking.ProfileManagement.Core.DTOs;
    using dBanking.ProfileManagement.Core.Entities;
    using dBanking.ProfileManagement.Core.RepositoryContracts;
    using dBanking.ProfileManagement.Core.ServiceContracts;
    using dBanking.ProfileManagement.Core.Events;

    public sealed class AddressService : IAddressService
    {
        private readonly IAddressRepository _addresses;
        private readonly IAuditRepository _audits;
        private readonly IUnitOfWork _uow;
        private readonly IProfileEventPublisher _events;
        private readonly IClock _clock;
        private readonly IMapper _mapper;

        public AddressService(
            IAddressRepository addresses,
            IAuditRepository audits,
            IUnitOfWork uow,
            IProfileEventPublisher eventsPublisher,
            IClock clock,
            IMapper mapper)
        {
            _addresses = addresses;
            _audits = audits;
            _uow = uow;
            _events = eventsPublisher;
            _clock = clock;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<AddressDto>> GetByCustomerAsync(Guid customerId, CancellationToken ct)
        {
            // Load addresses and map to DTOs
            throw new NotImplementedException();
        }

        public async Task<AddressDto> UpsertAsync(UpsertAddressRequestDto request, ActorContext actor, CancellationToken ct)
        {
            // Steps:
            // 1) Map DTO -> Address, apply defaults, timestamps
            // 2) Upsert via repository
            // 3) Audit (old=null, new=address) and publish ProfileAddressUpdated
            // 4) Return AddressDto
            throw new NotImplementedException();
        }

        public async Task<AddressDto> UpdateAsync(UpdateAddressRequestDto request, ActorContext actor, CancellationToken ct)
        {
            // Steps:
            // 1) Load existing address; compute diff
            // 2) Apply updates; set UpdatedAt
            // 3) Persist + audit changed fields; publish event
            // 4) Return AddressDto
            throw new NotImplementedException();
        }

        public async Task<OperationResultDto> SetPrimaryAsync(Guid customerId, Guid addressId, ActorContext actor, CancellationToken ct)
        {
            // Steps:
            // 1) Validate address belongs to customer
            // 2) Use repository to set primary; persist
            // 3) Audit & publish (optional)
            throw new NotImplementedException();
        }
    }
}
