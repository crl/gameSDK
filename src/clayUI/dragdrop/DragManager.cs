using foundation;
using UnityEngine;

namespace clayui
{
    public class DragManager
    {
        public static AbstractDragger Current;
        public static object CurrentData;
        protected static GameObject currentGO;

        public const string DRAG_START = "drag_start";

        public const string DRAG_STOP = "drag_stop";

        public static void startDrag(AbstractDragger dragInitiator, object sourceData=null)
        {
            Current = dragInitiator;
            CurrentData = sourceData;

            currentGO = dragInitiator.getDragImage();

            if (currentGO != null)
            {

                CanvasGroup canvasGroup = currentGO.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup=currentGO.AddComponent<CanvasGroup>();
                }
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;

                currentGO.transform.SetParent(UILocater.CanvasLayer.transform);
                currentGO.transform.position = Input.mousePosition;
            }
            TickManager.Add(tick);
            Facade.SimpleDispatch(DragManager.DRAG_START, Current);
        }

        private static void tick(float deltaTime)
        {
            if (Input.GetMouseButton(0))
            {
                currentGO.transform.position = Input.mousePosition;
            }
            else
            {
                TickManager.Remove(tick);
                Facade.SimpleDispatch(DragManager.DRAG_STOP, Current);
            }
        }
    }
}