using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;
using ReserveFlow.Modules.Staffing.Application.StaffMembers.CreateStaffMember;

namespace ReserveFlow.Modules.Staffing.Presentation;

internal sealed class CreateStaffMemberEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/staff", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .RequireTenantAccess()
            .WithTags("Staffing")
            .WithName("CreateStaffMember");
    }

    private static async Task<IResult> Handle(
        CreateStaffMemberRequest request,
        ICommandHandler<CreateStaffMemberCommand, StaffMemberResponse> handler,
        CancellationToken cancellationToken)
    {
        StaffMemberResponse response = await handler.Handle(
            new CreateStaffMemberCommand(request.TenantId, request.DisplayName, request.Email),
            cancellationToken);

        return TypedResults.Created($"/api/admin/staff/{response.Id}", response);
    }
}
