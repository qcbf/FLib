// ==================== qcbf@qq.com | 2025-07-01 ====================

using System;
using System.Diagnostics;

namespace FLib.Worlds
{
    [Conditional("DEBUG"), AttributeUsage(AttributeTargets.Class)]
    public class TimeLogicEditorAttribute : CommentAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsAllowPreview = true;

        /// <summary>
        /// 
        /// </summary>
        public Type RequiredRuntime;

        /// <summary>
        /// 
        /// </summary>
        public string TrackTypeNameMatch;

        /// <summary>
        /// void OnPreviewEnter(object ui){}
        /// </summary>
        public string EnterPreviewMethod;

        /// <summary>
        /// void OnPreviewExit(object ui){}
        /// </summary>
        public string ExitPreviewMethod;

        public TimeLogicEditorAttribute(string name, string detail = "") : base(name, detail)
        {
        }
    }
}
