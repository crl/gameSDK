using foundation;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace gameSDK
{
    public class TwoDRender:FoundationBehaviour
    {
        public static string UI_LIGHT_NAME = "UILight";
        public static string UI_3DCAMERA_NAME = "UI3DCamera";
        public static RenderTextureFormat RenderTextureFormat = RenderTextureFormat.Default;
        public static int TEXTURE_MAXSIZE = 1024;
       

        internal static Material sharedMaterial;
        private float cameraX = 500;
        private float cameraZ =-50;

        new private Camera camera;
        new private Light light;

        private ASDictionary<RawImage, RenderViewItem> renderDictionary;
        protected List<RenderViewItem> runing;
        public float offsetY = 20;
        public int defaultPixelScale = 100;
        public float cameraNear = -10f;
        public float cameraFar = 100f;
        public int cameraDepth = -1;
      

        public TwoDRender()
        {
            renderDictionary = new ASDictionary<RawImage, RenderViewItem>();
            runing = new List<RenderViewItem>();
        }

        protected void Start()
        {
            Transform tempTransform = BaseApp.UI3DContainer.transform.Find(UI_LIGHT_NAME);

            if (tempTransform == null && BaseApp.UICamera!=null)
            {
                tempTransform = BaseApp.UICamera.transform.Find(UI_LIGHT_NAME);
                if (tempTransform != null)
                {
                    tempTransform.SetParent(BaseApp.UI3DContainer.transform);
                }
            }
            if (tempTransform != null)
            {
                light = tempTransform.GetComponent<Light>();
            }

            tempTransform = BaseApp.UI3DContainer.transform.Find(UI_3DCAMERA_NAME);
            if (tempTransform == null && BaseApp.UICamera!=null)
            {
                tempTransform = BaseApp.UICamera.transform.Find(UI_3DCAMERA_NAME);
                if (tempTransform != null)
                {
                    tempTransform.SetParent(BaseApp.UI3DContainer.transform);
                }
            }
            if (tempTransform != null)
            {
                camera = tempTransform.GetComponent<Camera>();
            }
        }

      

        public Light createLight(Vector3 dir, float shadowStrength = 0.3f)
        {
            if (light == null)
            {
                GameObject lightGameObject = new GameObject(UI_LIGHT_NAME);
                light = lightGameObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.shadows = LightShadows.Hard;
                light.cullingMask = 1 << LayerX.GetUI3DLayer();
                lightGameObject.transform.SetParent(BaseApp.UI3DContainer.transform);
            }
            light.shadowStrength = shadowStrength;
            light.transform.eulerAngles = dir;
            return light;
        }

        public Light getLight()
        {
            return light;
        }

        public RenderViewItem addToRender(RawImage image, AbstractBaseObject baseObject,Vector3 position,
            bool canRotation = true)
        {
            RenderViewItem renderViewItem = renderDictionary[image];
            if (renderViewItem == null)
            {
                renderViewItem = new RenderViewItem(image);
                renderDictionary[image] = renderViewItem;
            }
            if (canRotation)
            {
                ASEventTrigger trigger = EventDispatcher.Get(image);
                trigger.data = renderViewItem;
                trigger.mouseEnterEnabled = true;
                trigger.addEventListener(MouseEventX.MOUSE_DOWN, mouseHandle);
                trigger.addEventListener(MouseEventX.MOUSE_ENTER, mouseHandle);
                trigger.addEventListener(MouseEventX.MOUSE_UP, mouseHandle);
            }

            baseObject.transform.SetParent(BaseApp.UI3DContainer.transform, false);
            renderViewItem.add(baseObject, position + new Vector3(cameraX, 0, 0));

            return renderViewItem;
        }

        public void removeToRender(RawImage image)
        {
            RenderViewItem renderViewItem = renderDictionary[image];
            if (renderViewItem != null)
            {
                stop(image);
                renderDictionary.Remove(image);
                renderViewItem.dispose();
            }
        }

        private Vector3 downVector2;

        private void mouseHandle(EventX e)
        {
            ASEventTrigger trigger = (ASEventTrigger) e.target;
            RenderViewItem renderViewItem = (RenderViewItem) trigger.data;

            if (renderViewItem.checkRotation() == false)
            {
                return;
            }

            switch (e.type)
            {
                case MouseEventX.MOUSE_DOWN:
                    renderViewItem.cache();
                    downVector2 = Input.mousePosition;
                    break;
                case MouseEventX.MOUSE_UP:
                    break;
                case MouseEventX.MOUSE_ENTER:
                    Vector2 deltaVector2 = downVector2 - Input.mousePosition;
                    float dis = (deltaVector2.x + deltaVector2.y) / 2.0f;
                    renderViewItem.rotationY = dis;
                    break;
            }
        }



        protected void tick(float deltaTime)
        {
            int len = runing.Count;
            if (len == 0)
            {
                TickManager.Remove(tick);
                return;
            }

            RenderViewItem renderViewItem;
            bool oldFog =RenderSettings.fog;
            RenderSettings.fog= false;

            for (int i = 0; i < len; i++)
            {
                renderViewItem = runing[i];
                int vlen = renderViewItem.viewList.Count;
                if (vlen == 0)
                {
                    continue;
                }

                RenderTexture renderTexture = renderViewItem.getRenderTexture();
                if (renderTexture == null)
                {
                    continue;
                }
                Vector3 postion = new Vector3(cameraX, 0, 0);
                postion = BaseApp.UI3DContainer.transform.TransformPoint(postion);
                camera.transform.LookAt(postion);
                camera.orthographicSize = renderTexture.height / (2f*defaultPixelScale);
                camera.targetTexture = renderTexture;
                renderViewItem.renderEnabled = true;
                camera.Render();
                camera.targetTexture = null;

                renderViewItem.renderEnabled = false;
            }

            RenderSettings.fog = oldFog;
        }

        public void start(RawImage image, bool forceActive = true)
        {
            if (camera == null)
            {
                GameObject go = new GameObject(UI_3DCAMERA_NAME);
                go.transform.SetParent(BaseApp.UI3DContainer.transform, false);
                go.SetActive(false);

                go.transform.localPosition = new Vector3(cameraX, offsetY, cameraZ);
                camera = go.AddComponent<Camera>();
                camera.orthographicSize = 2;
                camera.aspect = 1;
                
                camera.nearClipPlane = cameraNear;
                camera.farClipPlane = cameraFar;
                camera.orthographic = true;
                camera.depth = cameraDepth;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.cullingMask = 1 << LayerX.GetUI3DLayer();
                camera.backgroundColor = new Color(0, 0, 0, 0);
            }

            if (image == null)
            {
                return;
            }
            if (forceActive)
            {
                if (image.IsActive() == false)
                {
                    image.gameObject.SetActive(true);
                }
            }
            RenderViewItem renderViewItem = renderDictionary[image];
            if (renderViewItem == null)
            {
                return;
            }

            if (runing.IndexOf(renderViewItem) == -1)
            {
                runing.Add(renderViewItem);
                if (runing.Count == 1)
                {
                    TickManager.Add(tick);
                }
            }
        }

        public void drawMesh(Mesh mesh,Matrix4x4 matrix4X4,Material material,int layer)
        {
            Graphics.DrawMesh(mesh, matrix4X4, material, layer, camera);
        }

        public Camera getCamera()
        {
            return camera;
        }

        public void stop(RawImage image, bool disposeRenderTexture = true)
        {
            RenderViewItem renderViewItem = renderDictionary[image];
            if (renderViewItem == null)
            {
                return;
            }
            int index = runing.IndexOf(renderViewItem);
            if (index == -1)
            {
                return;
            }
            if (disposeRenderTexture)
            {
                disposeTexture(image);
            }
            runing.RemoveAt(index);
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        /// <param name="image"></param>
        public void disposeTexture(RawImage image)
        {
            RenderViewItem renderViewItem = renderDictionary[image];
            if (renderViewItem != null)
            {
                renderViewItem.disposeTexture();
            }
        }
    }


    public class RenderViewItem
    {
        public RawImage image;
        public List<AbstractBaseObject> viewList;
        public List<Vector3> viewPositionList;

        public Func<RenderViewItem, bool> outerCheckRotationFunc;
        public float offsetX = 0;
        public int textureSizeBitOffset = 0;
       
        private Color oldColor;
        private RenderTexture autoCreaTexture;
        public RenderViewItem(RawImage image)
        {
            this.image = image;

            oldColor = image.color;
            if (image.texture == null)
            {
                image.color = Color.clear;
            }
            viewList = new List<AbstractBaseObject>();
            viewPositionList = new List<Vector3>();
        }

        public float rotationY
        {
            set
            {
                int len = viewList.Count;
                for (int i = 0; i < len; i++)
                {
                    Transform transform = viewList[i].transform;
                    Vector3 v = transform.localEulerAngles;
                    v.y = _rotationY + value;
                    transform.localEulerAngles = v;
                }
            }
        }

        public bool renderEnabled
        {
            set
            {
                int len = viewList.Count;
                for (int i = 0; i < len; i++)
                {
                    AbstractBaseObject baseObject = viewList[i];
                    if (baseObject != null)
                    {
                        baseObject.renderable=value;
                    }
                }
            }
        }

        public void add(AbstractBaseObject item, Vector3 position)
        {
            if (item == null)
            {
                return;
            }
            if (viewList.Contains(item) == false)
            {
                item.renderable = false;
                item.transform.localPosition = position;
                viewList.Add(item);
                viewPositionList.Add(position);
            }
        }
        public void setOffset(AbstractBaseObject item, Vector3 offset)
        {
            int index = viewList.IndexOf(item);
            if (index != -1)
            {
                item.transform.localPosition = viewPositionList[index] + offset;
            }
        }
        public void remove(AbstractBaseObject item)
        {
            int index = viewList.IndexOf(item);
            if (index != -1)
            {
                viewList.RemoveAt(index);
            }
        }

        public void clear()
        {
            viewList.Clear();
        }

        private float _rotationY = 0;
        public bool canRotation = true;

        public void cache()
        {
            int len = viewList.Count;
            for (int i = 0; i < len; i++)
            {
                _rotationY = viewList[i].transform.localEulerAngles.y;
            }
        }

        public RenderTexture getRenderTexture()
        {
            if (image.IsActive() == false)
            {
                return null;
            }

            RenderTexture renderTexture = image.texture as RenderTexture;
            if (renderTexture == null)
            {
                image.color = oldColor;
                Vector2 v = image.GetComponent<RectTransform>().sizeDelta;
//                int w = UIUtils.GetNextPowerOfTwo(v.x);
//                int h = UIUtils.GetNextPowerOfTwo(v.y);
                int w = (int) v.x;
                int h = (int) v.y;

                if (w > TwoDRender.TEXTURE_MAXSIZE || h > TwoDRender.TEXTURE_MAXSIZE)
                {
                    float aspect = (float) w / h;
                    if (aspect > 1.0f)
                    {
                        w = TwoDRender.TEXTURE_MAXSIZE;
                        h = (int) (TwoDRender.TEXTURE_MAXSIZE / aspect);
                    }
                    else
                    {
                        h = TwoDRender.TEXTURE_MAXSIZE;
                        w = (int) (TwoDRender.TEXTURE_MAXSIZE * aspect);
                    }
                }
                if (autoCreaTexture != null)
                {
                    RenderTexture.ReleaseTemporary(autoCreaTexture);
                }

                renderTexture = autoCreaTexture = RenderTexture.GetTemporary(w, h, 24, TwoDRender.RenderTextureFormat,
                    RenderTextureReadWrite.Default);
                renderTexture.filterMode = FilterMode.Bilinear;
                renderTexture.wrapMode = TextureWrapMode.Clamp;
                renderTexture.anisoLevel = 0;
                renderTexture.antiAliasing = 4;
                image.texture = renderTexture;
            }
            return renderTexture;
        }
        public void addOpaqueMaterial(Material material=null)
        {
            if (material == null)
            {
                material = TwoDRender.sharedMaterial;
                if (material == null)
                {
                    Shader shader = Shader.Find("UI/Default No-Alpha");
                    material = TwoDRender.sharedMaterial = new Material(shader);
                }
            }

            if (material != null)
            {
                image.material = material;
            }
        }

        public void dispose()
        {
            viewList.Clear();

            if (autoCreaTexture != null)
            {
                RenderTexture.ReleaseTemporary(autoCreaTexture);
                autoCreaTexture = null;
            }
            //todo;
        }

        public void disposeTexture()
        {
            Texture texure = image.texture;
            if (autoCreaTexture != null)
            {
                image.texture = null;
                RenderTexture.ReleaseTemporary(autoCreaTexture);
                autoCreaTexture = null;
            }
            else if (texure != null)
            {
                image.color = Color.clear;
                UnityEngine.Object.Destroy(texure);
            }
        }

       
        public bool checkRotation()
        {
            if (canRotation == false)
            {
                return false;
            }

            if (outerCheckRotationFunc != null)
            {
                return outerCheckRotationFunc(this);
            }

            return canRotation;
        }
    }
}