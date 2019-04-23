using gameSDK;
using UnityEngine;
using UnityEngine.UI;

namespace foundation.story
{
    public class FadeGUI
    {
        private static Color color = new Color(0, 0, 0, 0f);
        private static RawImage _cameraFadeTexture;
        private static bool _enabled = false;

        public static float Alpha
        {
            set
            {
                color.a = value;

                if (_cameraFadeTexture == null)
                {
                    _cameraFadeTexture = AbstractCameraController.GetCameraFadeTexure();
                }
                _cameraFadeTexture.color = color;
            }
            get { return color.a; }
        }

      
        public static bool Enabled
        {
            set
            {
                _enabled = value;
                if (_cameraFadeTexture == null)
                {
                    _cameraFadeTexture = AbstractCameraController.GetCameraFadeTexure();
                }
                Alpha = 0;

                if (_cameraFadeTexture != null)
                {
                    _cameraFadeTexture.SetActive(_enabled);
                }
            }
            get { return _enabled; }
        }

      
    }
}