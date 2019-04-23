using foundation;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace clayui
{
    public class TabNav : TabNav<TabItem>
    {
        public TabNav(GameObject container = null)
        {
            this._container = container;
        }
    }

    public class TabNav<U>:EventDispatcher where U: TabItem
    {
        protected GameObject _container;
        private List<ITabItem> _tabsItems = new List<ITabItem>();

        public Func<ITabItem,bool> itemClickCheck;
        public bool toggle = false;

        public TabNav(GameObject container=null)
        {
            this._container = container;
        }

        public void addMediator(Image image, string mediatorName="")
        {
            if (image == null)
            {
                return;
            }
            U tabItem = Activator.CreateInstance<U>();
            tabItem.skin = image.gameObject;
            tabItem.mediatorName = mediatorName;
            add(tabItem);
        }

        public void addMediator<T>(Image image, bool checkImage = true)
        {
            if (checkImage && image == null)
            {
                return;
            }
            U tabItem = Activator.CreateInstance<U>();
            if (image != null)
            {
                tabItem.skin = image.gameObject;
            }

            tabItem.mediatorName = typeof(T).Name;
            add(tabItem);
        }

        public void addMediator(Image image, PanelDelegate panelDelegate,bool checkImage=true)
        {
            if (checkImage && image == null)
            {
                return;
            }
            U tabItem = Activator.CreateInstance<U>();
            if (image != null)
            {
                tabItem.skin = image.gameObject;
            }
            tabItem.panelDelegate = panelDelegate;
            if (panelDelegate != null)
            {
                panelDelegate.SetActive(false);
            }
            add(tabItem);
        }

        public void add(ITabItem tabItem)
        {
            if (_tabsItems.Contains(tabItem))
            {
                return;
            }

            tabItem.index = _tabsItems.Count;
            _tabsItems.Add(tabItem);

            tabItem.addEventListener(EventX.CLICK, itemClickHander);
        }

        public void remove(ITabItem item)
        {
            if (_tabsItems.Contains(item) == false)
            {
                return;
            }
            _tabsItems.Remove(item);

            int i = 0;
            foreach (ITabItem tabItem in _tabsItems)
            {
                tabItem.index = i++;
            }
        }

        public ITabItem getTabItem(int index)
        {
            return _tabsItems[index];
        }

        protected void itemClickHander(EventX e)
        {
            ITabItem tabItem = e.target as ITabItem;

            if (itemClickCheck!=null && itemClickCheck(tabItem)==false)
            {
                return;
            }

            selectedItem = tabItem;
            this.simpleDispatch(EventX.CLICK);
        }

        private ITabItem _selectedItem;
        public ITabItem selectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem != null)
                {
                    if (_selectedItem == value)
                    {
                        if (_selectedItem.isShow == false)
                        {
                            _selectedItem.show(_container);
                        }else if (toggle) { 
                            _selectedItem.hide();
                            _selectedItem = null;
                        }
                        this.simpleDispatch(EventX.CHANGE);
                        return;
                    }
                    _selectedItem.hide();
                }
                _selectedItem = value;

                if (_selectedItem != null)
                {
                    _selectedItem.show(_container);
                }

                this.simpleDispatch(EventX.CHANGE);
            }
        }

        public int selectedIndex
        {
            set
            {
                if (value < 0)
                {
                    selectedItem = null;
                    return;
                }
                
                if (value < _tabsItems.Count)
                {
                    if (itemClickCheck != null && itemClickCheck(_tabsItems[value]) == false)
                    {
                        return;
                    }
                    selectedItem =_tabsItems[value];
                }
            }
            get {
                if (_selectedItem != null)
                {
                    return _selectedItem.index;
                }
                return -1;
            }
        }
    }
}