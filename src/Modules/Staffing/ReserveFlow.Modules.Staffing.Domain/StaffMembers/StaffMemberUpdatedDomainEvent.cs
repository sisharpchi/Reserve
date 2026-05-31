using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Staffing.Domain.StaffMembers;

public sealed record StaffMemberUpdatedDomainEvent(
    Guid StaffMemberId,
    Guid TenantId,
    string DisplayName) : DomainEvent;
