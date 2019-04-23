using System;

namespace foundation
{
    public interface IMVCHost: IEventInterester, IInjectable, IAsync
    {
        /// <summary>
        /// 名称,用于与其它对像的区分;
        /// </summary>
        string name { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="data"></param>
        void execute<T>(Action<T> action, T data = default(T));

        /// <summary>
        ///  当被注册到应用中时触发; 
        /// </summary>
        void onRegister();

        /// <summary>
        /// 当从应用中删除时触发; 
        /// </summary>
        void onRemove();

        /// <summary>
        /// 清理缓存
        /// </summary>
        void onClearCache();
    }
}