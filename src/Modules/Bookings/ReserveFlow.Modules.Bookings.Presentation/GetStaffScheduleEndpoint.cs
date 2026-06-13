using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.GetStaffSchedule;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class GetStaffScheduleEndpoint : IEndpoint
{
    private const string StaffPolicy = "Staff";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/staff/me/schedule", Handle)
            .RequireAuthorization(StaffPolicy)
            .WithTags("Staff Bookings")
            .WithName("GetStaffSchedule");
    }

    private static async Task<IResult> Handle(
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IQueryHandler<GetStaffScheduleQuery, IReadOnlyList<BookingResponse>> handler,
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

        IReadOnlyList<BookingResponse> response = await handler.Handle(
            new GetStaffScheduleQuery(tenantId, staffMemberId),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
