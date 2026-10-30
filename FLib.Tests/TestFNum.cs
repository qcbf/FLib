using System.Reflection;
#if FIXED_POINT_NUMBER
using System.Numerics;
#endif

namespace FLib.Tests;

public class TestFNum
{
    private static readonly Func<FNum, FNum> _pow2 = typeof(FNum)
        .GetMethod("Pow2", BindingFlags.NonPublic | BindingFlags.Static)!
        .CreateDelegate<Func<FNum, FNum>>();

    [Theory]
    [InlineData(0.125)]
    [InlineData(0.5)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(1024)]
    public void NaturalLogMatchesReference(double input)
    {
        var value = (FNum)input;
        var expected = Math.Log((double)value);

        Assert.InRange(Math.Abs((double)FNum.Ln(value) - expected), 0, Math.Max(1, Math.Abs(expected)) * 1e-7);
    }

    [Theory]
    [InlineData(-4, 0.0625)]
    [InlineData(-1, 0.5)]
    [InlineData(0, 1)]
    [InlineData(0.5, 1.4142135623730951)]
    [InlineData(1, 2)]
    [InlineData(3, 8)]
    [InlineData(4, 16)]
    public void PowerOfTwoUsesExponent(double exponent, double expected)
    {
        Assert.InRange(Math.Abs((double)_pow2((FNum)exponent) - expected), 0, 1e-6);
    }

    [Fact]
    public void NormalizedVectorHasUnitLengthAndPreservesDirection()
    {
        var normalized = new FVector2(3, 4).Normalized;

        Assert.InRange(Math.Abs((double)normalized.X - 0.6), 0, 1e-7);
        Assert.InRange(Math.Abs((double)normalized.Y - 0.8), 0, 1e-7);
        Assert.InRange(Math.Abs((double)normalized.Magnitude - 1), 0, 1e-7);
        Assert.Equal(FVector2.Zero, FVector2.Zero.Normalized);
    }

    [Theory]
    [InlineData(0, 14, -9)]
    [InlineData(1, 7, -10)]
    [InlineData(-1, 15, -16)]
    [InlineData(2, 8, -17)]
    public void RotateAroundPointMatchesQuarterTurns(int quarterTurns, double expectedX, double expectedY)
    {
        var rotated = FVector2.Rotate(new FVector2(14, -9), new FVector2(11, -13), FNum.PI / 2 * quarterTurns);

        Assert.InRange(Math.Abs((double)rotated.X - expectedX), 0, 2e-6);
        Assert.InRange(Math.Abs((double)rotated.Y - expectedY), 0, 2e-6);
    }

#if FIXED_POINT_NUMBER
    [Fact]
    public void DivisionMatchesRoundedIntegerReferenceAcrossBitPositions()
    {
        for (var numeratorBit = 0; numeratorBit < 63; numeratorBit++)
        {
            for (var denominatorBit = 0; denominatorBit < 63; denominatorBit++)
            {
                var numerator = (1L << numeratorBit) | 17;
                var denominator = (1L << denominatorBit) | 3;
                var quotient = BigInteger.DivRem((BigInteger)numerator << 32, denominator, out var remainder);
                if (remainder * 2 >= denominator) quotient++;
                if (quotient > long.MaxValue) continue;

                Assert.Equal((long)quotient, (new FNum(numerator) / new FNum(denominator)).RawValue);
                Assert.Equal(-(long)quotient, (new FNum(-numerator) / new FNum(denominator)).RawValue);
            }
        }
    }

    [Theory]
    [InlineData(-26986075410L)]
    [InlineData(-26986075409L)]
    [InlineData(-26986075408L)]
    [InlineData(-13493037704L)]
    [InlineData(-6746518852L)]
    [InlineData(-1L)]
    [InlineData(0L)]
    [InlineData(1L)]
    [InlineData(6746518852L)]
    [InlineData(13493037704L)]
    [InlineData(26986075408L)]
    [InlineData(26986075409L)]
    [InlineData(26986075410L)]
    public void TrigonometryMatchesReferenceAtQuadrantsAndFastPathBoundaries(long rawAngle)
    {
        var angle = new FNum(rawAngle);
        var radians = (double)angle;

        Assert.InRange(Math.Abs((double)FNum.Sin(angle) - Math.Sin(radians)), 0, 2e-9);
        Assert.InRange(Math.Abs((double)FNum.Cos(angle) - Math.Cos(radians)), 0, 2e-9);
        Assert.InRange(Math.Abs((double)FNum.FastSin(angle) - Math.Sin(radians)), 0, 8e-6);
    }
#else
    [Fact]
    public void NaturalLogPreservesFloatingPointSpecialValues()
    {
        Assert.True(float.IsNegativeInfinity(FNum.Ln(FNum.Zero).RawValue));
        Assert.True(float.IsNaN(FNum.Ln(-FNum.One).RawValue));
        Assert.True(float.IsNaN(FNum.Ln((FNum)float.NaN).RawValue));
        Assert.True(float.IsPositiveInfinity(FNum.Ln((FNum)float.PositiveInfinity).RawValue));
    }
#endif
}
