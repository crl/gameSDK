using foundation;
using System.Collections.Generic;
using UnityEngine;

namespace clayui
{
    public class ImageMovieClip : SkinBase
    {
        protected GameObject _currentFrame;

        /// <summary>
        /// 简单的帧跳转组件
        /// </summary>
        /// <param name="skin"></param>
        public ImageMovieClip(GameObject skin)
        {
            this.skin = skin;
        }

        protected override void prebindComponents()
        {
            base.prebindComponents();

            var imgs = AS3_getChildren(_skin,"Image");

            foreach (var child in imgs)
            {
                child.SetActive(false);
            }


            if (imgs.Count > 0)
            {
                showFrame(1);
            }
        }

        public List<GameObject> AS3_getChildren(GameObject go,string namePrefix = "")
        {
            Transform[] allObj = go.GetComponentsInChildren<Transform>(true);

            List<GameObject> nodes = new List<GameObject>();
            foreach (var aa in allObj)
            {
                if (aa.parent == go.transform)
                {
                    if (string.IsNullOrEmpty(namePrefix) || aa.gameObject.name.IndexOf(namePrefix) == 0)
                    {
                        nodes.Add(aa.gameObject);
                    }
                }
            }
            return nodes;
        }

        public void showFrame(int frame)
        {
            GameObject nowFrame = getGameObject("Image" + frame);
            if (nowFrame != null)
            {
                nowFrame.SetActive(true);
            }
            if (_currentFrame != null && nowFrame != _currentFrame)
            {
                _currentFrame.SetActive(false);
            }
            _currentFrame = nowFrame;

        }

    }
}
