using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Catalog.Application.Services;
using ReserveFlow.Modules.Catalog.Application.Services.UpdateService;

namespace ReserveFlow.Modules.Catalog.Presentation;

internal sealed class UpdateServiceEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/admin/services/{serviceId:guid}", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Catalog")
            .WithName("UpdateService");
    }

    private static async Task<IResult> Handle(
        Guid serviceId,
        UpdateServiceRequest request,
        ICommandHandler<UpdateServiceCommand, ServiceResponse?> handler,
        CancellationToken cancellationToken)
    {
        ServiceResponse? response = await handler.Handle(
            new UpdateServiceCommand(
                serviceId,
                request.Name,
                request.DurationMinutes,
                request.Price,
                request.Currency),
            cancellationToken);

        return response is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(response);
    }
}
