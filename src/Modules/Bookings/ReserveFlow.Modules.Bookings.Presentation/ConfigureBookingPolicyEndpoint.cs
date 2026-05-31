using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.BookingPolicies;
using ReserveFlow.Modules.Bookings.Application.BookingPolicies.ConfigureBookingPolicy;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class ConfigureBookingPolicyEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/admin/tenants/{tenantId:guid}/booking-policy", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .RequireTenantAccess()
            .WithTags("Booking Policies")
            .WithName("ConfigureBookingPolicy");
    }

    private static async Task<Ok<BookingPolicyResponse>> Handle(
        Guid tenantId,
        ConfigureBookingPolicyRequest request,
        ICommandHandler<ConfigureBookingPolicyCommand, BookingPolicyResponse> handler,
        CancellationToken cancellationToken)
    {
        BookingPolicyResponse response = await handler.Handle(
            new ConfigureBookingPolicyCommand(
                tenantId,
                request.MinimumAdvanceMinutes,
                request.CancellationDeadlineHours),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
