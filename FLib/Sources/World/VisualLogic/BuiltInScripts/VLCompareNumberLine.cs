//==================={By Qcbf|qcbf@qq.com|5/30/2021 4:49:45 PM}===================

using System;
#if UNITY_PROJ
using UnityEngine;
#endif

namespace FLib.Worlds.BuiltInScripts
{
    [BytesPackGen, Comment("比较数字")]
    public partial class VLCompareNumberLine : VLBaseLineScript
    {
        [BytesPackGenField, VLFieldComment("值1")]
        public VLValue<double> V1;

        [BytesPackGenField, VLFieldComment("值2")]
        public VLValue<double> V2;

        [BytesPackGenField, VLFieldComment("符号")]
        public ECalcOperator Operator;

        [Flags]
        public enum ECalcOperator : byte
        {
#if UNITY_PROJ
            [InspectorName("等于")]
#endif
            Equals = 0x1,
#if UNITY_PROJ
            [InspectorName("大于")]
#endif
            Greater = 0x2,
#if UNITY_PROJ
            [InspectorName("小于")]
#endif
            Less = 0x4,
        }


        public override bool Handle()
        {
            var v1 = V1.Value;
            var v2 = V2.Value;
            if ((Operator & ECalcOperator.Equals) != 0 && v1 == v2) return true;
            if ((Operator & ECalcOperator.Greater) != 0 && v1 > v2) return true;
            return (Operator & ECalcOperator.Less) != 0 && v1 < v2;
        }
    }
}