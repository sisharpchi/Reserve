using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.Tenants;
using ReserveFlow.Modules.Tenants.Application.Tenants.CreateTenant;

namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed class CreateTenantEndpoint : IEndpoint
{
    private const string PlatformAdminPolicy = "PlatformAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/platform/tenants", Handle)
            .RequireAuthorization(PlatformAdminPolicy)
            .WithTags("Tenants")
            .WithName("CreateTenant");
    }

    private static async Task<IResult> Handle(
        CreateTenantRequest request,
        ICommandHandler<CreateTenantCommand, TenantResponse> handler,
        CancellationToken cancellationToken)
    {
        TenantResponse response = await handler.Handle(
            new CreateTenantCommand(request.Name, request.Slug, request.TimeZoneId),
            cancellationToken);

        return TypedResults.Created($"/api/platform/tenants/{response.Slug}", response);
    }
}
