using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
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
        int? pageNumber,
        int? pageSize,
        string? status,
        string? type,
        string? sortBy,
        string? sortDirection,
        ITenantContext tenantContext,
        IQueryHandler<GetModuleMessagesQuery, PagedResponse<ModuleMessageResponse>> handler,
        CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not Guid tenantId)
        {
            return TypedResults.BadRequest("Tenant context is required.");
        }

        PagedResponse<ModuleMessageResponse> response = await handler.Handle(
            new GetModuleMessagesQuery(tenantId, pageNumber, pageSize, status, type, sortBy, sortDirection),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
