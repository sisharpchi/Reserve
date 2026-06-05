using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Scheduling.Application.UnavailablePeriods;
using ReserveFlow.Modules.Scheduling.Application.UnavailablePeriods.CreateUnavailablePeriod;

namespace ReserveFlow.Modules.Scheduling.Presentation;

internal sealed class CreateOwnStaffUnavailablePeriodEndpoint : IEndpoint
{
    private const string StaffPolicy = "Staff";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/staff/unavailable-periods", Handle)
            .RequireAuthorization(StaffPolicy)
            .WithTags("Staff Scheduling")
            .WithName("CreateOwnStaffUnavailablePeriod");
    }

    private static async Task<IResult> Handle(
        CreateOwnUnavailablePeriodRequest request,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ICommandHandler<CreateUnavailablePeriodCommand, UnavailablePeriodResponse> handler,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return TypedResults.BadRequest("Tenant context is required.");
        }

        if (currentUser.StaffMemberId is not Guid staffMemberId)
        {
            return TypedResults.BadRequest("Staff member context is required.");
        }

        UnavailablePeriodResponse response = await handler.Handle(
            new CreateUnavailablePeriodCommand(
                tenantId,
                staffMemberId,
                ResourceId: null,
                request.StartsAtUtc,
                request.EndsAtUtc,
                request.Reason),
            cancellationToken);

        return TypedResults.Created($"/api/staff/unavailable-periods/{response.Id}", response);
    }
}
