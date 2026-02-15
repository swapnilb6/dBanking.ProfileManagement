using AutoMapper;
using dBanking.ProfileManagement.Core.DTOs;
using dBanking.ProfileManagement.Core.Entities;
using dBanking.ProfileManagement.Core.Events;
using dBanking.ProfileManagement.Core.RepositoryContracts;
using dBanking.ProfileManagement.Core.ServiceContracts;
using dBanking.ProfileManagement.Core.Services.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace dBanking.ProfileManagement.Core.Services
{
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
            try
            {
                var entities = await _addresses.GetByCustomerAsync(customerId, ct);
                return entities.Select(_mapper.Map<AddressDto>).ToList();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving addresses for customer {customerId}.", ex);
            }
        }

        public async Task<AddressDto> UpsertAsync(UpsertAddressRequestDto request, ActorContext actor, CancellationToken ct)
        {
            // Create new Address
            var entity = _mapper.Map<Address>(request);
            if (entity.AddressId == Guid.Empty) entity.AddressId = Guid.NewGuid();
            var now = _clock.UtcNow;
            entity.CreatedAt = now;
            entity.UpdatedAt = now;

            await _addresses.UpsertAsync(entity, ct);

            // If set primary, ensure unique primary per customer
            if (request.IsPrimary)
                await _addresses.SetPrimaryAsync(request.CustomerId, entity.AddressId, ct);

            // Audit

            var audit = AuditFactory.CreateWithMeta(
                customerId: request.CustomerId,
                entityName: "Address",
                entityId: entity.AddressId.ToString(),
                action: "Upsert",
                oldObject: null,                 // or the previous state if you fetched it
                newObject: entity,
                changedFieldsCsv: "ALL",          // or a computed diff list
                actor: actor,
                now: now,
                embedMeta: true                  // include Reason/IP/UA inside NewJson._meta
            );


            await _audits.AddAsync(audit, ct);
            await _uow.SaveChangesAsync(ct);

           
            // Event
            await _events.PublishAsync(new ProfileAddressUpdated(
                CustomerId: request.CustomerId,
                AddressId: entity.AddressId,
                AddressType: entity.AddressType.ToString(),
                CorrelationId: actor.CorrelationId ?? Guid.Empty,
                EffectiveFrom: entity.EffectiveFrom
            ), ct);

            return _mapper.Map<AddressDto>(entity);
        }

        public async Task<AddressDto> UpdateAsync(UpdateAddressRequestDto request, ActorContext actor, CancellationToken ct)
        {
            var existing = await _addresses.GetAsync(request.CustomerId, request.AddressId, ct)
                           ?? throw new KeyNotFoundException("Address not found.");

            var before = new Address
            {
                AddressId = existing.AddressId,
                CustomerId = existing.CustomerId,
                AddressType = existing.AddressType,
                Line1 = existing.Line1,
                Line2 = existing.Line2,
                Line3 = existing.Line3,
                City = existing.City,
                StateProvince = existing.StateProvince,
                PostalCode = existing.PostalCode,
                CountryCode = existing.CountryCode,
                IsPrimary = existing.IsPrimary,
                EffectiveFrom = existing.EffectiveFrom,
                EffectiveTo = existing.EffectiveTo,
                CreatedAt = existing.CreatedAt,
                UpdatedAt = existing.UpdatedAt
            };

            // Apply updates
            existing.AddressType = Enum.Parse<AddressType>(request.AddressType, true);
            existing.Line1 = request.Line1;
            existing.Line2 = request.Line2;
            existing.Line3 = request.Line3;
            existing.City = request.City;
            existing.StateProvince = request.StateProvince;
            existing.PostalCode = request.PostalCode;
            existing.CountryCode = request.CountryCode;
            existing.IsPrimary = request.IsPrimary;
            existing.EffectiveFrom = request.EffectiveFrom;
            existing.EffectiveTo = request.EffectiveTo;
            existing.UpdatedAt = _clock.UtcNow;

            await _addresses.UpdateAsync(existing, ct);

            if (request.IsPrimary)
                await _addresses.SetPrimaryAsync(request.CustomerId, existing.AddressId, ct);

            var changedCsv = DiffHelper.ChangedFieldsCsv(before, existing, nameof(Address.CreatedAt), nameof(Address.UpdatedAt));


            var now = _clock.UtcNow; // or DateTimeOffset.UtcNow

            var audit = AuditFactory.CreateWithMeta(
                customerId: request.CustomerId,
                entityName: "Address",
                entityId: existing.AddressId.ToString(),
                action: "Upsert",
                oldObject: null,                 // or the previous state if you fetched it
                newObject: existing,
                changedFieldsCsv: "ALL",          // or a computed diff list
                actor: actor,
                now: now,
                embedMeta: true                  // include Reason/IP/UA inside NewJson._meta
            );

            await _audits.AddAsync(audit, ct);


            await _uow.SaveChangesAsync(ct);

            await _events.PublishAsync(new ProfileAddressUpdated(
                CustomerId: request.CustomerId,
                AddressId: existing.AddressId,
                AddressType: existing.AddressType.ToString(),
                CorrelationId: actor.CorrelationId ?? Guid.Empty,
                EffectiveFrom: existing.EffectiveFrom
            ), ct);

            return _mapper.Map<AddressDto>(existing);
        }

        public async Task<OperationResultDto> SetPrimaryAsync(Guid customerId, Guid addressId, ActorContext actor, CancellationToken ct)
        {
            await _addresses.SetPrimaryAsync(customerId, addressId, ct);
            var now = _clock.UtcNow; // or DateTimeOffset.UtcNow

            var audit = AuditFactory.CreateWithMeta(
                customerId: customerId,
                entityName: "Address",
                entityId: addressId.ToString(),
                action: "SetPrimary",
                oldObject: null,                 // or the previous state if you fetched it
                newObject: JsonHelper.ToJson(new { IsPrimary = true }),
                changedFieldsCsv: "IsPrimary",          // or a computed diff list
                actor: actor,
                now: now,
                embedMeta: true                  // include Reason/IP/UA inside NewJson._meta
            );

            await _audits.AddAsync(audit, ct);



            await _uow.SaveChangesAsync(ct);

            return new OperationResultDto { Success = true, Code = "OK", Message = "Primary address updated." };
        }
    }
}