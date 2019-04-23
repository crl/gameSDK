using System;

namespace foundation
{
    public class BaseSocketDecoder : ISocketDecoder
    {
        protected static IFacade facade;

        public BaseSocketDecoder()
        {
            if (facade == null)
            {
                facade = Facade.GetInstance();
            }

            if (this is IInjectable)
            {
                facade.inject(this);
            }
        }

        protected virtual void onCache()
        {
        }
        protected virtual void onClearCache()
        {
            onCache();
        }
        public void clearCache()
        {
            onClearCache();
        }
    }
}