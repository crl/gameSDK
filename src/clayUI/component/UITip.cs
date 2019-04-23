using foundation;
using gameSDK;

namespace clayui
{
    public interface IUITip
    {
        void show(string message);
        void show(AbstractItemVO vo, params AbstractItemVO[] args);
    }
    public class UITip
    {
        private static IUITip impl;

        public static void SetImpl(IUITip value)
        {
            impl = value;
        }
        public static void Show(string message)
        {
            if (impl != null)
            {
                impl.show(message);
            }
            else
            {
                DebugX.Log("not implement UITip:"+message);
            }
        }

        public static void ShowCode(string code,params object[] args)
        {
            string m = RFMessage.getMessage(code,args);
            if (string.IsNullOrEmpty(m) == false)
            {
                Show(m);
            }
            else
            {
                DebugX.Log("not implement UITip:" + code);
            }
        }
        public static void ShowCode(int code, params object[] args)
        {
            string m = RFMessage.getMessage(code,args);
            if (string.IsNullOrEmpty(m) == false)
            {
                Show(m);
            }
            else
            {
                DebugX.Log("not implement UITip:" + code);
            }
        }

        public static void Show(AbstractItemVO vo,params AbstractItemVO[] args)
        {
            if (impl != null)
            {
                impl.show(vo,args);
            }
            else
            {
                DebugX.Log("not implement UITip:" +vo);
            }
        }
    }
}