using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;

namespace ReserveFlow.Modules.Bookings.Application.Bookings.GetBookings;

public sealed record GetBookingsQuery(
    Guid TenantId,
    int? PageNumber,
    int? PageSize,
    string? Status,
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtc,
    string? SortBy,
    string? SortDirection)
    : IQuery<PagedResponse<BookingResponse>>;
