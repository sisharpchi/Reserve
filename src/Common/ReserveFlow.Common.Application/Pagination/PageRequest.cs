namespace ReserveFlow.Common.Application.Pagination;

public sealed record PageRequest(int PageNumber, int PageSize)
{
    public const int DefaultPageNumber = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Skip => (PageNumber - 1) * PageSize;

    public static PageRequest Create(int? pageNumber, int? pageSize)
    {
        int normalizedPageNumber = pageNumber.GetValueOrDefault(DefaultPageNumber);

        if (normalizedPageNumber < 1)
        {
            normalizedPageNumber = DefaultPageNumber;
        }

        int normalizedPageSize = pageSize.GetValueOrDefault(DefaultPageSize);

        if (normalizedPageSize < 1)
        {
            normalizedPageSize = DefaultPageSize;
        }

        if (normalizedPageSize > MaxPageSize)
        {
            normalizedPageSize = MaxPageSize;
        }

        return new PageRequest(normalizedPageNumber, normalizedPageSize);
    }
}
