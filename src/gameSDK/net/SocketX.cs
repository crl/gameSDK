using System;
using System.Collections.Generic;

namespace foundation
{
    public class SocketX:ISocket
    {
        protected static Dictionary<string, HashSet<int>> SpecialCMDMap = new Dictionary<string, HashSet<int>>();
        protected ISocketSender sender;
        protected SocketRouter _router = new SocketRouter();
        private static SocketX instance;

        public static void AddSpecialCMD(string key, params int[] cmds)
        {
            HashSet<int> hashSet = null;
            if (SpecialCMDMap.TryGetValue(key, out hashSet) == false)
            {
                hashSet = new HashSet<int>();
                SpecialCMDMap.Add(key, hashSet);
            }
            foreach (int cmd in cmds)
            {
                hashSet.Add(cmd);
            }
        }

        public static void RemovSpecialCMD(string key,params int[] cmds)
        {
            HashSet<int> hashSet = null;
            if (SpecialCMDMap.TryGetValue(key, out hashSet) == false)
            {
                return;
            }
            foreach (int cmd in cmds)
            {
                hashSet.Remove(cmd);
            }
        }

        public static bool IsInSpecialCMD(string key,int cmd)
        {
            HashSet<int> hashSet = null;
            if (SpecialCMDMap.TryGetValue(key, out hashSet) == false)
            {
                return false;
            }
            return hashSet.Contains(cmd);
        }

        public static void ClearSpecialCMD(string key)
        {
            HashSet<int> hashSet = null;
            if (SpecialCMDMap.TryGetValue(key, out hashSet) == false)
            {
                return;
            }
            hashSet.Clear();
            SpecialCMDMap.Remove(key);
        }

        public static void bindSender(ISocketSender sender)
        {
            GetInstance();
            instance.sender = sender;
            sender.router = instance._router;
        }

        public bool isConnected
        {
            get
            {
                return sender.connected;
            }
        }

        public void addListener(int CMD, Action<IMessageExtensible> handler)
        {
            _router.addListener(CMD,handler);
        }

        public void addListenerOnce(int CMD, Action<IMessageExtensible> onceHandler)
        {
            _router.addListenerOnce(CMD, onceHandler);
        }


        public void removeListener(int CMD, Action<IMessageExtensible> handler)
        {
            _router.removeListener(CMD, handler);
        }

        public void close()
        {
            if (sender != null)
            {
                sender.close();
            }
        }
        public bool send(IMessageExtensible message)
        {            
            return sender.send(message);
        }

        public void route(IMessageExtensible msg)
        {
            _router.dispatch(msg);
        }

        public static bool IsConnected
        {
            get
            {
                return instance.isConnected;
            }
        }

        public static bool AddEventListener(string type, Action<EventX> listener, int priority = 0)
        {
            if (instance.sender == null)
            {
                return false;
            }
            return instance.sender.addEventListener(type, listener);
        }
        public static bool RemoveEventListener(string type, Action<EventX> listener)
        {
            if (instance.sender == null)
            {
                return false;
            }
            return instance.sender.removeEventListener(type, listener);
        }

        public static void AddListener(int CMD, Action<IMessageExtensible> handler)
        {
            instance.addListener(CMD,handler);
        }
        private static void AddListenerOnce(int CMD, Action<IMessageExtensible> handler)
        {
            instance.addListenerOnce(CMD, handler);
        }

        public static void RemoveListener(int CMD, Action<IMessageExtensible> handler)
        {
            instance.removeListener(CMD, handler);
        }

        public static bool Send(IMessageExtensible msg, Action<IMessageExtensible> onceHandler=null)
        {
            if (onceHandler != null)
            {
                instance.addListenerOnce(msg.getMessageType(), onceHandler);
            }
            return instance.send(msg);
        }

        public static void Close()
        {
            if (instance != null)
            {
                instance.close();
            }
        }

        private static Dictionary<int,Action> pushLaterSet=new Dictionary<int, Action>();

        public static void SendLaterRetry(IMessageExtensible msg, int tryCount = 3, float laterTime = 1.0f,
            bool isFirst = true)
        {
            if (tryCount < 1)
            {
                tryCount = 1;
            }

            int cmd = msg.getMessageType();
            ClearLater(cmd);

            Action autoSendAction = () =>
            {
                Action<IMessageExtensible> b = null;
                if (tryCount-- > 0)
                {
                    b = (IMessageExtensible result) =>
                    {
                        if (instance.sender.checkRetry(result))
                        {
                            SendLaterRetry(msg, tryCount, laterTime *2f, false);
                        }
                        else
                        {
                            ClearLater(cmd);
                        }
                    };
                }
                else
                {
                    ClearLater(cmd);
                }
                Send(msg, b);
            };

            if (isFirst)
            {
                autoSendAction();
            }
            else
            {
                pushLaterSet.Add(cmd, autoSendAction);
                CallLater.Add(autoSendAction, laterTime);
            }
        }

        public static void ClearLater(int cmd)
        {
            Action autoSendAction = null;
            if (pushLaterSet.TryGetValue(cmd, out autoSendAction))
            {
                pushLaterSet.Remove(cmd);
                CallLater.Remove(autoSendAction);
            }
        }

        public static void LaterSend(IMessageExtensible msg,float laterSecond = 1.0f, Action<IMessageExtensible> onceHandle= null)
        {
            CallLater.Add(
                ()=>Send(msg, onceHandle),
                laterSecond);
        }

        public static void Route(IMessageExtensible msg)
        {
            instance.route(msg);
        }

        public static ISocket GetInstance()
        {
            if (instance == null)
            {
                instance = new SocketX();
            }
            return instance;
        }
    }
}