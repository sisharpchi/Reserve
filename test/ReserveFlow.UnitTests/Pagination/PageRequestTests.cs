using ReserveFlow.Common.Application.Pagination;

namespace ReserveFlow.UnitTests.Pagination;

public sealed class PageRequestTests
{
    [Fact]
    public void CreateUsesDefaultsForMissingOrInvalidValues()
    {
        PageRequest pageRequest = PageRequest.Create(pageNumber: 0, pageSize: -1);

        Assert.Equal(PageRequest.DefaultPageNumber, pageRequest.PageNumber);
        Assert.Equal(PageRequest.DefaultPageSize, pageRequest.PageSize);
        Assert.Equal(0, pageRequest.Skip);
    }

    [Fact]
    public void CreateCapsPageSizeAtMaximum()
    {
        PageRequest pageRequest = PageRequest.Create(pageNumber: 3, pageSize: 1_000);

        Assert.Equal(3, pageRequest.PageNumber);
        Assert.Equal(PageRequest.MaxPageSize, pageRequest.PageSize);
        Assert.Equal(200, pageRequest.Skip);
    }
}
