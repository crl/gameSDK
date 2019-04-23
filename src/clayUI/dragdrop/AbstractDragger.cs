using foundation;
using UnityEngine;

namespace clayui
{
    public class AbstractDragger:SkinBase
    {
        protected bool _dragEnabled = false;
        protected bool _dropEnabled = false;
        private bool isDragOver = false;
        private Vector3 pressVector3;
        private ASEventTrigger eventTrigger;
        public AbstractDragger()
        {
            Facade.AddEventListener(DragManager.DRAG_START,managerDragHandle);
            Facade.AddEventListener(DragManager.DRAG_STOP, managerDragHandle);
        }

        protected override void bindComponents()
        {
            eventTrigger=EventDispatcher.Get(_skin);

            eventTrigger.addEventListener(MouseEventX.MOUSE_DOWN, mouseHandle);
            eventTrigger.addEventListener(MouseEventX.MOUSE_UP, mouseHandle);
        }

        private void mouseHandle(EventX e)
        {
            if (e.type == MouseEventX.MOUSE_DOWN)
            {
                this.pressVector3 = Input.mousePosition;
                eventTrigger.addEventListener(MouseEventX.MOUSE_ENTER, enterHandle);
            }
            else
            {
                eventTrigger.removeEventListener(MouseEventX.MOUSE_ENTER, enterHandle);
            }
        }

        private void enterHandle(EventX e)
        {
            if (checkCanDrag())
            {
                DragManager.startDrag(this);
            }
        }

        protected bool checkCanDrag(){
			return _dragEnabled;
		}

        public GameObject getDragImage()
        {
            return null;
        }

        protected void managerDragHandle(EventX e) { 
            if (_dragEnabled == false && _dropEnabled == false)
            {
                return;
            }

            AbstractDragger dragger = e.target as AbstractDragger;
            if (e.type == DragManager.DRAG_START)
            {
                preDragStart(dragger);
            }
            else
            {
                if (isDragOver==true && dragger != this)
                {
                    onDrop(dragger);
                }
                onDragStop(dragger);
            }
        }

        protected virtual void preDragStart(AbstractDragger dragger)
        {
            if (onDragStart(dragger))
            {
                eventTrigger.addEventListener(MouseEventX.MOUSE_OVER, overHandler);
                eventTrigger.addEventListener(MouseEventX.MOUSE_OUT, overHandler);
            }
        }

        private void overHandler(EventX e)
        {
            if (e.type == MouseEventX.MOUSE_OVER)
            {
                isDragOver = true;
                onDragEnter(DragManager.Current);
            }
            else
            {
                isDragOver = false;
                onDragOut(DragManager.Current);
            }
        }

        protected virtual void onDragEnter(AbstractDragger dragger)
        {
            
        }

        protected virtual void onDragOut(AbstractDragger dragger)
        {

        }

        protected virtual bool onDragStart(AbstractDragger dragger)
        {
            return _dropEnabled && _enabled;
        }

        protected virtual void onDragStop(AbstractDragger dragger)
        {

        }

        protected void onDrop(AbstractDragger dragger)
        {

        }


        protected virtual bool canDrage 
        {
            get { return _dragEnabled; }
        }
    }
}