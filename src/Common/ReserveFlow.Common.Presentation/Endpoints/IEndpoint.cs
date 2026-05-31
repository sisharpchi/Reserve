using Microsoft.AspNetCore.Routing;

namespace ReserveFlow.Common.Presentation.Endpoints;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}
