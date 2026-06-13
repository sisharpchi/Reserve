using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Application.Pagination;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;
using ReserveFlow.Modules.Staffing.Application.StaffMembers.GetStaffMembers;

namespace ReserveFlow.Modules.Staffing.Presentation;

internal sealed class GetAdminStaffMembersEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/staff", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .RequireTenantAccess()
            .WithTags("Staffing")
            .WithName("GetAdminStaffMembers");
    }

    private static async Task<Ok<PagedResponse<StaffMemberResponse>>> Handle(
        Guid tenantId,
        int? pageNumber,
        int? pageSize,
        string? search,
        bool? isActive,
        string? sortBy,
        string? sortDirection,
        IQueryHandler<GetStaffMembersQuery, PagedResponse<StaffMemberResponse>> handler,
        CancellationToken cancellationToken)
    {
        PagedResponse<StaffMemberResponse> response = await handler.Handle(
            new GetStaffMembersQuery(tenantId, pageNumber, pageSize, search, isActive, sortBy, sortDirection),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
