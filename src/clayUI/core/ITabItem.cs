using foundation;
using System;
using UnityEngine;

namespace clayui
{
    public interface ITabItem : IEventDispatcher
    {
       /// <summary>
        /// 相关视图; 
       /// </summary>
		GameObject target{
            get;
            set;
        }

        void show(GameObject container=null);
        void hide();

        bool isShow { get; }

        /// <summary>
        ///  是否可用;
        /// </summary>
        bool enabled{
            get;
            set;
        }
		
		/// <summary>
        /// 索引; 
		/// </summary>
        int index
        {
            get;
            set;
        }
    }
}
