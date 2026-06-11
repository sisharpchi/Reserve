using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Identity.Application.TenantUsers.InviteTenantOwner;

namespace ReserveFlow.Modules.Identity.Presentation;

internal sealed class InviteTenantOwnerEndpoint : IEndpoint
{
    private const string PlatformAdminPolicy = "PlatformAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/platform/tenants/{tenantId:guid}/owner/invite", Handle)
            .RequireAuthorization(PlatformAdminPolicy)
            .WithTags("Identity")
            .WithName("InviteTenantOwner");
    }

    private static async Task<IResult> Handle(
        Guid tenantId,
        InviteTenantOwnerRequest request,
        ICommandHandler<InviteTenantOwnerCommand, InviteTenantOwnerResponse> handler,
        CancellationToken cancellationToken)
    {
        InviteTenantOwnerResponse response = await handler.Handle(
            new InviteTenantOwnerCommand(
                tenantId,
                request.Email,
                request.DisplayName),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
