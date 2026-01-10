// ==================== qcbf@qq.com | 2025-09-12 ====================

using System;
using System.Threading.Tasks;
using FLib;

namespace FLib.Net
{
    public interface IFNetCallbackable
    {
        void Invoke(FNetChannel channel, FNetProcessor processor);
    }
}
