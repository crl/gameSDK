using System;
using System.Collections.Generic;

namespace foundation
{
    public interface IView<T> where T:IMVCHost
    {
        /// <summary>
        ///  注册一个视图中介; 
        /// </summary>
        /// <param name="mediator"></param>
		void register( T value );


        /// <summary>
        /// 取得 一个视图中介;
        /// </summary>
        /// <param name="mediatorName"></param>
        /// <returns></returns>
        T get( string name );
		
		/// <summary>
		/// 删除一个视图中介;
		/// </summary>
		/// <param name="mediatorName"></param>
		/// <returns></returns>
		T remove( string name ) ;
		
		/// <summary>
		/// 是否存在相应的视图中介;
		/// </summary>
		/// <param name="mediatorName"></param>
		/// <returns></returns>
		bool has( string name ) ;

        /// <summary>
        /// 清理它管理的所有内容; 
        /// </summary>
        void clear();

        /// <summary>
        /// 清理缓存数据
        /// </summary>
        void clearCache();

        ASDictionary<string, T> all();
    }
}
