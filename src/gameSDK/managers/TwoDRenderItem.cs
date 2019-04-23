using gameSDK;
using UnityEngine;
using UnityEngine.UI;

namespace foundation
{
    /// <summary>
    ///   ui上显示3D特效模型等效果
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    public class TwoDRenderItem : MonoBehaviour
    {
        protected RawImage image;
        protected BaseObject baseObject;

        public Vector3 position = Vector3.zero;
        public bool canRotation = false;
        public string uri = "";

        protected virtual void Start()
        {
            image = GetComponent<RawImage>();
            load(uri);
        }

        public void load(string value)
        {
            if (string.IsNullOrEmpty(value) == false)
            {
                this.uri = value;
                if (baseObject == null)
                {
                    baseObject = BaseApp.actorManager.createActor(ObjectType.PanelAvatar);
                    BaseApp.twoDRender.addToRender(image, baseObject, position, canRotation);
                }
                baseObject.load(uri);
            }
        }

        protected virtual void OnEnable()
        {
            if (baseObject != null && image!=null)
            {
                BaseApp.twoDRender.start(image);
            }
        }

        protected virtual void OnDisable()
        {
            if (baseObject != null && image != null)
            {
                BaseApp.twoDRender.stop(image);
            }
        }
    }
}