using foundation;

namespace gameSDK
{
    public abstract class SceneBase :AbstractState
    {
        protected bool _resizeable = false;
        protected IFacade facade;
        public SceneBase(string type) 
        {
            this._type = type;
        }

        public override void initialize()
        {
            facade = Facade.GetInstance();
            if (this is IInjectable)
            {
                facade.inject(this);
            }
            facade.autoInitialize(type);

            facade.addEventListener(EventX.CLEAR_CACHE, onClearCache);
            base.initialize();
        }

        protected virtual void onClearCache(EventX e)
        {

        }
    }
}
