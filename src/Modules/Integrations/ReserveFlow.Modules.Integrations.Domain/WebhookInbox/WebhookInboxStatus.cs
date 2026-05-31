namespace ReserveFlow.Modules.Integrations.Domain.WebhookInbox;

public enum WebhookInboxStatus
{
    Received = 0,
    Processed = 1,
    Failed = 2,
    Ignored = 3
}
