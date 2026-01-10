// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;
using System.Diagnostics;

namespace FLib.Worlds
{
    [Conditional("DEBUG"), AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class VLFieldComment : CommentAttribute
    {
        public bool IsVariableOnly;

        public VLFieldComment(string name, string detail = "") : base(name, detail)
        {
        }
    }
}
