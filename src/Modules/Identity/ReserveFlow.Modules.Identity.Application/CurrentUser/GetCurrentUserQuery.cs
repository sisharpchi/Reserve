using ReserveFlow.Common.Application.Messaging;

namespace ReserveFlow.Modules.Identity.Application.CurrentUser;

public sealed record GetCurrentUserQuery : IQuery<CurrentUserResponse>;
