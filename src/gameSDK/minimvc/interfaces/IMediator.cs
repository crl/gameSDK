using clayui;
using System;

namespace foundation
{
    public interface IMediator: IMVCHost 
    {
        CallReferrer callReferrer { get; set; }

        void setView(IPanel value);
		
		IPanel getView();
		
		void setModel(IProxy value);
			
		IProxy getModel();

        void toggleSelf(int type = -1);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="data"></param>
        /// <param name="isShowView">是否显示视图</param>
        void execute<T>(Action<T> action, T data = default(T), bool isShowView = true);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="data"></param>
        /// <param name="isShowView"></param>
        void execute(string eventType, object data = null, bool isShowView = true);
    }
}
