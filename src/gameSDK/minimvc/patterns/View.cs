using System;

namespace foundation
{
    public class View<T>:IView<T> where T:IMVCHost
    {
        protected ASDictionary<string, T> map ;

        public View( )
		{
            map = new ASDictionary<string, T>();
		}

        public void register(T value)
        {
            String name = value.name;
			if ( map.ContainsKey(name) ) {
				throw new Exception("重复定义:"+name);
			}
			map[ name ] = value;
            value.onRegister();
        }

        public T get(string name)
        {
            if (string.IsNullOrEmpty(name) == false)
            {
                T mediator;
                if (map.TryGetValue(name, out mediator))
                {
                    return mediator;
                }
            }
            return default(T);
        }

        public T remove(string name)
        {
            if (string.IsNullOrEmpty(name) == false)
            {
                T value;
                if (map.TryGetValue(name, out value))
                {
                    map.Remove(name);
                    value.onRemove();
                    return value;
                }
            }
            return default(T);
        }

        public bool has(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }
            return map.ContainsKey(name);
        }

        public void clear()
        {
            foreach(string key in map){
                remove(key);
            }
        }

        public ASDictionary<string, T> all()
        {
            return map;
        }

        public void clearCache()
        {
            foreach (T item in map.Values)
            {
                if (item.isReady)
                {
                    item.onClearCache();
                }
            }
        }
    }
}
