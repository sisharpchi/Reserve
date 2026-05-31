using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Catalog.Application.Services;
using ReserveFlow.Modules.Catalog.Application.Services.DeactivateService;

namespace ReserveFlow.Modules.Catalog.Presentation;

internal sealed class DeactivateServiceEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/admin/services/{serviceId:guid}", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Catalog")
            .WithName("DeactivateService");
    }

    private static async Task<IResult> Handle(
        Guid serviceId,
        ICommandHandler<DeactivateServiceCommand, ServiceResponse?> handler,
        CancellationToken cancellationToken)
    {
        ServiceResponse? response = await handler.Handle(
            new DeactivateServiceCommand(serviceId),
            cancellationToken);

        return response is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(response);
    }
}
