using foundation;

namespace gameSDK
{
    public class ActionBase
    {
        public string type
        {
            get;
            protected set;
        }

        public ActionBase()
        {
            
        }

        protected static ASDictionary GetASDictionary(string type)
        {
            ASDictionary dic = ASDictionary.Get();
            dic["type"] = type;
            return dic;
        }

        public virtual bool doAction(ASDictionary dic)
        {
            return false;
        }
    }
}