using gameSDK;
using UnityEngine;

namespace foundation
{
    public interface IDelegateContainer
    {
        T addDelegate<T>(GameObject skin) where T : PanelDelegate,new();
    }
}