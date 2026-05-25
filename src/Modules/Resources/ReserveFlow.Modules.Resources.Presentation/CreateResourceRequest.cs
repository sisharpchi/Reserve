namespace ReserveFlow.Modules.Resources.Presentation;

internal sealed record CreateResourceRequest(
    Guid TenantId,
    string Name,
    string ResourceType,
    int Capacity);
