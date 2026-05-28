using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;
using ReserveFlow.Modules.Staffing.Application.StaffMembers.DeactivateStaffMember;

namespace ReserveFlow.Modules.Staffing.Presentation;

internal sealed class DeactivateStaffMemberEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/admin/staff/{staffMemberId:guid}", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Staffing")
            .WithName("DeactivateStaffMember");
    }

    private static async Task<IResult> Handle(
        Guid staffMemberId,
        ICommandHandler<DeactivateStaffMemberCommand, StaffMemberResponse?> handler,
        CancellationToken cancellationToken)
    {
        StaffMemberResponse? response = await handler.Handle(
            new DeactivateStaffMemberCommand(staffMemberId),
            cancellationToken);

        return response is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(response);
    }
}
