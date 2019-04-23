using System;
using foundation;
using UnityEngine;

namespace clayui
{
    /// <summary>
    /// 界面摆好的ItemRender;
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ViewList<T>: AbstractPageList where T:SkinBaseItemRender,new()
    {
        protected string _keyChild;

        public Action<T, GameObject> itemBindComponentsAction;
        public Action<T,object> itemBindDatAction;

        public ViewList(string keyChild="icon",GameObject skin=null)
        {
            this._keyChild = keyChild;
            if (skin != null)
            {
                this.skin = skin;
            }
        }
        protected override void bindComponents()
        {
            int len=_skin.transform.childCount;
            bool isEmptyName = string.IsNullOrEmpty(_keyChild);
            Transform item;
            for (int i = 0; i < len; i++)
            {
                if (isEmptyName)
                {
                    item = _skin.transform.GetChild(i);
                }
                else
                {
                    item = _skin.transform.Find(_keyChild + i);
                }

                if (item == null)
                {
                    continue;
                }

                SkinBaseItemRender itemRender = new T();
                itemRender.skin = item.gameObject;

                if (itemBindComponentsAction != null)
                {
                    itemBindComponentsAction(itemRender as T, item.gameObject);
                }
                _childrenList.Add(itemRender);
            }
        }

        override protected void renderList()
        {
            int len = Mathf.Min(_dataProvider.Count, _childrenList.Count);
            SkinBaseItemRender item;

            for (int i = 0; i < len; i++)
            {
                item=_childrenList[i] as SkinBaseItemRender;
                item.SetActive(true);

                item.index = i;
                if (item is IPageListRef)
                {
                    ((IPageListRef)item).ownerPageList = this;
                }
                layout(item, i);
                bindItemData(item, _dataProvider[i]);
                bindItemEvent(item, true);
            }

            int t = _childrenList.Count;
            for (int i = len; i < t; i++)
            {
                item = _childrenList[i] as SkinBaseItemRender;
                bindItemEvent(item, false);
                item.SetActive(false);
            }
        }

        protected override void bindItemData(IListItemRender item, object data)
        {
            base.bindItemData(item,data);
            if (itemBindDatAction != null)
            {
                itemBindDatAction((T)item, data);
            }
        }

        protected override void addItemToContainer(IListItemRender item)
        {
            ///已摆好,不需要重新摆
        }

        protected override void layout(IListItemRender item, int index = 0)
        {
            ///已摆好,不需要重新摆
        }
    }
}