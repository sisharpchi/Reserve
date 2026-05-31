using Microsoft.AspNetCore.Builder;

namespace ReserveFlow.Common.Infrastructure.Observability;

public static class CorrelationIdApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
