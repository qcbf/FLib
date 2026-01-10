//==================={By Qcbf|qcbf@qq.com|7/26/2021 6:12:11 PM}===================

using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;

namespace FLib
{
    public unsafe struct FRandom
    {
        [ThreadStatic] private static SharedWrap _shared;
        public static ref FRandom Shared => ref (_shared ??= new SharedWrap() { Val = Create() }).Val;

        private class SharedWrap
        {
            public FRandom Val;
        }


        private const int MBig = int.MaxValue;

        private int _next;
        private int _nextP;
        private fixed int _seedArray[56];


        public static FRandom Create(int? seed = null)
        {
            var r = new FRandom();
            r.SetSeed(seed ?? Environment.TickCount ^ (int)TimeHelper.Timestamp);
            return r;
        }

        public void SetSeed()
        {
            SetSeed(Environment.TickCount ^ (int)TimeHelper.Timestamp);
        }

        public void SetSeed(int seed)
        {
            var mj = 161803398 - seed;
            _seedArray[55] = mj;
            var mk = 1;
            for (var i = 1; i < 55; i++)
            {
                var ii = 21 * i % 55;
                _seedArray[ii] = mk;
                mk = mj - mk;
                if (mk < 0) mk += MBig;
                mj = _seedArray[ii];
            }

            for (var k = 1; k < 5; k++)
            {
                for (var i = 1; i < 56; i++)
                {
                    _seedArray[i] -= _seedArray[1 + (i + 30) % 55];
                    if (_seedArray[i] < 0) _seedArray[i] += MBig;
                }
            }

            _next = 0;
            _nextP = 21;
        }

        private int Generate()
        {
            var locINext = _next;
            var locINextP = _nextP;

            if (++locINext >= 56) locINext = 1;
            if (++locINextP >= 56) locINextP = 1;

            var retVal = _seedArray[locINext] - _seedArray[locINextP];

            if (retVal == MBig) retVal--;
            if (retVal < 0) retVal += MBig;

            _seedArray[locINext] = retVal;

            _next = locINext;
            _nextP = locINextP;

            return retVal;
        }

        /// <summary>
        /// 
        /// </summary>
        public FVector2 NextCircle()
        {
            var r = NextNumber() * (2 * FNum.PI);
            return new FVector2(FNum.Cos(r), FNum.Sin(r));
        }

        /// <summary>
        /// 
        /// </summary>
        public int Next()
        {
            return Generate();
        }

        /// <summary>
        /// 
        /// </summary>
        public int Next(int min, int max) => Generate() / (int.MaxValue / (max - min)) + min;

        /// <summary>
        /// 
        /// </summary>
        public int Next(in Range range) => Next(range.Start.Value, range.End.Value + 1);

        /// <summary>
        /// 
        /// </summary>
        public FNum NextNumber()
        {
            return (FNum)Generate() / int.MaxValue;
        }

        /// <summary>
        /// 
        /// </summary>
        public FNum NextNumber(FNum min, FNum max)
        {
            return NextNumber() * (max - min) + min;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool NextBool()
        {
            return Generate() < int.MaxValue >> 1;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool NextBool(FNum prob)
        {
            return Generate() < int.MaxValue * prob;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max">不包含</param>
        /// <param name="weights">权重组</param>
        /// <param name="weightsSum">权重总和</param>
        public int NextSegmentValue(int min, int max, Span<int> weights, int weightsSum = 100)
        {
            if (max <= min) return min;
            var total = max - min;
            if (total == 0)
                return min;
            if (weights.Length > total)
            {
                if (total == 1)
                    return Next(0, weights[0] + weights[1]) < weights[0] ? min : max;
                for (var i = weights.Length - 1; i >= total; i--)
                    weightsSum -= weights[i];
                weights = weights[..total];
            }
            var segmentValue = (FNum)total / weights.Length;
            return (int)(segmentValue * NextWeightIndex(weights, weightsSum) + NextNumber(0, segmentValue) + min);
        }

        /// <summary>
        /// weight random
        /// </summary>
        /// <returns>weight index</returns>
        public int NextWeightIndex(in Span<int> weights, int weightsSum = 100)
        {
            // var randomWeight = Random.Range(0, weightSum);
            // var begin = 0;
            // var end = pieces.Count - 1;
            // while (begin <= end)
            // {
            //     var m = begin + ((end - begin) >> 1);
            //     var weight = pieces[m].Weight;
            //     if (randomWeight < weight)
            //         end = m - 1;
            //     else if (randomWeight > weight)
            //         begin = m + 1;
            //     else
            //         return m;
            // }

            // return begin < pieces.Count ? begin : pieces.Count - 1;
            
            
            if (weights.Length < 1) return 0;
#if DEBUG
            var expectSum = weights[0];
            for (var i = 1; i < weights.Length; i++)
                expectSum += weights[i];
            if (expectSum != weightsSum)
                throw new Exception($"weight error actual:{weightsSum}, expected:{expectSum}, weights:{string.Join(',', weights.ToArray().Take(20))}");
#endif

            var weight = Next(0, weightsSum);
            for (var i = 0; i < weights.Length; i++)
            {
                if (weight < weights[i])
                    return i;
                weight -= weights[i];
            }

            return 0;
        }
    }
}
