using System.Collections;
using foundation;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace clayui
{
    public interface IVerticalVO
    {
        float height { get; }
    }
    public class VerticalPageList : MPageList
    {
        private RectTransform _layoutTransform;
        private Vector3 _startPosition;
        private RectTransform _scrollTransform;
        private ScrollRect _scrollRect;

        private Vector3 _defaultLayoutPosition;

        /// <summary>
        /// 滑动时重新计算间距
        /// </summary>
        public float resetOffsetY = 20;

        public VerticalPageList(IFactory factory, GameObject skin) : base(factory, skin)
        {
            _layoutTransform = skin.GetComponent<RectTransform>();

            _scrollRect = _layoutTransform.parent.GetComponent<ScrollRect>();
            _scrollRect.content = _layoutTransform;
            _scrollRect.horizontal = false;
            _scrollRect.vertical = true;

            _scrollTransform = _scrollRect.GetComponent<RectTransform>();
            _scrollRect.onValueChanged.AddListener(onScrollChangHandle);

            _defaultLayoutPosition = _layoutTransform.anchoredPosition;
            _startPosition = _layoutTransform.anchoredPosition;
        }

        public void onlySetDataProvider(IList value)
        {
            if (value == null)
            {
                value = EMPTY_OBJECTS;
            }
            _dataProvider = value;
            calculatorBound();
        }

        override public void scrollToBegin()
        {
            _layoutTransform.anchoredPosition = _defaultLayoutPosition;
            _startPosition = _defaultLayoutPosition;
            renderList();
        }
        override public void scrollToEnd()
        {
            Vector2 v = _layoutTransform.anchoredPosition;
            float totalLength = getTotalLength();
            float showLength = _scrollTransform.rect.height;
            v.y = Mathf.Max(totalLength - showLength,0);

            _layoutTransform.anchoredPosition = v;
            _startPosition = _layoutTransform.anchoredPosition;

            renderList();
        }

        override protected void onScrollChangHandle(Vector2 normalizedPosition)
        {
            if (Mathf.Abs(_layoutTransform.anchoredPosition.y - _startPosition.y) > resetOffsetY)
            {
                _startPosition = _layoutTransform.anchoredPosition;
                renderList();
            }
        }

        /// <summary>
        /// 设置开始render和结束render位置
        /// </summary>
        protected override IntVector2 getRenderListRange()
        {
            IntVector2 result=base.getRenderListRange();
            if (_scrollTransform == null)
            {
                return result;
            }

            int totalCount = _dataProvider.Count;
            float posY = _layoutTransform.anchoredPosition.y; //相对pagelist的y
            for (int i = 0; i < totalCount; i++)
            {
                IVerticalVO vo = (IVerticalVO)_dataProvider[i];
                float iconHeight = vo.height;
                posY -= iconHeight;
                if (posY - iconHeight > 0)
                {
                    result.x = i;
                }

                float scrollRectHeight = -_scrollTransform.rect.height;
                if (posY + iconHeight < scrollRectHeight)
                {
                    result.y = i;
                    break;
                }
            }

            return result;
        }

        protected override void layout(IListItemRender item, int i)
        {
            SkinBaseItemRender render = (SkinBaseItemRender) item;
            if (render != null)
            {
                GameObject skin = render.skin;
                skin.transform.SetSiblingIndex(i);
                Vector3 temp = skin.transform.localPosition;
                temp.z =temp.x= 0;
                temp.y = getPositionY(i, 0);
                skin.transform.localPosition = temp;
            }
        }

        protected override void calculatorBound()
        {
            if (_layoutTransform == null)
            {
                _layoutTransform = skin.GetComponent<RectTransform>();
            }

            Vector2 temp = _layoutTransform.sizeDelta;
            temp.y = getTotalLength();
            _layoutTransform.sizeDelta = temp;
        }

        /// <summary>
        /// 重置单项位置
        /// </summary>
        public virtual void resetChildPosition()
        {
            List<IListItemRender> allChild = childrenList;
            for (int i = 0; i < allChild.Count; i++)
            {
                SkinBaseItemRender render = allChild[i] as SkinBaseItemRender;
                if (render != null)
                {
                    layout(render, render.index);
                    render.refresh();
                }
            }
        }

        protected virtual float getTotalLength()
        {
            float result = 0;
            for (int i = 0, len = _dataProvider.Count; i < len; i++)
            {
                IVerticalVO vo = (IVerticalVO)_dataProvider[i];
                result += vo.height;
            }

            return result;
        }

        protected virtual float getPositionY(int index, float offset)
        {
            float result = offset;
            for (int i = 0, len = _dataProvider.Count; i < len; i++)
            {
                if (i >= index)
                {
                    break;
                }
                IVerticalVO vo = (IVerticalVO)_dataProvider[i];
                result -= vo.height;
            }
            return result;
        }
    }
}

