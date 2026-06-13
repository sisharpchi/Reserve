using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Tenants.Application.Tenants.GetPlatformUsage;

namespace ReserveFlow.Modules.Tenants.Presentation;

internal sealed class GetPlatformUsageEndpoint : IEndpoint
{
    private const string PlatformAdminPolicy = "PlatformAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/platform/usage", Handle)
            .RequireAuthorization(PlatformAdminPolicy)
            .WithTags("Platform")
            .WithName("GetPlatformUsage");
    }

    private static async Task<Ok<PlatformUsageResponse>> Handle(
        IQueryHandler<GetPlatformUsageQuery, PlatformUsageResponse> handler,
        CancellationToken cancellationToken)
    {
        PlatformUsageResponse response = await handler.Handle(
            new GetPlatformUsageQuery(),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
