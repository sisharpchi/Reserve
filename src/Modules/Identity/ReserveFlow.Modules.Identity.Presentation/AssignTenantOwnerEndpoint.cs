using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Identity.Application.TenantUsers.AssignTenantOwner;

namespace ReserveFlow.Modules.Identity.Presentation;

internal sealed class AssignTenantOwnerEndpoint : IEndpoint
{
    private const string PlatformAdminPolicy = "PlatformAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/platform/tenants/{tenantId:guid}/assign-owner", Handle)
            .RequireAuthorization(PlatformAdminPolicy)
            .WithTags("Identity")
            .WithName("AssignTenantOwner");
    }

    private static async Task<IResult> Handle(
        Guid tenantId,
        AssignTenantOwnerRequest request,
        ICommandHandler<AssignTenantOwnerCommand, AssignTenantOwnerResponse> handler,
        CancellationToken cancellationToken)
    {
        AssignTenantOwnerResponse response = await handler.Handle(
            new AssignTenantOwnerCommand(
                tenantId,
                request.KeycloakSubject,
                request.Email,
                request.DisplayName),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
