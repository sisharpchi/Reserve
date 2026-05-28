using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ReserveFlow.Common.Application.Messaging;
using ReserveFlow.Common.Presentation.Endpoints;
using ReserveFlow.Modules.Scheduling.Application.WorkingHours;
using ReserveFlow.Modules.Scheduling.Application.WorkingHours.CreateWorkingHour;

namespace ReserveFlow.Modules.Scheduling.Presentation;

internal sealed class CreateResourceWorkingHourEndpoint : IEndpoint
{
    private const string TenantAdminPolicy = "TenantAdmin";

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/resources/{resourceId:guid}/working-hours", Handle)
            .RequireAuthorization(TenantAdminPolicy)
            .WithTags("Scheduling")
            .WithName("CreateResourceWorkingHour");
    }

    private static async Task<IResult> Handle(
        Guid resourceId,
        CreateWorkingHourRequest request,
        ICommandHandler<CreateWorkingHourCommand, WorkingHourResponse> handler,
        CancellationToken cancellationToken)
    {
        WorkingHourResponse response = await handler.Handle(
            new CreateWorkingHourCommand(
                request.TenantId,
                StaffMemberId: null,
                resourceId,
                request.DayOfWeek,
                request.StartsAt,
                request.EndsAt),
            cancellationToken);

        return TypedResults.Created($"/api/admin/resources/{resourceId}/working-hours/{response.Id}", response);
    }
}
