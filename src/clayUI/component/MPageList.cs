using foundation;
using UnityEngine;

namespace clayui
{
    public class MPageList : AbstractPageList
    {
        public bool useLayout = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="factory">单项工厂</param>
        /// <param name="skin">父皮肤</param>
        public MPageList(IFactory factory,GameObject skin)
        {
            this._itemFacotry = factory;
            this.skin = skin;
            if (skin != null)
            {
                skin.SetActive(true);
            }
        }

        protected override void addItemToContainer(IListItemRender item)
        {
            SkinBaseItemRender render = item as SkinBaseItemRender;
            if (render !=null)
            {
               GameObject go=render.skin;
                go.transform.SetParent(skin.transform, false);
                go.transform.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// 设置排版次序
        /// </summary>
        /// <param name="render"></param>
        /// <param name="i"></param>
        override protected void layout(IListItemRender render, int i)
        {
            if (useLayout == false)
            {
                return;
            }
            SkinBase skinBase=render as SkinBase;
            if (skinBase != null)
            {
                GameObject skin = skinBase.skin;
                skin.transform.SetSiblingIndex(i);
                Vector3 temp = skin.transform.localPosition;
                temp.z = 0;
                skin.transform.localPosition = temp;
            }
        }
    }
}
