using gameSDK;
using UnityEngine;
using UnityEngine.UI;

namespace foundation
{
    public interface IRaderManager
    {
        BaseRaderItem createUnit(BaseObject baseObject,bool isCenter=false);
        BaseRaderItem updateUnit(BaseObject baseObject);
        void disposeUnit(BaseObject baseObject);
    }

    public class BaseRaderItem:MonoBehaviour, IDataRenderer<BaseObject>
    {
        [SerializeField]
        protected BaseObject _data;
        protected bool _isDisposed = false;
        [SerializeField]
        protected Image image;
        public BaseObject data
        {
            get { return _data; }
            set
            {
                _data = value;
                doData();
            }
        }

        public virtual void dispose(float delayTime = 0.2f)
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            if (this.gameObject != null)
            {
                GameObject.Destroy(this.gameObject, delayTime);
            }
        }

        protected virtual void OnDestroy()
        {
            _isDisposed = true;
        }

        public virtual void doData()
        {
            if (image == null)
            {
                image = gameObject.AddComponent<Image>();
            }
            //todo other;
        }

        public virtual void updateView()
        {
            
        }
    }
}