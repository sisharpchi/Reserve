using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Scheduling.Application.UnavailablePeriods;
using ReserveFlow.Modules.Scheduling.Application.UnavailablePeriods.CreateUnavailablePeriod;

namespace ReserveFlow.Modules.Scheduling.Presentation;

internal sealed class CreateResourceUnavailablePeriodEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/resources/{resourceId:guid}/unavailable-periods", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Scheduling")
            .WithName("CreateResourceUnavailablePeriod");
    }

    private static async Task<IResult> Handle(
        Guid resourceId,
        CreateUnavailablePeriodRequest request,
        ICommandHandler<CreateUnavailablePeriodCommand, UnavailablePeriodResponse> handler,
        CancellationToken cancellationToken)
    {
        UnavailablePeriodResponse response = await handler.Handle(
            new CreateUnavailablePeriodCommand(
                request.TenantId,
                StaffMemberId: null,
                resourceId,
                request.StartsAtUtc,
                request.EndsAtUtc,
                request.Reason),
            cancellationToken);

        return TypedResults.Created($"/api/admin/resources/{resourceId}/unavailable-periods/{response.Id}", response);
    }
}
