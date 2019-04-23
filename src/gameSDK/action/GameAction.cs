using System.Collections.Generic;
using foundation;

namespace gameSDK
{
    public class GameAction
    {
        protected static Dictionary<string, ActionBase> _actionDic=new Dictionary<string, ActionBase>();

        /// <summary>
        ///  注册action 
        /// </summary>
        /// <param name="parser"></param>
        public static void RegAction<T>() where T:ActionBase,new()
        {
            ActionBase parser=new T();
            _actionDic[parser.type] = parser;
        }

        /// <summary>
        /// 执行action 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <returns>是否成功执行</returns>
        public static bool DoAction(ASDictionary data, string type = null)
        {
            if (type == null)
            {
                type = (string) data["type"];
            }

            ActionBase parser = null;
            if (_actionDic.TryGetValue(type, out parser))
            {
                return parser.doAction(data);
            }

            return false;
        }
    }
}