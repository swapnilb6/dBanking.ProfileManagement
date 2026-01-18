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
            var entities = await _addresses.GetByCustomerAsync(customerId, ct);
            return entities.Select(_mapper.Map<AddressDto>).ToList();
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
            await _audits.AddAsync(new AuditRecord
            {
                CustomerId = request.CustomerId,
                Entity = "Address",
                EntityId = entity.AddressId.ToString(),
                Operation = "Upsert",
                OldValueJson = JsonHelper.ToJson(null),
                NewValueJson = JsonHelper.ToJson(entity),
                ChangedFieldsCsv = "ALL",
                ActorId = actor.ActorId,
                ActorRole = actor.ActorRole,
                SourceChannel = actor.SourceChannel,
                ReasonCode = actor.Reason,
                IpAddress = actor.IpAddress,
                UserAgent = actor.UserAgent,
                CorrelationId = actor.CorrelationId,
                Timestamp = now
            }, ct);

            await _uow.SaveChangesAsync(ct);

            // Event
            await _events.PublishAsync(new ProfileAddressUpdated(
                CustomerId: request.CustomerId,
                AddressId: entity.AddressId,
                AddressType: entity.AddressType.ToString(),
                CorrelationId: actor.CorrelationId ?? string.Empty,
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

            await _audits.AddAsync(new AuditRecord
            {
                CustomerId = request.CustomerId,
                Entity = "Address",
                EntityId = existing.AddressId.ToString(),
                Operation = "Update",
                OldValueJson = JsonHelper.ToJson(before),
                NewValueJson = JsonHelper.ToJson(existing),
                ChangedFieldsCsv = string.IsNullOrWhiteSpace(changedCsv) ? "NONE" : changedCsv,
                ActorId = actor.ActorId,
                ActorRole = actor.ActorRole,
                SourceChannel = actor.SourceChannel,
                ReasonCode = actor.Reason,
                IpAddress = actor.IpAddress,
                UserAgent = actor.UserAgent,
                CorrelationId = actor.CorrelationId,
                Timestamp = _clock.UtcNow
            }, ct);

            await _uow.SaveChangesAsync(ct);

            await _events.PublishAsync(new ProfileAddressUpdated(
                CustomerId: request.CustomerId,
                AddressId: existing.AddressId,
                AddressType: existing.AddressType.ToString(),
                CorrelationId: actor.CorrelationId ?? string.Empty,
                EffectiveFrom: existing.EffectiveFrom
            ), ct);

            return _mapper.Map<AddressDto>(existing);
        }

        public async Task<OperationResultDto> SetPrimaryAsync(Guid customerId, Guid addressId, ActorContext actor, CancellationToken ct)
        {
            await _addresses.SetPrimaryAsync(customerId, addressId, ct);

            await _audits.AddAsync(new AuditRecord
            {
                CustomerId = customerId,
                Entity = "Address",
                EntityId = addressId.ToString(),
                Operation = "SetPrimary",
                OldValueJson = JsonHelper.ToJson(new { }),
                NewValueJson = JsonHelper.ToJson(new { IsPrimary = true }),
                ChangedFieldsCsv = "IsPrimary",
                ActorId = actor.ActorId,
                ActorRole = actor.ActorRole,
                SourceChannel = actor.SourceChannel,
                ReasonCode = actor.Reason,
                IpAddress = actor.IpAddress,
                UserAgent = actor.UserAgent,
                CorrelationId = actor.CorrelationId,
                Timestamp = _clock.UtcNow
            }, ct);

            await _uow.SaveChangesAsync(ct);

            return new OperationResultDto { Success = true, Code = "OK", Message = "Primary address updated." };
        }
    }
}