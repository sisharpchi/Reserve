using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Scheduling.Application.WorkingHours;
using ReserveFlow.Modules.Scheduling.Application.WorkingHours.CreateWorkingHour;

namespace ReserveFlow.Modules.Scheduling.Presentation;

internal sealed class CreateStaffWorkingHourEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/staff/{staffMemberId:guid}/working-hours", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .RequireTenantAccess()
            .WithTags("Scheduling")
            .WithName("CreateStaffWorkingHour");
    }

    private static async Task<IResult> Handle(
        Guid staffMemberId,
        CreateWorkingHourRequest request,
        ICommandHandler<CreateWorkingHourCommand, WorkingHourResponse> handler,
        CancellationToken cancellationToken)
    {
        WorkingHourResponse response = await handler.Handle(
            new CreateWorkingHourCommand(
                request.TenantId,
                staffMemberId,
                ResourceId: null,
                request.DayOfWeek,
                request.StartsAt,
                request.EndsAt),
            cancellationToken);

        return TypedResults.Created($"/api/admin/staff/{staffMemberId}/working-hours/{response.Id}", response);
    }
}
