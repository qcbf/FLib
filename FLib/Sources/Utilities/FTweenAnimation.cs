//==================={By Qcbf|qcbf@qq.com|7/26/2021 6:22:04 PM}===================

using System;

namespace FLib
{
    public struct FTweenAnimation
    {
        public FNum Delay;
        public EEaseType Ease;
        public FNum Progress;

        private FNum _progressDeltaScale;

        public FNum Duration
        {
            readonly get => FNum.One / _progressDeltaScale;
            set => _progressDeltaScale = FNum.One / value;
        }

        public readonly bool IsWorking => Progress >= 0 && Progress <= 1 && _progressDeltaScale != 0;

        public enum EEaseType : byte
        {
            None,

            Linear,

            InQuad,
            OutQuad,
            InOutQuad,

            InExpo,
            OutExpo,
            InOutExpo,

            InBack,
            OutBack,
            InOutBack,

            InBounce,
            OutBounce,
            InOutBounce,
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Update(FNum deltaTime, out FNum v)
        {
            v = default;
            if (!IsWorking)
                return false;

            if (Delay > 0)
            {
                Delay -= deltaTime;
                return true;
            }

            v = MathEx.Clamp(Tween(Ease, Progress += deltaTime * _progressDeltaScale), 0, 1);
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reverse()
        {
            _progressDeltaScale = -_progressDeltaScale;
            if (Progress == 1)
                Progress = 0;
            else if (Progress == 0)
                Progress = 1;
        }

        #region helper
        public static FNum Tween(EEaseType type, FNum t, FNum from, FNum to)
        {
            return from + (to - from) * Tween(type, t);
        }

        public static FNum Tween(EEaseType type, FNum t)
        {
            return type switch
            {
                EEaseType.InQuad => EaseInQuad(t),
                EEaseType.OutQuad => EaseOutQuad(t),
                EEaseType.InOutQuad => EaseInOutQuad(t),
                EEaseType.InExpo => EaseInExpo(t),
                EEaseType.OutExpo => EaseOutExpo(t),
                EEaseType.InOutExpo => EaseInOutExpo(t),
                EEaseType.InBack => EaseInBack(t),
                EEaseType.OutBack => EaseOutBack(t),
                EEaseType.InOutBack => EaseInOutBack(t),
                EEaseType.InBounce => EaseInBounce(t),
                EEaseType.OutBounce => EaseOutBounce(t),
                EEaseType.InOutBounce => EaseInOutBounce(t),
                _ => t
            };
        }

        #region Bounce
        // ReSharper disable InconsistentNaming
        private static readonly FNum N2_5 = 25 * FNum.Tenth;
        private static readonly FNum N1_5 = 15 * FNum.Tenth;
        private static readonly FNum N2_75 = 275 * FNum.Hundredth;
        private static readonly FNum N7_5625 = 75625 / (FNum)10000;
        private static readonly FNum N0_75 = 75 * FNum.Hundredth;
        private static readonly FNum N0_9375 = 9375 / (FNum)10000;
        private static readonly FNum N2_25 = 225 * FNum.Hundredth;
        private static readonly FNum N2_625 = 2625 * FNum.Thousandth;
        private static readonly FNum N0_984375 = 984375 / (FNum)1000000;
        private static readonly FNum N1_70158 = 170158 / (FNum)100000;
        private static readonly FNum N1_525 = 1_525 * FNum.Thousandth;

        private static FNum EaseInOutBounce(FNum t)
        {
            if (t < FNum.OneHalf) return EaseInBounce(t * 2) * FNum.OneHalf;
            return EaseOutBounce(t * 2 - FNum.One) * FNum.OneHalf + FNum.OneHalf;
        }

        private static FNum EaseOutBounce(FNum t)
        {
            if (t < FNum.One / N2_75)
            {
                return FNum.One * (N7_5625 * t * t);
            }
            if (t < 2 / N2_75)
            {
                t -= N1_5 / N2_75;
                return FNum.One * (N7_5625 * t * t + N0_75);
            }
            if (t < N2_5 / N2_75)
            {
                t -= N2_25 / N2_75;
                return FNum.One * (N7_5625 * t * t + N0_9375);
            }
            t -= N2_625 / N2_75;
            return FNum.One * (N7_5625 * t * t + N0_984375);
        }

        private static FNum EaseInBounce(FNum t)
        {
            return FNum.One - EaseOutBounce(FNum.One - t);
        }
        #endregion

        #region Back
        private static FNum EaseInOutBack(FNum t)
        {
            var v = N1_70158;
            t *= 2;
            if (t < FNum.One)
            {
                v *= N1_525;
                return FNum.One / 2 * (t * t * ((v + FNum.One) * t - v));
            }

            t -= 2;
            v *= N1_525;
            return FNum.One / 2 * (t * t * ((v + FNum.One) * t + v) + 2);
        }

        private static FNum EaseOutBack(FNum t)
        {
            t -= FNum.One;
            return t * t * ((N1_70158 + 1) * t + N1_70158) + 1;
        }

        private static FNum EaseInBack(FNum t)
        {
            return t * t * ((N1_70158 + 1) * t - N1_70158);
        }
        #endregion

        #region Expo
        private static FNum EaseInOutExpo(FNum t)
        {
            return t == 0 ? 0 : t == 1 ? 1 : t < FNum.OneHalf ? FNum.Pow(2, 20 * t - 10) / 2 : (2 - FNum.Pow(2, -20 * t + 10)) / 2;
        }

        private static FNum EaseOutExpo(FNum t)
        {
            return t == 1 ? 1 : 1 - FNum.Pow(2, -10 * t);
        }

        private static FNum EaseInExpo(FNum t)
        {
            return t == 0 ? 0 : FNum.Pow(2, 10 * t - 10);
        }
        #endregion

        #region Quad
        private static FNum EaseInOutQuad(FNum t)
        {
            t *= 2;
            if (t < FNum.One) return FNum.OneHalf * t * t;
            t -= FNum.One;
            return -FNum.One / 2 * (t * (t - 2) - FNum.One);
        }

        private static FNum EaseOutQuad(FNum t)
        {
            return -FNum.One * t * (t - 2);
        }

        private static FNum EaseInQuad(FNum t)
        {
            return FNum.One * t * t;
        }
        #endregion
        #endregion
    }
}
