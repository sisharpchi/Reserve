using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Abstractions;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Staffing.Application.StaffMembers;
using ReserveFlow.Modules.Staffing.Application.StaffMembers.GetActiveStaffMembers;

namespace ReserveFlow.Modules.Staffing.Presentation;

internal sealed class GetTenantStaffEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/public/tenants/{tenantSlug}/staff", Handle)
            .WithTags("Public Staffing")
            .WithName("GetTenantStaff");
    }

    private static async Task<Results<Ok<IReadOnlyList<StaffMemberResponse>>, NotFound>> Handle(
        string tenantSlug,
        ITenantSlugResolver tenantSlugResolver,
        IQueryHandler<GetActiveStaffMembersQuery, IReadOnlyList<StaffMemberResponse>> handler,
        CancellationToken cancellationToken)
    {
        Guid? tenantId = await tenantSlugResolver.ResolveTenantIdAsync(tenantSlug, cancellationToken);

        if (tenantId is null)
        {
            return TypedResults.NotFound();
        }

        IReadOnlyList<StaffMemberResponse> response = await handler.Handle(
            new GetActiveStaffMembersQuery(tenantId.Value),
            cancellationToken);

        return TypedResults.Ok(response);
    }
}
