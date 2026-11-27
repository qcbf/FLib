namespace FLib.Tests;

public class TestFVersion
{
    [Fact]
    public void Json5RoundTripsVersionAsString()
    {
        var version = new FVersion(255, 2047, 8191);
        var json = Json5.Serialize(version);

        Assert.Equal("\"255.2047.8191\"", json);
        Assert.Equal(version, Json5.Deserialize<FVersion>(json));
    }

    [Fact]
    public void ComponentsUseTheirAssignedBits()
    {
        var version = new FVersion(1, 1024, 8191);
        Assert.Equal(0x01801fff, (int)version);

        version.Minor = 1;
        Assert.Equal("1.1.8191", version.ToString());

        version.Patch = 0;
        Assert.Equal("1.1.0", version.ToString());
    }

    [Theory]
    [InlineData("1.2.3", 1u, 2u, 3u, "2.4.6")]
    [InlineData("1.2.8191", 0u, 0u, 1u, "1.3.0")]
    [InlineData("1.2047.3", 0u, 1u, 0u, "2.0.3")]
    [InlineData("127.2047.8191", 0u, 0u, 1u, "128.0.0")]
    [InlineData("255.2047.8190", 0u, 0u, 1u, "255.2047.8191")]
    [InlineData("1.2.3", 0u, 4096u, 32768u, "3.6.3")]
    [InlineData("0.0.0", 0u, 0u, uint.MaxValue, "255.2047.8191")]
    public void SafeAddCarriesVersionComponents(string version, uint major, uint minor, uint patch, string expected)
    {
        Assert.Equal(expected, new FVersion(version).SafeAdd(major, minor, patch).ToString());
    }

    [Theory]
    [InlineData("0.0.0", 256u, 0u, 0u)]
    [InlineData("255.0.0", 1u, 0u, 0u)]
    [InlineData("255.2047.8191", 0u, 0u, 1u)]
    [InlineData("0.0.0", 0u, 524288u, 0u)]
    [InlineData("0.0.0", uint.MaxValue, 0u, 0u)]
    public void SafeAddThrowsOnMajorOverflow(string version, uint major, uint minor, uint patch)
    {
        Assert.Throws<OverflowException>(() => new FVersion(version).SafeAdd(major, minor, patch));
    }
}
