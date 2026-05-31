using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;
using ReserveFlow.Modules.Staffing.Application.StaffMembers.GetStaffMember;

namespace ReserveFlow.Modules.Staffing.Presentation;

internal sealed class GetAdminStaffMemberEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/staff/{staffMemberId:guid}", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Staffing")
            .WithName("GetAdminStaffMember");
    }

    private static async Task<IResult> Handle(
        Guid staffMemberId,
        IQueryHandler<GetStaffMemberQuery, StaffMemberResponse?> handler,
        CancellationToken cancellationToken)
    {
        StaffMemberResponse? response = await handler.Handle(
            new GetStaffMemberQuery(staffMemberId),
            cancellationToken);

        return response is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(response);
    }
}
