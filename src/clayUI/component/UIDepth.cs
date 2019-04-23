using UnityEngine;
using UnityEngine.UI;

namespace clayui
{
    public class UIDepth : MonoBehaviour
    {
        public int order;
        private int oldOrder;

        public bool isUI = true;

        void OnEnable()
        {
            doChange();
        }

        void Update()
        {
            if (oldOrder != order)
            {
                doChange();
            }
        }

        void doChange()
        {
            oldOrder = order;
            if (isUI)
            {
                Canvas canvas = this.GetComponent<Canvas>();

                if (canvas == null)
                {
                    canvas = this.gameObject.AddComponent<Canvas>();
                    this.gameObject.AddComponent<GraphicRaycaster>();
                }

                canvas.overrideSorting = true;
                canvas.sortingOrder = order;
            }
            else
            {
                GraphicRaycaster grc = this.GetComponent<GraphicRaycaster>();
                if (grc!=null)
                {
                    Destroy(grc);
                    Canvas canvas = this.GetComponent<Canvas>();
                    Destroy(canvas);
                }

                Renderer[] renders = this.GetComponentsInChildren<Renderer>(true);

                foreach (Renderer renderer in renders)
                {
                    renderer.sortingOrder = order;
                }
            }
        }
    }
}