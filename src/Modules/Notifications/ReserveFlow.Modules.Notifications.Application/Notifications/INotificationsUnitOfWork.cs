namespace ReserveFlow.Modules.Notifications.Application.Notifications;

public interface INotificationsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
