using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.Bookings;
using ReserveFlow.Modules.Bookings.Application.Bookings.GetBookings;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class GetAdminBookingsEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/bookings", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Bookings")
            .WithName("GetAdminBookings");
    }

    private static async Task<IResult> Handle(
        ITenantContext tenantContext,
        int? pageNumber,
        int? pageSize,
        string? status,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        string? sortBy,
        string? sortDirection,
        IQueryHandler<GetBookingsQuery, PagedResponse<BookingResponse>> handler,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return TypedResults.BadRequest("Tenant context is required.");
        }

        PagedResponse<BookingResponse> response = await handler.Handle(
            new GetBookingsQuery(tenantId, pageNumber, pageSize, status, fromUtc, toUtc, sortBy, sortDirection),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
