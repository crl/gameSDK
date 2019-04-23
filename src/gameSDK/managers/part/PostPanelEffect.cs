using foundation;
using UnityEngine;

namespace gameSDK
{
    public class PostPanelEffectManager
    {
        public const string EVENT_GLASS = "PostPanelGlassEffect";
        public static RenderTexture LastFrameRenderTexture
        {
            get
            {
                if (postEffect != null)
                {
                    return postEffect.texture;
                }
                return null;
            }
        }

        private static PostEffect postEffect;
        protected static Camera activeCamera;
        public static void Glass(bool isShow)
        {
            if (isShow)
            {
                Camera camera=BaseApp.MainCamera;
                if (camera != null)
                {
                    activeCamera = camera;
                    postEffect=activeCamera.gameObject.GetOrAddComponent<PostEffect>();
                    postEffect.enabled = true;
                }

            }else if (activeCamera != null)
            {
                postEffect=activeCamera.gameObject.GetOrAddComponent<PostEffect>();
                postEffect.enabled = false;
            }
            Facade.SimpleDispatch(EVENT_GLASS, isShow);
        }
    }

    [RequireComponent(typeof(Camera))]
    public class PostEffect : MonoBehaviour
    {
        new private Camera camera;
        public RenderTexture texture;
        protected void Awake()
        {
            camera = GetComponent<Camera>();
        }

        [ImageEffectOpaque]
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination);
            texture = source;
        }
    }
}