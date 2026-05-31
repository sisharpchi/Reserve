using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;
using ReserveFlow.Modules.Staffing.Application.StaffMembers.GetActiveStaffMembers;

namespace ReserveFlow.Modules.Staffing.Presentation;

internal sealed class GetTenantStaffEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/tenants/{tenantId:guid}/staff", Handle)
            .WithTags("Public Staffing")
            .WithName("GetTenantStaff");
    }

    private static async Task<Ok<IReadOnlyList<StaffMemberResponse>>> Handle(
        Guid tenantId,
        IQueryHandler<GetActiveStaffMembersQuery, IReadOnlyList<StaffMemberResponse>> handler,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StaffMemberResponse> response = await handler.Handle(
            new GetActiveStaffMembersQuery(tenantId),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
