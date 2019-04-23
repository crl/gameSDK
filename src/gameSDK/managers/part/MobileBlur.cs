using System.Collections.Generic;
using foundation;
using UnityEngine;
using UnityEngine.UI;

namespace gameSDK
{


    /// <summary>
    /// 毛玻璃效果
    /// </summary>
    [AddComponentMenu("Lingyu/MobileBlur")]
    [RequireComponent(typeof(RawImage))]
    public class MobileBlur : MonoBehaviour
    {
        public static int TEXTURE_MAXSIZE = 1024;
        public static Color IMAGE_BACKGROUND_COLOR = Color.clear;

        public enum BlurType
        {
            Standard,
            Sgx
        }

        [Range(0f, 5f)] public int downsample = 2;
        [Range(0f, 50f)] public float blurSize = 8.0f;
        [Range(1, 4)] public int blurIterations = 1;
        public BlurType blurType = BlurType.Standard;

        public Shader currentShader;
        protected Material _blurMaterial;


        private static Shader BlurShader;
        protected static RenderTexture TempRenderTexture;
        protected static RenderTexture TempRenderTexture1;

        protected RawImage image;

        private static List<MobileBlur> MobileBlursStack = new List<MobileBlur>();
        private static MobileBlur TopListMobileBlur;

        protected Material blurMaterial
        {
            get
            {
                if (this._blurMaterial == null)
                {
                    this._blurMaterial = new Material(this.currentShader);
                    this._blurMaterial.hideFlags = HideFlags.HideAndDontSave;
                }

                return this._blurMaterial;
            }
        }

        public static void MobileBlursStackCheck()
        {
            if (TopListMobileBlur != null)
            {
                TopListMobileBlur.toggle(false);
            }

            if (MobileBlursStack.Count > 0)
            {
                TopListMobileBlur = MobileBlursStack[MobileBlursStack.Count - 1];
                TopListMobileBlur.toggle(true);
            }
            else
            {
                if (TempRenderTexture)
                {
                    RenderTexture.ReleaseTemporary(TempRenderTexture);
                    TempRenderTexture = null;

                    RenderTexture.ReleaseTemporary(TempRenderTexture1);
                    TempRenderTexture1 = null;
                }

                TopListMobileBlur = null;
            }
        }

        public void preHide()
        {
        }

        public void toggle(bool b)
        {
            image = GetComponent<RawImage>();

            if (b == false)
            {
                this.image.material = null;
                PostPanelEffectManager.Glass(false);
                isShow = false;
                return;
            }

            if (IMAGE_BACKGROUND_COLOR != Color.clear)
            {
                image.color = IMAGE_BACKGROUND_COLOR;
            }

            if (BlurShader == null)
            {
                BlurShader = Shader.Find("Hidden/FastBlur");
            }

            currentShader = BlurShader;

            if (_blurMaterial != null)
            {
                _blurMaterial.shader = currentShader;
            }

            if (this.currentShader == null || !this.currentShader.isSupported || image == null)
            {
                base.enabled = false;
            }


            this.image.material = null;
            PostPanelEffectManager.Glass(true);
            isShow = true;
        }


        protected void OnDisable()
        {
            if (this._blurMaterial)
            {
                UnityEngine.Object.DestroyImmediate(this._blurMaterial);
                this._blurMaterial = null;
            }

            if (image != null)
            {
                this.image.material = null;
            }

            if (MobileBlursStack.Contains(this))
            {
                MobileBlursStack.Remove(this);
                MobileBlursStackCheck();
            }

            isShow = false;
        }

        protected void OnEnable()
        {
            if (MobileBlursStack.Contains(this) == false)
            {
                MobileBlursStack.Add(this);
            }

            isShow = true;
        }

        private bool isShow;

        public void OnRenderObject()
        {
            if (isShow == false)
            {
                return;
            }

            RenderTexture CameraRenderTexture = PostPanelEffectManager.LastFrameRenderTexture;
            if (CameraRenderTexture == null)
            {
                return;
            }

            Vector2 v = BaseApp.CanvasScaler.referenceResolution;
            int width = (int) Screen.width >> this.downsample;
            int height = (int) Screen.height >> this.downsample;

            if (width > TEXTURE_MAXSIZE || height > TEXTURE_MAXSIZE)
            {
                float aspect = (float) width / height;
                if (aspect > 1.0f)
                {
                    width = TEXTURE_MAXSIZE;
                    height = (int) (TEXTURE_MAXSIZE / aspect);
                }
                else
                {
                    height = TEXTURE_MAXSIZE;
                    width = (int) (TEXTURE_MAXSIZE * aspect);
                }
            }

            float widthMod = 1f / (1f * (float) (1 << this.downsample));
            this.blurMaterial.SetVector("_Parameter", new Vector4(blurSize * widthMod, -blurSize * widthMod, 0f, 0f));
            // downsample
            if (TempRenderTexture == null)
            {
                TempRenderTexture = RenderTexture.GetTemporary(width, height, 0);
                TempRenderTexture1 = RenderTexture.GetTemporary(width, height, 0);
            }

            Graphics.Blit(CameraRenderTexture, TempRenderTexture, _blurMaterial, 0);

            int passOffs = (blurType != BlurType.Standard) ? 2 : 0;
            for (int i = 0; i < this.blurIterations; i++)
            {
                float iterationOffs = (i * 1.0f);
                this.blurMaterial.SetVector("_Parameter",
                    new Vector4(this.blurSize * widthMod + iterationOffs, -this.blurSize * widthMod - iterationOffs, 0f,
                        0f));
                // vertical blur
                Graphics.Blit(TempRenderTexture, TempRenderTexture1, this._blurMaterial, 1 + passOffs);
                // horizontal blur
                Graphics.Blit(TempRenderTexture1, TempRenderTexture, this._blurMaterial, 2 + passOffs);
            }

            image.texture = TempRenderTexture;
        }
    }
}