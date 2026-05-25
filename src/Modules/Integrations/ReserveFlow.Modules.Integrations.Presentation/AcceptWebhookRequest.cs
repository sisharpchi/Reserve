namespace ReserveFlow.Modules.Integrations.Presentation;

internal sealed record AcceptWebhookRequest(
    Guid? TenantId,
    string EventType,
    string PayloadJson);
