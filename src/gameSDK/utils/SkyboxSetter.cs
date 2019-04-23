using UnityEngine;

namespace gameSDK
{
    public class SkyboxSetter
    {
        public static Material defalutSkyMaterial;
        public static void SetSkybox(Texture2D front, Texture2D back, Texture2D left, Texture2D right, Texture2D up,
            Texture2D down)
        {
            SetSkybox(new Texture2D[6]
            {
                front, back,
                left, right,
                up, down
            });
        }

        public static void SetSkybox(Texture2D[] textures)
        {
            Shader shader = Shader.Find("Skybox/6 Sided");
            if (shader == null)
            {
                return;
            }
            Material material = new Material(shader);
            material.SetTexture("_FrontTex", textures[0]);
            material.SetTexture("_BackTex", textures[1]);
            material.SetTexture("_LeftTex", textures[2]);
            material.SetTexture("_RightTex", textures[3]);
            material.SetTexture("_UpTex", textures[4]);
            if (textures.Length > 5)
            {
                material.SetTexture("_DownTex", textures[5]);
            }
            Camera camera = BaseApp.MainCamera;
            Skybox skybox = camera.GetComponent<Skybox>();
            if (skybox == null)
            {
                skybox = camera.gameObject.AddComponent<Skybox>();
            }
            skybox.material = material;
        }

        public static Material getDefaultSkybox(bool autoGet=true)
        {
            if (defalutSkyMaterial == null)
            {
                if (autoGet)
                {
                    Camera camera = BaseApp.MainCamera;
                    Skybox skybox = camera.GetComponent<Skybox>();
                    if (skybox != null)
                    {
                        defalutSkyMaterial = skybox.material;
                    }
                }
            }
            return defalutSkyMaterial;
        }

        public static void defaultSkybox()
        {
            if (defalutSkyMaterial == null)
            {
                return;
            }
            Camera camera = BaseApp.MainCamera;
            Skybox skybox = camera.GetComponent<Skybox>();
            if (skybox != null && skybox.material != defalutSkyMaterial)
            {
                skybox.material = defalutSkyMaterial;
            }
        }
    }
}