using foundation;
using UnityEngine;

namespace clayui
{
    public class TouchPageList : PageList
    {
        /// <summary>
        /// 1.移动速度
        /// </summary>
        public float speed = 500;

        /// <summary>
        /// 每次手指滑动只会移动一个项
        /// </summary>
        /// <param name="itemFacotry"></param>
        /// <param name="itemWidth"></param>
        /// <param name="itemHeight"></param>
        /// <param name="vertical"></param>
        /// <param name="maxDirect"></param>
        public TouchPageList(IFactory itemFacotry, int itemWidth = 0, int itemHeight = 0, bool vertical = true, int maxDirect = -1) : base(itemFacotry, itemWidth, itemHeight, vertical, maxDirect)
        {
            
        }

        protected override void bindComponents()
        {
            base.bindComponents();

            spacing = new Vector2(0f, 0f);

            DragMonoByInput evt = skin.GetComponent<DragMonoByInput>();
            if (evt == null)
            {
                evt = skin.AddComponent<DragMonoByInput>();
            }

            evt.beginDragAction = onBeginDragAction;
            evt.onDragAction = onDragAction;
            evt.endDragAction = onEndDragAction;
        }

        private void onDragAction(Vector3 obj)
        {
            simpleDispatch(EventX.TOUCH_MOVE);
        }

        private float startTime = 0f;
        private Vector3 starPos;
        private bool isDrag;
        private float targetPos = 0f;
        private float smooting = 10f;

        private void onBeginDragAction(Vector3 pos)
        {
            isDrag = true;
            startTime = Time.realtimeSinceStartup;
            starPos = pos;

            simpleDispatch(EventX.TOUCH_BEGAN);
        }

        private void onEndDragAction(Vector3 pos)
        {
            isDrag = false;

            Vector3 movePos = pos - starPos;
            float costTime = Time.realtimeSinceStartup - startTime;

            RectTransform tmp = skin.GetComponent<RectTransform>();
            float moveSpeed;
            if (_vertical)
            {
                //垂直
                float moveY = Mathf.Abs(movePos.y);
                moveSpeed = moveY / costTime;

                if (tmp.sizeDelta.y < moveY*2 || moveSpeed > speed)
                {
                    if (movePos.y > 0)
                    {
                        selectedIndex++;
                    }
                    else
                    {
                        selectedIndex--;
                    }

                }
                else
                {
                    smoothMove();
                }

            }
            else
            {
                //水平
                float moveX = Mathf.Abs(movePos.x);
                moveSpeed = moveX / costTime;
                if (tmp.sizeDelta.x < moveX*2 || moveSpeed > speed)
                {
                    if (movePos.x < 0)
                    {
                        selectedIndex++;
                    }
                    else
                    {
                        selectedIndex--;
                    }

                }
                else
                {
                    smoothMove();
                }
                
            }

            simpleDispatch(EventX.TOUCH_END);

            //            DebugX.Log("moveSpeed :" + moveSpeed);
        }

        void tick(float deltaTime)
        {
            if (!isDrag)
            {
                if (_vertical)
                {
                    float offset = Mathf.Abs(targetPos - _scrollRect.verticalNormalizedPosition);
                    float step = offset * deltaTime * smooting;
                    if (step > offset)
                    {
                        _scrollRect.verticalNormalizedPosition = targetPos;
                    }
                    else
                    {
                        _scrollRect.verticalNormalizedPosition = Mathf.Lerp(
                       _scrollRect.verticalNormalizedPosition, targetPos, deltaTime * smooting);
                    }
                 
                    if (Mathf.Abs(_scrollRect.verticalNormalizedPosition - targetPos) <= 0.00999999977648258)
                    {
                        TickManager.Remove(tick);
                    }
                }
                else
                {
                    float offset = Mathf.Abs(targetPos - _scrollRect.horizontalNormalizedPosition);
                    float step = offset * deltaTime * smooting;
                    if (step > offset)
                    {
                        _scrollRect.horizontalNormalizedPosition = targetPos;
                    }
                    else
                    {
                        _scrollRect.horizontalNormalizedPosition = Mathf.Lerp(
                       _scrollRect.horizontalNormalizedPosition, targetPos, deltaTime * smooting);
                    }

                    if (Mathf.Abs(_scrollRect.horizontalNormalizedPosition - targetPos) <= 0.00999999977648258)
                    {
                        TickManager.Remove(tick);
                    }
                  
                }
                
            }
                
        }

        new public int selectedIndex
        {
            get
            {
                if (_selectedData == null) return -1;

                return _dataProvider.IndexOf(_selectedData);
            }
            set
            {
                if (value < 0 || value > _dataProvider.Count - 1)
                {
//                    if (_selectedItem != null)
//                    {
//                        selectedItem = null;
//                    }
                    return;
                }

                int index = value - currentStartChildIndex;
                if (index < 0 || index > _childrenList.Count - 1)
                {
                    if (_selectedItem != null)
                    {
                        _selectedItem.isSelected = false;
                    }

                    _selectedItem = null;

                    _selectedData = _dataProvider[value];

                    if (hasEventListener(EventX.CHANGE))
                    {
                        this.dispatchEvent(new EventX(EventX.CHANGE, _selectedItem));
                    }
                }
                else
                {
                    selectedItem = _childrenList[index];
                }

                smoothMove();

            }
        }

        private void smoothMove()
        {
            if (_vertical)
            {
                targetPos = (dataProvider.Count - 1 - selectedIndex) * _itemBound.y / (_layoutTransform.sizeDelta.y - _itemBound.y);
                TickManager.Add(tick);
            }
            else
            {
                targetPos = selectedIndex * _itemBound.x / (_layoutTransform.sizeDelta.x - _itemBound.x);
                TickManager.Add(tick);
            }
            if (float.IsNaN(targetPos))
            {
                targetPos = 0;
            }
            targetPos = Mathf.Clamp01(targetPos);
        }

        
    }

    
}
