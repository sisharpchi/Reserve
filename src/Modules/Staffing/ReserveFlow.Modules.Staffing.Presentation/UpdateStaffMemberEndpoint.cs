using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;
using ReserveFlow.Modules.Staffing.Application.StaffMembers.UpdateStaffMember;

namespace ReserveFlow.Modules.Staffing.Presentation;

internal sealed class UpdateStaffMemberEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/admin/staff/{staffMemberId:guid}", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Staffing")
            .WithName("UpdateStaffMember");
    }

    private static async Task<IResult> Handle(
        Guid staffMemberId,
        UpdateStaffMemberRequest request,
        ICommandHandler<UpdateStaffMemberCommand, StaffMemberResponse?> handler,
        CancellationToken cancellationToken)
    {
        StaffMemberResponse? response = await handler.Handle(
            new UpdateStaffMemberCommand(
                staffMemberId,
                request.DisplayName,
                request.Email),
            cancellationToken);

        return response is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(response);
    }
}
