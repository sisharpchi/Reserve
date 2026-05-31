using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Identity.Application.Users.SyncUser;

public sealed record SyncUserCommand : ICommand<SyncUserResponse>;
