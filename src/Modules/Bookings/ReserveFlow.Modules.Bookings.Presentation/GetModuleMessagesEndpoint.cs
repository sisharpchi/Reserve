using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Bookings.Application.ModuleMessages;
using ReserveFlow.Modules.Bookings.Application.ModuleMessages.GetModuleMessages;

namespace ReserveFlow.Modules.Bookings.Presentation;

internal sealed class GetModuleMessagesEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/module-messages", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Bookings")
            .WithName("GetModuleMessages");
    }

    private static async Task<IResult> Handle(
        int? take,
        ITenantContext tenantContext,
        IQueryHandler<GetModuleMessagesQuery, IReadOnlyList<ModuleMessageResponse>> handler,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return TypedResults.BadRequest("Tenant context is required.");
        }

        IReadOnlyList<ModuleMessageResponse> response = await handler.Handle(
            new GetModuleMessagesQuery(tenantId, take ?? 100),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
