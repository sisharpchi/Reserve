namespace ReserveFlow.Modules.Resources.Presentation;

internal sealed record UpdateResourceRequest(
    string Name,
    string ResourceType,
    int Capacity);
