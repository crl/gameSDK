using foundation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace clayui
{
    /// <summary>
    /// 抽像pagelist 主要默认实现与显示无关的,list的选择操作,数据与排列的抽像功能
    /// </summary>
    public abstract class AbstractPageList:SkinBase,IDataProviderList
    {
        protected  static object[] EMPTY_OBJECTS = new object[0];

        public bool allowRepeat = true;
        protected object _selectedData;
        protected IListItemRender _selectedItem;
        protected IList _dataProvider;
        protected IFactory _itemFacotry;

        public Action<string, IListItemRender, object> itemEventHandle;
        /// <summary>
        /// 当前显示列表;
        /// </summary>
        protected List<IListItemRender> _childrenList;
        protected Stack<IListItemRender> _caches;
        protected Stack<IListItemRender> _oldChildrenList;
        protected int _sourceLength;

        public AbstractPageList()
        {
            _oldChildrenList = new Stack<IListItemRender>();
            _caches = new Stack<IListItemRender>();
            _childrenList = new List<IListItemRender>();
        }

        public IFactory itemFacotry
        {
            get
            {
                return _itemFacotry;
            }
        }
        protected void innerClear(Stack<IListItemRender> willCleanChildren)
        {
            IListItemRender item;
            int length = willCleanChildren.Count;
            SkinBase view;
            while (willCleanChildren.Count > 0)
            {
                item = willCleanChildren.Pop();
                view = item as SkinBase;
                if (view != null)
                {
                    view.SetActive(false);
                }

                bindItemEvent(item, false);
                _caches.Push(item);
            }
        }

        public virtual void scrollToBegin()
        {
        }

        public virtual void scrollToEnd()
        {
        }

        public virtual void scrollToData(object data)
        {
            int index = dataProvider.IndexOf(data);
            if (index != -1)
            {
                scrollToIndex(index);
            }
        }

        public virtual void scrollToIndex(int index)
        {
        }

        protected virtual void onScrollChangHandle(Vector2 normalizedPosition)
        {
        }

        public IList dataProvider
        {
            get { return _dataProvider; }
            set
            {
                if (value == null)
                {
                    value = EMPTY_OBJECTS;
                }
                _dataProvider = value;

                //清空旧的选择项;
                object oldSelectedData = _selectedData;
                _selectedData = null;
                if (_selectedItem != null)
                {
                    _selectedItem.isSelected = false;
                    _selectedItem = null;
                }
                //开始渲染新数据;
                renderList();
                calculatorBound();
                resetSelectedOldData(oldSelectedData);
            }
        }

        protected int currentStartIndex = 0;
        protected virtual void renderList()
        {
           IntVector2 v= getRenderListRange();
            int start = v.x;
            int end = v.y;

            int oldChildLen = _childrenList.Count;
            if (oldChildLen > 0)
            {
                for (int j = oldChildLen - 1; j > -1; j--)
                {
                    _oldChildrenList.Push(_childrenList[j]);
                }
                _childrenList.Clear();
            }

            IListItemRender item;
            currentStartIndex = start;
            for (int i = start; i < end; i++)
            {
                if (oldChildLen > 0)
                {
                    oldChildLen--;
                    item = _oldChildrenList.Pop();
                }
                else if (_caches.Count > 0)
                {
                    item = _caches.Pop();
                    SkinBase view = item as SkinBase;
                    if (view != null)
                    {
                        view.SetActive(true);
                    }
                }
                else
                {
                    item = (IListItemRender) _itemFacotry.newInstance();
                    addItemToContainer(item);
                }
                
                item.index = i;
                if (item is IPageListRef)
                {
                    ((IPageListRef)item).ownerPageList = this;
                }
                layout(item, i);
                bindItemEvent(item, true);
                bindItemData(item, getItemData(i));
                _childrenList.Add(item);
            }

            if (oldChildLen > 0)
            {
                innerClear(_oldChildrenList);
            }
        }

        protected virtual object getItemData(int index)
        {
            return _dataProvider[index];
        }

        protected abstract void addItemToContainer(IListItemRender item);
        protected virtual void bindItemData(IListItemRender item, object data)
        {
            item.data = data;
            if (_selectedData != null && _selectedData == data)
            {
                item.isSelected = true;
                _selectedItem = item;
            }
            else
            {
                item.isSelected = false;
            }
        }

        protected virtual IntVector2 getRenderListRange()
        {
            return new IntVector2(0, dataLength);
        }

        protected virtual void calculatorBound()
        {

        }

        public int dataLength
        {
            get
            {
                if (_dataProvider == null)
                {
                    return 0;
                }
                return _dataProvider.Count;
            }
        }
        public List<IListItemRender> childrenList
        {
            get
            {
                return _childrenList;
            }
        }

        

        protected virtual void bindItemEvent(IListItemRender item, bool isBind)
        {
            if (isBind)
            {
                item.itemEventHandle = itemEventHandle;
                item.addEventListener(EventX.CLICK, clickHandle);
            }
            else
            {
                item.itemEventHandle = null;
                item.removeEventListener(EventX.CLICK, clickHandle);
            }
        }

        protected virtual void clickHandle(EventX e)
        {
            IListItemRender item = e.currentTarget as IListItemRender;
            selectedItem = item;

            if (hasEventListener(EventX.ITEM_CLICK))
            {
                simpleDispatch(EventX.ITEM_CLICK, item);
            }
        }

        protected abstract void layout(IListItemRender item, int index = 0);

        protected virtual void resetSelectedOldData(object value)
        {
            if (value == null)
            {
                return;
            }

            int index = _dataProvider.IndexOf(value);
            if (index == -1)
            {
                return;
            }
            _selectedData = _dataProvider[index];

            int itemRenderIndex = index - currentStartIndex;
            if (itemRenderIndex < 0 || itemRenderIndex > _childrenList.Count - 1)
            {
                return;
            }

            _selectedItem = _childrenList[itemRenderIndex];
            if (_selectedItem != null)
            {
                _selectedItem.isSelected = true;
            }
        }

        public object selectedData
        {
            get { return _selectedData; }
            set
            {
                if (value == null)
                {
                    selectedIndex = -1;
                    return;
                }
                selectedIndex = _dataProvider.IndexOf(value);
            }
        }


        public IListItemRender selectedItem
        {
            set
            {
                if (_selectedItem == value && allowRepeat == false)
                {
                    return;
                }
                if (_selectedItem != null)
                {
                    _selectedItem.isSelected = false;
                }

                _selectedItem = value;
                if (_selectedItem != null)
                {
                    _selectedItem.isSelected = true;
                    _selectedData = _selectedItem.data;
                }
                else
                {
                    _selectedData = null;
                }

                if (hasEventListener(EventX.CHANGE))
                {
                    this.dispatchEvent(new EventX(EventX.CHANGE, _selectedItem));
                }
            }
            get { return _selectedItem; }
        }

        public int selectedIndex
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
                    selectedItem = null;
                    return;
                }


                int itemRenderIndex = value - currentStartIndex;
                if (itemRenderIndex < 0 || itemRenderIndex > _childrenList.Count - 1)
                {
                    if (_selectedItem != null)
                    {
                        _selectedItem.isSelected = false;
                        _selectedItem = null;
                    }

                    _selectedData = _dataProvider[value];
                }
                else
                {
                    selectedItem = _childrenList[itemRenderIndex];
                }
            }
        }

        public override void refresh()
        {
            int len = _childrenList.Count;
            for (int i = 0; i < len; i++)
            {
                _childrenList[i].refresh();
            }
        }
    }
}