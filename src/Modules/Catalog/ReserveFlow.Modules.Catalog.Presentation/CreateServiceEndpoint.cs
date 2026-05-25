using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Catalog.Application.Services;
using ReserveFlow.Modules.Catalog.Application.Services.CreateService;

namespace ReserveFlow.Modules.Catalog.Presentation;

internal sealed class CreateServiceEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/services", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Catalog")
            .WithName("CreateService");
    }

    private static async Task<IResult> Handle(
        CreateServiceRequest request,
        ICommandHandler<CreateServiceCommand, ServiceResponse> handler,
        CancellationToken cancellationToken)
    {
        ServiceResponse response = await handler.Handle(
            new CreateServiceCommand(
                request.TenantId,
                request.Name,
                request.DurationMinutes,
                request.Price,
                request.Currency),
            cancellationToken);

        return TypedResults.Created($"/api/admin/services/{response.Id}", response);
    }
}
