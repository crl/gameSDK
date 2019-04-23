using System;
using System.Collections.Generic;

namespace foundation
{
    public class SocketRouter
    {
        private Dictionary<int, List<ListenerBox<IMessageExtensible>>> eventsMap;
        private Dictionary<int, List<Action<IMessageExtensible>>> onceListenerMaps;

        public SocketRouter()
        {
            eventsMap = new Dictionary<int, List<ListenerBox<IMessageExtensible>>>();
            onceListenerMaps = new Dictionary<int, List<Action<IMessageExtensible>>>();
        }

        public bool addListener(int code, Action<IMessageExtensible> handle, int priority = 0)
        {
            List<ListenerBox<IMessageExtensible>> list;
            ListenerBox<IMessageExtensible> listenerBox;

            if (eventsMap.TryGetValue(code, out list) == false)
            {
                list = new List<ListenerBox<IMessageExtensible>>();
                eventsMap.Add(code, list);

                listenerBox = new ListenerBox<IMessageExtensible>(handle, priority);

                list.Add(listenerBox);
                return true;
            }

            int i = 0;
            int len = list.Count;

            while (i < len)
            {
                listenerBox = list[i];
                if (listenerBox.listener == handle)
                {
                    if (listenerBox.priority == priority)
                    {
                        return false;
                    }

                    list.RemoveAt(i);
                    len--;
                    break;
                }
                i++;
            }

            listenerBox = new ListenerBox<IMessageExtensible>(handle, priority);

            for (i = 0; i < len; i++)
            {
                if (priority > list[i].priority)
                {
                    list.Insert(i, listenerBox);
                    return true;
                }
            }

            list.Add(listenerBox);

            return true;
        }


        internal bool addListenerOnce(int code, Action<IMessageExtensible> onceHandler)
        {
            List<Action<IMessageExtensible>> list;

            if (onceListenerMaps.TryGetValue(code, out list) == false)
            {
                list = new List<Action<IMessageExtensible>>();
                list.Add(onceHandler);
                onceListenerMaps.Add(code, list);
                return true;
            }

            if (list.IndexOf(onceHandler) == -1)
            {
                list.Add(onceHandler);
                return true;
            }

            return false;
        }


        public bool hasListener(int code)
        {
            return eventsMap.ContainsKey(code);
        }

        public bool removeListener(int code, Action<IMessageExtensible> handle)
        {
            List<ListenerBox<IMessageExtensible>> list;

            if (eventsMap.TryGetValue(code, out list) == false)
            {
                return false;
            }

            ListenerBox<IMessageExtensible> listenerBox;
            int len = list.Count;
            int i = 0;

            while (i < len)
            {
                listenerBox = list[i];
                if (listenerBox.listener.Equals(handle))
                {
                    list.RemoveAt(i);
                    break;
                }
                else
                {
                    i++;
                }
            }

            if (list.Count == 0)
            {
                eventsMap.Remove(code);
            }

            return true;
        }


        public bool dispatch(IMessageExtensible e)
        {
            bool result = false;
            int code = e.getMessageType();
            List<ListenerBox<IMessageExtensible>> list;

            if (eventsMap.TryGetValue(code, out list))
            {
                foreach (ListenerBox<IMessageExtensible> listenerBox in list.ToArray())
                {
                    listenerBox.listener(e);
                }
                result = true;
            }

            List<Action<IMessageExtensible>> onceList;
            if (onceListenerMaps.TryGetValue(code, out onceList))
            {
                onceListenerMaps.Remove(code);
                foreach (Action<IMessageExtensible> item in onceList.ToArray())
                {
                    item(e);
                }
                result = true;
            }
            return result;
        }
    }
}
