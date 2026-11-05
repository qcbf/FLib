namespace FLib.Tests;

public class TestFOrientedRect
{
    [Theory]
    [InlineData(45, 6, true)]
    [InlineData(45, -6, true)]
    [InlineData(135, 1.5, false)]
    [InlineData(135, -1.5, false)]
    public void ContainsRotatesPointsWithEqualAxisOffsets(double angle, double offset, bool expected)
    {
        var rect = new FOrientedRect
        {
            Center = new FVector2(11, -13),
            Extends = new FVector2(10, 2),
            Angle = (FNum)angle
        };
        var point = rect.Center + new FVector2((FNum)offset, (FNum)offset);

        Assert.Equal(expected, rect.Contains(point));
    }
}
