using foundation;
using UnityEngine;

namespace gameSDK
{
    public interface IAIState:IEventDispatcher 
    {
        /// <summary>
        /// 是否初始化完成 
        /// </summary>
        bool initialized
        {
            get;
        }

        string nextState
        {
            get;
        }


        AIStateMachine stateMachine
        {
            get; set; }


        /// <summary>
        /// 只做一次调用; 
        /// </summary>
        void initialize();

        /// <summary>
        /// 状态标识; 
        /// </summary>

        string type
        {
            get;
        }

        void update();

        /// <summary>
        /// 退出当前状态时; 
        /// </summary>

        void exit();

        /// <summary>
        /// 进入当前状态; 
        /// </summary>

        void enter();


        bool changeState(string type);

        void setAgent(GameObject agent);

        /// <summary>
        /// 更新属性，这样有些动态赋值的属性就不需要在update方法里面重复获取
        /// </summary>
        void updateProperties();
    }
}