using System;
using System.Collections.Generic;

namespace foundation
{
    public class CallReferrer
    {
        public Action<CallReferrer> callBack;

        public object[] parms;
        public CallReferrer()
        {
        }

        public void execute()
        {
            if (callBack != null)
            {
                callBack(this);
            }
        }

        public static int MAX = 10;
        private static Queue<CallReferrer> pool = new Queue<CallReferrer>();

        public static CallReferrer Get(Action<CallReferrer> callBack=null,params object[] args)
        {
            CallReferrer v;

            if (pool.Count > 0)
            {
                v = pool.Dequeue();
            }
            else
            {
                v = new CallReferrer();
            }
            v.parms = args;
            v.callBack = callBack;
            return v;
        }

        public static CallReferrer GetToggleMediator(IMediator mediator, Action<CallReferrer> callBack=null, params object[] args)
        {
            CallReferrer referrer = Get(null);

            referrer.callBack = (CallReferrer re) =>
            {
                Facade.ToggleMediator(mediator.name);
                if (callBack != null) callBack(re);
            };
            referrer.parms = args;
            return referrer;
        }

        public static CallReferrer GetToggleMediator<T>(Action<CallReferrer> callBack = null,params object[] args) where T: IMediator
        {
            T m = Facade.GetMediator<T>();
            return GetToggleMediator(m, callBack, args);
        }


        public static void Recycle(CallReferrer value)
        {
            if (pool.Count > MAX)
            {
                return;
            }
            value.callBack = null;
            value.parms= null;
            pool.Enqueue(value);
        }
    }
}