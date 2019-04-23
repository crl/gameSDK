using foundation;
using System;
using UnityEngine;

namespace foundation
{
    public interface IPanel:ISkinable,IDisposable,IDataRenderer,IEventDispatcher,IResizeable, IDelegateContainer
    {
        /// <summary>
        /// 展示出来
        /// </summary>
		void show(GameObject go=null,bool isModel=false);

        void setParent(GameObject parent);

        /**
		 * 是否是展示状态 
		 * @return 
		 * 
		 */
        bool isShow
        {
            get;
        }

        IMediator __refMediator
        {
            set;
            get;
        }

		/**
		 * 弹到最顶层; 
		 * 
		 */		
		void bringTop();

        /// <summary>
        /// panel可以存在的场景状态
        /// </summary>
        /// <returns></returns>
        ASList<string> getSceneState();

       /// <summary>
       /// 
       /// </summary>
       /// <param name="mapType"></param>
       /// <param name="panelState"></param>
        void changeState(int mapType,string panelState);


        bool activeInHierarchy { get; }

        /**
		 * 隐藏 
		 * @param event
		 * 
		 */
        void hide(EventX e=null);
    }
}
