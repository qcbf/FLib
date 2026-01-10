//==================={By Qcbf|qcbf@qq.com|5/30/2021 4:21:36 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    [BytesPackGen]
    public partial class VLDynamicFields
    {
        [BytesPackGenField] public Dictionary<string, ObjectBytesPackWrap> Values = new();
        public int Count => Values.Count;

        public VLValueBase this[string name]
        {
            get => (VLValueBase)Values[name].Value;
            set => Values[name] = new ObjectBytesPackWrap(value);
        }
    }
}
