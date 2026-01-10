//==================={By Qcbf|qcbf@qq.com|4/29/2022 3:14:28 PM}===================

// #define FIXED_POINT_NUMBER

#define FIXED_POINT_NUMBER_FAST

using System;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using FLib.FixedNumber;

namespace FLib
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public struct FNum : IEquatable<FNum>, IComparable<FNum>, IFormattable, IJson5Deserializable, IJson5Serializable, IConvertible, IBytesSerializable
    {
#if FIXED_POINT_NUMBER
        public long RawValue;
        public static readonly FNum Epsilon = new(1L);
        public static readonly FNum MaxValue = new(MAX_VALUE);
        public static readonly FNum MinValue = new(MIN_VALUE);
        public static readonly FNum PI = new(mPI);
        private static readonly FNum PiOver2 = new(PI_OVER_2);
        private static readonly FNum Log2Max = new(LOG2MAX);
        private static readonly FNum Log2Min = new(LOG2MIN);
        private static readonly FNum Ln2 = new(LN2);
        private static readonly FNum LutInterval = (LUT_SIZE - 1) / PiOver2;
        private const int NUM_BITS = 64;
        private const long PI_TIMES_2 = 0x6487ED511;
        private const long MAX_VALUE = long.MaxValue;
        private const long MIN_VALUE = long.MinValue;
        private const int FRACTIONAL_PLACES = 32;
        private const long ONE = 1L << FRACTIONAL_PLACES;
        private const long mPI = 0x3243F6A88;
        private const long PI_OVER_2 = 0x1921FB544;
        private const long LN2 = 0xB17217F7;
        private const long LOG2MAX = 0x1F00000000;
        private const long LOG2MIN = -0x2000000000;
        private const int LUT_SIZE = (int)(PI_OVER_2 >> 15);
        public FNum(long rawValue) => RawValue = rawValue;
        public FNum(ReadOnlySpan<char> str)
        {
            _ = decimal.TryParse(str, out var v);
            RawValue = (long)(v * ONE);
        }
#else
        public float RawValue;
        public static readonly FNum Epsilon = float.Epsilon;
        public static readonly FNum MaxValue = float.MaxValue;
        public static readonly FNum MinValue = float.MinValue;
        public static readonly FNum PI = MathF.PI;
        private static readonly FNum Ln2 = 0.69315f;
        public FNum(float value) => RawValue = value;
        public FNum(ReadOnlySpan<char> str)
        {
            _ = decimal.TryParse(str, out var v);
            RawValue = (float)v;
        }
#endif

        public static readonly FNum One = 1;
        public static readonly FNum Zero = new();
        public static readonly FNum Ten = One * 10;
        public static readonly FNum Hundred = One * 100;
        public static readonly FNum Thousand = One * 1000;
        public static readonly FNum OneHalf = One / 2;
        public static readonly FNum Tenth = One / Ten;
        public static readonly FNum Hundredth = One / Hundred;
        public static readonly FNum Thousandth = One / Thousand;
        public static readonly FNum SqrtMaxValue = Sqrt(MaxValue);


        #region Math
        /// <summary>
        /// Returns a number indicating the sign of a Fix64 number.
        /// Returns 1 if the value is positive, 0 if is 0, and -1 if it is negative.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Sign(FNum value) => value.RawValue < 0 ? -1 : value.RawValue > 0 ? 1 : 0;

        /// <summary>
        /// 
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SignWithoutZero(FNum value) => (value >= Zero) ? 1 : -1;

        /// <summary>
        /// Returns the absolute value of a Fix64 number.
        /// Note: Abs(Fix64.MinValue) == Fix64.MaxValue.
        /// </summary>
        public static FNum Abs(FNum value)
        {
#if FIXED_POINT_NUMBER
            if (value.RawValue == MIN_VALUE)
            {
                return MaxValue;
            }

            // branchless implementation, see http://www.strchr.com/optimized_abs_function
            var mask = value.RawValue >> 63;
            return new FNum((value.RawValue + mask) ^ mask);
#else
            return MathF.Abs(value);
#endif
        }

        /// <summary>
        /// Returns the absolute value of a Fix64 number.
        /// FastAbs(Fix64.MinValue) is undefined.
        /// </summary>
        public static FNum FastAbs(FNum value)
        {
#if FIXED_POINT_NUMBER
            // branchless implementation, see http://www.strchr.com/optimized_abs_function
            var mask = value.RawValue >> 63;
            return new FNum((value.RawValue + mask) ^ mask);
#else
            return MathF.Abs(value);
#endif
        }


        /// <summary>
        /// Returns the largest integer less than or equal to the specified number.
        /// </summary>
        public static FNum Floor(FNum value)
        {
#if FIXED_POINT_NUMBER
            // Just zero out the fractional part
            return new FNum((long)((ulong)value.RawValue & 0xFFFFFFFF00000000));
#else
            return MathF.Floor(value);
#endif
        }

        /// <summary>
        /// Returns the smallest integral value that is greater than or equal to the specified number.
        /// </summary>
        public static FNum Ceiling(FNum value)
        {
#if FIXED_POINT_NUMBER
            var hasFractionalPart = (value.RawValue & 0x00000000FFFFFFFF) != 0;
            return hasFractionalPart ? Floor(value) + One : value;
#else
            return MathF.Ceiling(value);
#endif
        }

        /// <summary>
        /// Rounds a value to the nearest integral value.
        /// If the value is halfway between an even and an uneven value, returns the even value.
        /// </summary>
        public static FNum Round(FNum value)
        {
#if FIXED_POINT_NUMBER
            var fractionalPart = value.RawValue & 0x00000000FFFFFFFF;
            var integralPart = Floor(value);
            if (fractionalPart < 0x80000000)
            {
                return integralPart;
            }
            if (fractionalPart > 0x80000000)
            {
                return integralPart + One;
            }
            // if number is halfway between two values, round to the nearest even number
            // this is the method used by System.Math.Round().
            return (integralPart.RawValue & ONE) == 0
                ? integralPart
                : integralPart + One;
#else
            return MathF.Round(value);
#endif
        }

#if FIXED_POINT_NUMBER
#if FIXED_POINT_NUMBER_FAST
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FNum operator +(in FNum x, in FNum y) => FastAdd(x, y);
#else
        public static FNum operator +(FNum x, FNum y)
        {
            var xl = x.RawValue;
            var yl = y.RawValue;
            var sum = xl + yl;
            // if signs of operands are equal and signs of sum and x are different
            if ((~(xl ^ yl) & (xl ^ sum) & MIN_VALUE) != 0)
            {
                sum = xl > 0 ? MAX_VALUE : MIN_VALUE;
            }
            return new FNum(sum);
        }
#endif
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FNum FastAdd(in FNum x, in FNum y) => new(x.RawValue + y.RawValue);

#if FIXED_POINT_NUMBER
#if FIXED_POINT_NUMBER_FAST
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FNum operator -(in FNum x, in FNum y) => FastSub(x, y);
#else
        public static FNum operator -(FNum x, FNum y)
        {
            var xl = x.RawValue;
            var yl = y.RawValue;
            var diff = xl - yl;
            // if signs of operands are different and signs of sum and x are different
            if (((xl ^ yl) & (xl ^ diff) & MIN_VALUE) != 0)
            {
                diff = xl < 0 ? MIN_VALUE : MAX_VALUE;
            }
            return new FNum(diff);
        }
#endif
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FNum FastSub(FNum x, FNum y) => new(x.RawValue - y.RawValue);

#if FIXED_POINT_NUMBER
        private static long AddOverflowHelper(long x, long y, ref bool overflow)
        {
            var sum = x + y;
            // x + y overflows if sign(x) ^ sign(y) != sign(sum)
            overflow |= ((x ^ y ^ sum) & MIN_VALUE) != 0;
            return sum;
        }

#if FIXED_POINT_NUMBER_FAST
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FNum operator *(in FNum x, in FNum y) => FastMul(x, y);
#else
        public static FNum operator *(FNum x, FNum y)
        {
            var xl = x.RawValue;
            var yl = y.RawValue;
            if (xl == One.RawValue) return y;
            if (yl == One.RawValue) return x;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> FRACTIONAL_PLACES;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> FRACTIONAL_PLACES;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> FRACTIONAL_PLACES;
            var hiResult = hihi << FRACTIONAL_PLACES;

            var overflow = false;
            var sum = AddOverflowHelper((long)loResult, lohi, ref overflow);
            sum = AddOverflowHelper(sum, hilo, ref overflow);
            sum = AddOverflowHelper(sum, hiResult, ref overflow);

            var opSignsEqual = ((xl ^ yl) & MIN_VALUE) == 0;

            // if signs of operands are equal and sign of result is negative,
            // then multiplication overflowed positively
            // the reverse is also true
            if (opSignsEqual)
            {
                if (sum < 0 || (overflow && xl > 0))
                {
                    return MaxValue;
                }
            }
            else
            {
                if (sum > 0)
                {
                    return MinValue;
                }
            }

            // if the top 32 bits of hihi (unused in the result) are neither all 0s nor 1s,
            // then this means the result overflowed.
            var topCarry = hihi >> FRACTIONAL_PLACES;
            if (topCarry != 0 && topCarry != -1 /*&& xl != -17 && yl != -17*/)
            {
                return opSignsEqual ? MaxValue : MinValue;
            }

            // If signs differ, both operands' magnitudes are greater than 1,
            // and the result is greater than the negative operand, then there was negative overflow.
            if (opSignsEqual) return new FNum(sum);
            long posOp, negOp;
            if (xl > yl)
            {
                posOp = xl;
                negOp = yl;
            }
            else
            {
                posOp = yl;
                negOp = xl;
            }
            if (sum > negOp && negOp < -ONE && posOp > ONE)
            {
                return MinValue;
            }

            return new FNum(sum);
        }
#endif
#endif

        /// <summary>
        /// Performs multiplication without checking for overflow.
        /// Useful for performance-critical code where the values are guaranteed not to cause overflow
        /// </summary>
        public static FNum FastMul(FNum x, FNum y)
        {
#if FIXED_POINT_NUMBER
            var xl = x.RawValue;
            var yl = y.RawValue;
            if (xl == One.RawValue) return y;
            if (yl == One.RawValue) return x;
            if (xl == 0 || yl == 0) return Zero;

            var xlo = (ulong)(xl & 0x00000000FFFFFFFF);
            var xhi = xl >> FRACTIONAL_PLACES;
            var ylo = (ulong)(yl & 0x00000000FFFFFFFF);
            var yhi = yl >> FRACTIONAL_PLACES;

            var lolo = xlo * ylo;
            var lohi = (long)xlo * yhi;
            var hilo = xhi * (long)ylo;
            var hihi = xhi * yhi;

            var loResult = lolo >> FRACTIONAL_PLACES;
            var hiResult = hihi << FRACTIONAL_PLACES;

            var sum = (long)loResult + lohi + hilo + hiResult;
            return new FNum(sum);
#else
            return x.RawValue * y.RawValue;
#endif
        }

#if FIXED_POINT_NUMBER
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int CountLeadingZeroes(ulong x)
        {
            var result = 0;
            while ((x & 0xF000000000000000) == 0)
            {
                result += 4;
                x <<= 4;
            }
            while ((x & 0x8000000000000000) == 0)
            {
                result += 1;
                x <<= 1;
            }
            return result;
        }

        public static FNum operator /(FNum x, FNum y)
        {
            var yl = y.RawValue;
            if (yl == One.RawValue) return x; // 如果y是1, 则直接返回x
            if (yl == 0) throw new DivideByZeroException();

            var xl = x.RawValue;
            if (xl == 0) return Zero;

            var remainder = (ulong)(xl >= 0 ? xl : -xl);
            var divider = (ulong)(yl >= 0 ? yl : -yl);
            var quotient = 0UL;
            var bitPos = NUM_BITS / 2 + 1;


            // If the divider is divisible by 2^n, take advantage of it.
            while ((divider & 0xF) == 0 && bitPos >= 4)
            {
                divider >>= 4;
                bitPos -= 4;
            }

            while (remainder != 0 && bitPos >= 0)
            {
                var shift = CountLeadingZeroes(remainder);
                if (shift > bitPos)
                {
                    shift = bitPos;
                }
                remainder <<= shift;
                bitPos -= shift;

                var div = remainder / divider;
                remainder = remainder % divider;
                quotient += div << bitPos;

                // Detect overflow
                if ((div & ~(0xFFFFFFFFFFFFFFFF >> bitPos)) != 0)
                {
                    return ((xl ^ yl) & MIN_VALUE) == 0 ? MaxValue : MinValue;
                }

                remainder <<= 1;
                --bitPos;
            }

            // rounding
            ++quotient;
            var result = (long)(quotient >> 1);
            if (((xl ^ yl) & MIN_VALUE) != 0)
            {
                result = -result;
            }

            return new FNum(result);
        }
#endif

        /// <summary>
        /// Performs modulo as fast as possible; throws if x == MinValue and y == -1.
        /// Use the operator (%) for a more reliable but slower modulo.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FNum FastMod(FNum x, FNum y) => new(x.RawValue % y.RawValue);

        /// <summary>
        /// Returns 2 raised to the specified power.
        /// Provides at least 6 decimals of accuracy.
        /// </summary>
        internal static FNum Pow2(FNum x)
        {
#if FIXED_POINT_NUMBER
            if (x.RawValue == 0)
            {
                return One;
            }

            // Avoid negative arguments by exploiting that exp(-x) = 1/exp(x).
            var neg = x.RawValue < 0;
            if (neg)
            {
                x = -x;
            }

            if (x == One)
            {
                return neg ? One / 2 : 2;
            }
            if (x >= Log2Max)
            {
                return neg ? One / MaxValue : MaxValue;
            }
            if (x <= Log2Min)
            {
                return neg ? MaxValue : Zero;
            }

            /* The algorithm is based on the power series for exp(x):
             * http://en.wikipedia.org/wiki/Exponential_function#Formal_definition
             *
             * From term n, we get term n+1 by multiplying with x/n.
             * When the sum term drops to zero, we can stop summing.
             */

            var integerPart = (int)Floor(x);
            // Take fractional part of exponent
            x = new FNum(x.RawValue & 0x00000000FFFFFFFF);

            var result = One;
            var term = One;
            var i = 1;
            while (term.RawValue != 0)
            {
                term = FastMul(FastMul(x, term), Ln2) / i;
                result += term;
                i++;
            }

            result = new FNum(result.RawValue << integerPart);
            if (neg)
            {
                result = One / result;
            }

            return result;
#else
            return MathF.Pow(x, 2);
#endif
        }

        /// <summary>
        /// Returns the base-2 logarithm of a specified number.
        /// Provides at least 9 decimals of accuracy.
        /// </summary>
        /// <exception>The argument was non-positive</exception>
        internal static FNum Log2(FNum x)
        {
#if FIXED_POINT_NUMBER
            if (x.RawValue <= 0)
                throw new Exception($"Non-positive value passed to Ln {x}");

            // This implementation is based on Clay. S. Turner's fast binary logarithm
            // algorithm (C. S. Turner,  "A Fast Binary Logarithm Algorithm", IEEE Signal
            //     Processing Mag., pp. 124,140, Sep. 2010.)

            long b = 1U << (FRACTIONAL_PLACES - 1);
            long y = 0;

            var rawX = x.RawValue;
            while (rawX < ONE)
            {
                rawX <<= 1;
                y -= ONE;
            }

            while (rawX >= ONE << 1)
            {
                rawX >>= 1;
                y += ONE;
            }

            var z = new FNum(rawX);

            for (var i = 0; i < FRACTIONAL_PLACES; i++)
            {
                z = FastMul(z, z);
                if (z.RawValue >= ONE << 1)
                {
                    z = new FNum(z.RawValue >> 1);
                    y += b;
                }
                b >>= 1;
            }

            return new FNum(y);
#else
            return MathF.Log(x, 2);
#endif
        }

        /// <summary>
        /// Returns the natural logarithm of a specified number.
        /// Provides at least 7 decimals of accuracy.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The argument was non-positive
        /// </exception>
        public static FNum Ln(FNum x)
        {
            return FastMul(Log2(x), Ln2);
        }

        /// <summary>
        /// Returns a specified number raised to the specified power.
        /// Provides about 5 digits of accuracy for the result.
        /// </summary>
        /// <exception cref="DivideByZeroException">
        /// The base was zero, with a negative exponent
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// The base was negative, with a non-zero exponent
        /// </exception>
        public static FNum Pow(FNum b, FNum exp)
        {
#if FIXED_POINT_NUMBER
            if (b == One)
            {
                return One;
            }
            if (exp.RawValue == 0)
            {
                return One;
            }
            if (b.RawValue == 0)
            {
                if (exp.RawValue < 0)
                {
                    throw new DivideByZeroException();
                }
                return Zero;
            }

            var log2 = Log2(b);
            return Pow2(exp * log2);
#else
            return MathF.Pow(b, exp);
#endif
        }

        /// <summary>
        /// Returns the square root of a specified number.
        /// </summary>
        public static FNum Sqrt(FNum x)
        {
#if FIXED_POINT_NUMBER
            var xl = x.RawValue;
            if (xl < 0)
            {
                // We cannot represent infinities like Single and Double, and Sqrt is
                // mathematically undefined for x < 0. So we just throw an exception.
                throw new Exception($"Negative value passed to Sqrt {x}");
            }

            var num = (ulong)xl;
            var result = 0UL;

            // second-to-top bit
            var bit = 1UL << (NUM_BITS - 2);

            while (bit > num)
            {
                bit >>= 2;
            }

            // The main part is executed twice, in order to avoid
            // using 128 bit values in computations.
            for (var i = 0; i < 2; ++i)
            {
                // First we get the top 48 bits of the answer.
                while (bit != 0)
                {
                    if (num >= result + bit)
                    {
                        num -= result + bit;
                        result = (result >> 1) + bit;
                    }
                    else
                    {
                        result = result >> 1;
                    }
                    bit >>= 2;
                }

                if (i == 0)
                {
                    // Then process it again to get the lowest 16 bits.
                    if (num > (1UL << (NUM_BITS / 2)) - 1)
                    {
                        // The remainder 'num' is too large to be shifted left
                        // by 32, so we have to add 1 to result manually and
                        // adjust 'num' accordingly.
                        // num = a - (result + 0.5)^2
                        //       = num + result^2 - (result + 0.5)^2
                        //       = num - result - 0.5
                        num -= result;
                        num = (num << (NUM_BITS / 2)) - 0x80000000UL;
                        result = (result << (NUM_BITS / 2)) + 0x80000000UL;
                    }
                    else
                    {
                        num <<= NUM_BITS / 2;
                        result <<= NUM_BITS / 2;
                    }

                    bit = 1UL << (NUM_BITS / 2 - 2);
                }
            }
            // Finally, if next bit would have been 1, round the result upwards.
            if (num > result)
            {
                ++result;
            }
            return new FNum((long)result);
#else
            return MathF.Sqrt(x);
#endif
        }

        /// <summary>
        /// Returns the Sine of x.
        /// The relative error is less than 1E-10 for x in [-2PI, 2PI], and less than 1E-7 in the worst case.
        /// </summary>
        public static FNum Sin(FNum x)
        {
#if FIXED_POINT_NUMBER
            var clampedL = ClampSinValue(x.RawValue, out var flipHorizontal, out var flipVertical);
            var clamped = new FNum(clampedL);

            // Find the two closest values in the LUT and perform linear interpolation
            // This is what kills the performance of this function on x86 - x64 is fine though
            var rawIndex = FastMul(clamped, LutInterval);
            var roundedIndex = (int)Round(rawIndex);
            var indexError = FastSub(rawIndex, roundedIndex);

            var nearestValue = new FNum(LUT.SinLut[flipHorizontal ? LUT.SinLut.Length - 1 - roundedIndex : roundedIndex]);
            var secondNearestValue = new FNum(LUT.SinLut[flipHorizontal ? LUT.SinLut.Length - 1 - roundedIndex - Sign(indexError) : roundedIndex + Sign(indexError)]);

            var delta = FastMul(indexError, FastAbs(FastSub(nearestValue, secondNearestValue))).RawValue;
            var interpolatedValue = nearestValue.RawValue + (flipHorizontal ? -delta : delta);
            var finalValue = flipVertical ? -interpolatedValue : interpolatedValue;
            return new FNum(finalValue);
#else
            return MathF.Sin(x);
#endif
        }

        /// <summary>
        /// Returns a rough approximation of the Sine of x.
        /// This is at least 3 times faster than Sin() on x86 and slightly faster than Math.Sin(),
        /// however its accuracy is limited to 4-5 decimals, for small enough values of x.
        /// </summary>
        public static FNum FastSin(FNum x)
        {
#if FIXED_POINT_NUMBER
            var clampedL = ClampSinValue(x.RawValue, out var flipHorizontal, out var flipVertical);

            // Here we use the fact that the SinLut table has a number of entries
            // equal to (PI_OVER_2 >> 15) to use the angle to index directly into it
            var rawIndex = (uint)(clampedL >> 15);
            if (rawIndex >= LUT_SIZE)
            {
                rawIndex = LUT_SIZE - 1;
            }
            var nearestValue = LUT.SinLut[flipHorizontal ? LUT.SinLut.Length - 1 - (int)rawIndex : (int)rawIndex];
            return new FNum(flipVertical ? -nearestValue : nearestValue);
#else
            return MathF.Sin(x);
#endif
        }

#if FIXED_POINT_NUMBER
        private static long ClampSinValue(long angle, out bool flipHorizontal, out bool flipVertical)
        {
            const long largePI = 7244019458077122842;
            // Obtained from ((Fix64)1686629713.065252369824872831112M).m_rawValue
            // This is (2^29)*PI, where 29 is the largest N such that (2^N)*PI < MaxValue.
            // The idea is that this number contains way more precision than PI_TIMES_2,
            // and (((x % (2^29*PI)) % (2^28*PI)) % ... (2^1*PI) = x % (2 * PI)
            // In practice this gives us an error of about 1,25e-9 in the worst case scenario (Sin(MaxValue))
            // Whereas simply doing x % PI_TIMES_2 is the 2e-3 range.

            var clamped2Pi = angle;
            for (var i = 0; i < 29; ++i)
            {
                clamped2Pi %= largePI >> i;
            }
            if (angle < 0)
            {
                clamped2Pi += PI_TIMES_2;
            }

            // The LUT contains values for 0 - PiOver2; every other value must be obtained by
            // vertical or horizontal mirroring
            flipVertical = clamped2Pi >= mPI;
            // obtain (angle % PI) from (angle % 2PI) - much faster than doing another modulo
            var clampedPi = clamped2Pi;
            while (clampedPi >= mPI)
            {
                clampedPi -= mPI;
            }
            flipHorizontal = clampedPi >= PI_OVER_2;
            // obtain (angle % PI_OVER_2) from (angle % PI) - much faster than doing another modulo
            var clampedPiOver2 = clampedPi;
            if (clampedPiOver2 >= PI_OVER_2)
            {
                clampedPiOver2 -= PI_OVER_2;
            }
            return clampedPiOver2;
        }
#endif

        /// <summary>
        /// Returns the cosine of x.
        /// The relative error is less than 1E-10 for x in [-2PI, 2PI], and less than 1E-7 in the worst case.
        /// </summary>
        public static FNum Cos(FNum x)
        {
#if FIXED_POINT_NUMBER
            var xl = x.RawValue;
            var rawAngle = xl + (xl > 0 ? -mPI - PI_OVER_2 : PI_OVER_2);
            return Sin(new FNum(rawAngle));
#else
            return MathF.Cos(x);
#endif
        }

        /// <summary>
        /// Returns a rough approximation of the cosine of x.
        /// See FastSin for more details.
        /// </summary>
        public static FNum FastCos(FNum x)
        {
#if FIXED_POINT_NUMBER
            var xl = x.RawValue;
            var rawAngle = xl + (xl > 0 ? -mPI - PI_OVER_2 : PI_OVER_2);
            return FastSin(new FNum(rawAngle));
#else
            return MathF.Cos(x);
#endif
        }

        /// <summary>
        /// Returns the tangent of x.
        /// </summary>
        /// <remarks>
        /// This function is not well-tested. It may be wildly inaccurate.
        /// </remarks>
        public static FNum Tan(FNum x)
        {
#if FIXED_POINT_NUMBER
            var clampedPi = x.RawValue % mPI;
            var flip = false;
            if (clampedPi < 0)
            {
                clampedPi = -clampedPi;
                flip = true;
            }
            if (clampedPi > PI_OVER_2)
            {
                flip = !flip;
                clampedPi = PI_OVER_2 - (clampedPi - PI_OVER_2);
            }

            var clamped = new FNum(clampedPi);

            // Find the two closest values in the LUT and perform linear interpolation
            var rawIndex = FastMul(clamped, LutInterval);
            var roundedIndex = (int)Round(rawIndex);
            var indexError = FastSub(rawIndex, roundedIndex);

            var nearestValue = new FNum(LUT.TanLut[roundedIndex]);
            var secondNearestValue = new FNum(LUT.TanLut[roundedIndex + Sign(indexError)]);

            var delta = FastMul(indexError, FastAbs(FastSub(nearestValue, secondNearestValue))).RawValue;
            var interpolatedValue = nearestValue.RawValue + delta;
            var finalValue = flip ? -interpolatedValue : interpolatedValue;
            return new FNum(finalValue);
#else
            return MathF.Tan(x);
#endif
        }

        /// <summary>
        /// Returns the arccos of the specified number, calculated using Atan and Sqrt
        /// This function has at least 7 decimals of accuracy.
        /// </summary>
        public static FNum Acos(FNum x)
        {
#if FIXED_POINT_NUMBER
            if (x < -One || x > One)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }

            if (x.RawValue == 0) return PiOver2;

            var result = Atan(Sqrt(One - x * x) / x);
            return x.RawValue < 0 ? result + PI : result;
#else
            return MathF.Acos(x);
#endif
        }

        /// <summary>
        /// Returns the arctan of the specified number, calculated using Euler series
        /// This function has at least 7 decimals of accuracy.
        /// </summary>
        public static FNum Atan(FNum z)
        {
#if FIXED_POINT_NUMBER
            if (z.RawValue == 0) return Zero;

            // Force positive values for argument
            // Atan(-z) = -Atan(z).
            var neg = z.RawValue < 0;
            if (neg)
            {
                z = -z;
            }

            var two = (FNum)2;
            var three = (FNum)3;

            var invert = z > One;
            if (invert) z = One / z;

            var result = One;
            var term = One;

            var zSq = z * z;
            var zSq2 = zSq * two;
            var zSqPlusOne = zSq + One;
            var zSq12 = zSqPlusOne * two;
            var dividend = zSq2;
            var divisor = zSqPlusOne * three;

            for (var i = 2; i < 30; ++i)
            {
                term *= dividend / divisor;
                result += term;

                dividend += zSq2;
                divisor += zSq12;

                if (term.RawValue == 0) break;
            }

            result = result * z / zSqPlusOne;

            if (invert)
            {
                result = PiOver2 - result;
            }

            if (neg)
            {
                result = -result;
            }
            return result;
#else
            return MathF.Atan(z);
#endif
        }

        public static FNum Atan2(FNum y, FNum x)
        {
#if FIXED_POINT_NUMBER
            var yl = y.RawValue;
            var xl = x.RawValue;
            if (xl == 0)
            {
                if (yl > 0)
                {
                    return PiOver2;
                }
                if (yl == 0)
                {
                    return Zero;
                }
                return -PiOver2;
            }
            FNum atan;
            var z = y / x;

            // Deal with overflow
            if (One + 0.28M * z * z == MaxValue)
            {
                return y < Zero ? -PiOver2 : PiOver2;
            }

            if (Abs(z) < One)
            {
                atan = z / (One + 0.28M * z * z);
                if (xl >= 0) return atan;
                if (yl < 0)
                {
                    return atan - PI;
                }
                return atan + PI;
            }
            else
            {
                atan = PiOver2 - z / (z * z + 0.28M);
                if (yl < 0)
                {
                    return atan - PI;
                }
            }
            return atan;
#else
            return MathF.Atan2(y, x);
#endif
        }

        public static FNum Max(FNum a, FNum b) => new(Math.Max(a.RawValue, b.RawValue));
        public static FNum Min(FNum a, FNum b) => new(Math.Min(a.RawValue, b.RawValue));
        #endregion

        public readonly void Z_BytesWrite(ref BytesWriter writer) => writer.Push((float)this);
        public void Z_BytesRead(ref BytesReader reader) => this = (FNum)reader.Read<float>();
        readonly Json5CustomDeserializeResult IJson5Deserializable.JsonDeserialize(ref Json5SyntaxNodes nodes, object otherData, in Json5DeserializeOptionData options) => nodes.TryMoveNextValueOrCloseToken(out var node) ? new Json5CustomDeserializeResult(new FNum(node.ContentSpan)) : false;
        readonly string IJson5Serializable.JsonSerialize(object obj, object customData, int indent, Json5SerializeOptionData opData) => ToString("0.####");

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly override bool Equals(object obj) => obj is FNum num && num.RawValue == RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly bool Equals(FNum other) => RawValue == other.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly override int GetHashCode() => RawValue.GetHashCode();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public readonly int CompareTo(FNum other) => RawValue.CompareTo(other.RawValue);
#if FIXED_POINT_NUMBER
        public readonly string ToString(string format, IFormatProvider formatProvider) => ((decimal)this).ToString(format ?? "0.#####", formatProvider);
        public readonly string ToString(string format) => ((decimal)this).ToString(format ?? "0.#####");
        public readonly override string ToString() => ((decimal)this).ToString("0.#####");
#else
        public readonly string ToString(string format, IFormatProvider formatProvider) => ((float)this).ToString(format, formatProvider);
        public readonly string ToString(string format) => ((float)this).ToString(format);
        public readonly override string ToString() => ((float)this).ToString("0.#####");
#endif
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(FNum x, FNum y) => x.RawValue == y.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator ==(FNum x, int y) => x == (FNum)y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(FNum x, int y) => x != (FNum)y;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator !=(FNum x, FNum y) => x.RawValue != y.RawValue;

#if FIXED_POINT_NUMBER
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FNum operator %(FNum x, FNum y) => new(x.RawValue == MIN_VALUE & y.RawValue == -1 ? 0 : x.RawValue % y.RawValue);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static FNum operator -(FNum x) => x.RawValue == MIN_VALUE ? MaxValue : new FNum(-x.RawValue);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator >(FNum x, FNum y) => x.RawValue > y.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator <(FNum x, FNum y) => x.RawValue < y.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator >=(FNum x, FNum y) => x.RawValue >= y.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static bool operator <=(FNum x, FNum y) => x.RawValue <= y.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator FNum(long value) => new(value * ONE);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator long(FNum value) => value.RawValue >> FRACTIONAL_PLACES;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator FNum(int value) => new(value * ONE);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator int(FNum value) => (int)(value.RawValue >> FRACTIONAL_PLACES);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator FNum(uint value) => new(value * ONE);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator uint(FNum value) => (uint)(value.RawValue >> FRACTIONAL_PLACES);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator FNum(short value) => new(value * ONE);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator short(FNum value) => (short)(value.RawValue >> FRACTIONAL_PLACES);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator FNum(ushort value) => new(value * ONE);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator ushort(FNum value) => (ushort)(value.RawValue >> FRACTIONAL_PLACES);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator FNum(byte value) => new(value * ONE);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator byte(FNum value) => (byte)(value.RawValue >> FRACTIONAL_PLACES);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator FNum(float value) => new((long)(value * ONE));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator float(FNum value) => (float)value.RawValue / ONE;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator FNum(double value) => new((long)(value * ONE));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator double(FNum value) => (double)value.RawValue / ONE;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator FNum(in decimal value) => new((long)(value * ONE));
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator decimal(FNum value) => (decimal)value.RawValue / ONE;

#else
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator FNum(float value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static implicit operator float(FNum value) => value.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator FNum(int value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator int(FNum value) => (int)value.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator FNum(long value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator long(FNum value) => (long)value.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator FNum(short value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator short(FNum value) => (short)value.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator FNum(byte value) => new(value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator byte(FNum value) => (byte)value.RawValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator FNum(decimal value) => new((float)value);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static explicit operator decimal(FNum value) => (decimal)value.RawValue;
#endif


        #region IConvertible
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly TypeCode IConvertible.GetTypeCode() => TypeCode.Single;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly bool IConvertible.ToBoolean(IFormatProvider provider) => this == 1;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly byte IConvertible.ToByte(IFormatProvider provider) => (byte)this;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly char IConvertible.ToChar(IFormatProvider provider) => (char)(byte)this;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly DateTime IConvertible.ToDateTime(IFormatProvider provider) => TimeHelper.Default.TimestampToDate((uint)this);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly decimal IConvertible.ToDecimal(IFormatProvider provider) => (decimal)this;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly double IConvertible.ToDouble(IFormatProvider provider) => this;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly short IConvertible.ToInt16(IFormatProvider provider) => (short)this;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly int IConvertible.ToInt32(IFormatProvider provider) => (int)this;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly long IConvertible.ToInt64(IFormatProvider provider) => (long)this;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly sbyte IConvertible.ToSByte(IFormatProvider provider) => (sbyte)this;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly float IConvertible.ToSingle(IFormatProvider provider) => this;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly string IConvertible.ToString(IFormatProvider provider) => ToString("", provider);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly object IConvertible.ToType(Type conversionType, IFormatProvider provider) => typeof(FNum);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly ushort IConvertible.ToUInt16(IFormatProvider provider) => (ushort)this;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly uint IConvertible.ToUInt32(IFormatProvider provider) => (uint)this;
        [MethodImpl(MethodImplOptions.AggressiveInlining)] readonly ulong IConvertible.ToUInt64(IFormatProvider provider) => (ulong)this;
        #endregion
    }
}
