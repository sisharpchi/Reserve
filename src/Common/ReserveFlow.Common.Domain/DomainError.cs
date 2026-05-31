namespace ReserveFlow.Common.Domain;

public sealed record DomainError(string Code, string Description)
{
    public static readonly DomainError None = new(string.Empty, string.Empty);
}
