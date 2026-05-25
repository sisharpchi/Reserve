using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Staffing.Domain.StaffMembers;

public sealed record StaffMemberCreatedDomainEvent(
    Guid StaffMemberId,
    Guid TenantId,
    string DisplayName) : DomainEvent;
