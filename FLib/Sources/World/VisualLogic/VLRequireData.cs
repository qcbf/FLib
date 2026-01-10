//==================={By Qcbf|qcbf@qq.com|9/3/2021 4:23:21 PM}===================

using FLib;
using System;
using System.Collections.Generic;

namespace FLib.Worlds
{
    [BytesPackGen]
    public partial struct VLRequireData : ICloneable<VLRequireData>
    {
        [BytesPackGenField] public byte[] EnvData;
        [BytesPackGenField] public Dictionary<string, ObjectBytesPackWrap> Variables;

        public bool IsValid => EnvData?.Length > 0;

        public VLValueBase this[string name]
        {
            get => (VLValueBase)Variables[name].Value;
            set => Variables[name] = new ObjectBytesPackWrap(value);
        }

        /// <summary>
        /// 
        /// </summary>
        public void CopyVariablesFrom(SlimDictionary<string, ObjectBytesPackWrap> srcVariables)
        {
            if (srcVariables != null)
            {
                Variables ??= new Dictionary<string, ObjectBytesPackWrap>(srcVariables.Count);
                foreach (var item in srcVariables)
                    Variables[item.Key] = item.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public VLEnvironment CreateWithStartupEnvironment(object runtimeData, string debugInfo)
        {
            var vl = CreateEnvironment(debugInfo);
            if (vl != null)
            {
                vl.RuntimeData = runtimeData;
                vl.Startup();
            }
            return vl;
        }

        /// <summary>
        /// 
        /// </summary>
        public VLEnvironment CreateEnvironment(string debugInfo)
        {
            if (!IsValid) return null;
            var vl = new VLEnvironment();
            try
            {
                BytesPack.Unpack(ref vl, EnvData, debugInfo);
            }
            catch (Exception ex)
            {
                throw new Exception($"create visual logic error {debugInfo}\n{ex}");
            }
            if (Variables != null)
            {
                foreach (var item in vl.Variables.Values)
                {
                    if (Variables.TryGetValue(item.Key, out var val))
                    {
                        vl.Variables[item.Key] = ((VLValueBase)val.Value).Clone();
                    }
                }
            }
            vl.DebugInfo = debugInfo;
            return vl;
        }

        public VLRequireData Clone()
        {
            var req = new VLRequireData
            {
                EnvData = EnvData?.Clone() as byte[],
                Variables = new Dictionary<string, ObjectBytesPackWrap>(Variables),
            };
            return req;
        }
    }
}
