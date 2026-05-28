using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;
using ReserveFlow.Modules.Staffing.Application.StaffMembers.GetStaffMembers;

namespace ReserveFlow.Modules.Staffing.Presentation;

internal sealed class GetAdminStaffMembersEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/staff", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Staffing")
            .WithName("GetAdminStaffMembers");
    }

    private static async Task<Ok<IReadOnlyList<StaffMemberResponse>>> Handle(
        Guid tenantId,
        IQueryHandler<GetStaffMembersQuery, IReadOnlyList<StaffMemberResponse>> handler,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StaffMemberResponse> response = await handler.Handle(
            new GetStaffMembersQuery(tenantId),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
