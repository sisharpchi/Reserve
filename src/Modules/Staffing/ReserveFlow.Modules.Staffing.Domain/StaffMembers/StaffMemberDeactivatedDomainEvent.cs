using ReserveFlow.Common.Domain;

namespace ReserveFlow.Modules.Staffing.Domain.StaffMembers;

public sealed record StaffMemberDeactivatedDomainEvent(
    Guid StaffMemberId,
    Guid TenantId) : DomainEvent;
