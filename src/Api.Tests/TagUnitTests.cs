using Api.Features.Tags;
using Xunit;

namespace Api.Tests;

public class TagUnitTests
{
    [Fact]
    public void CreateTagRequest_CreatesInstance()
    {
        var name = "Unit Test Tag";
        var request = new CreateTagRequest(name);
        Assert.Equal(name, request.Name);
    }
}
