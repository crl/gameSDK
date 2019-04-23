using System.Collections.Generic;
using foundation;

namespace clayui
{
    public class ToggleGroup:EventDispatcher
    {
        public static string CHANGE_INDEX = "ChangeIndex";

        private List<ToggleButton> _list;
        private ToggleButton _selectedItem = null;
        private bool _isMultipleSelected = false;

        /// <summary>
        /// 需要知道整个group是多选还是单选
        /// </summary>
        /// <param name="isMultipleSelected"></param>
        public ToggleGroup(bool isMultipleSelected = false)
        {
            _list = new List<ToggleButton>();
            this._isMultipleSelected = isMultipleSelected;
        }

        public ToggleButton selectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem!=null)
                {
                    _selectedItem.selected = false;
                }
                _selectedItem = value;

                if (_selectedItem != null)
                {
                    _selectedItem.selected = true;
                }

                this.simpleDispatch(EventX.CHANGE);
            }
        }

        public int selectedIndex
        {
            get
            {
                if (_selectedItem == null)
                {
                    return -1;
                }
                return _list.IndexOf(_selectedItem);
            }
            set
            {
                if (value == -1)
                {
                    selectedItem = null;
                    return;
                }

                if (value >= _list.Count)
                {
                    value = _list.Count - 1;
                }

                selectedItem = _list[value];
            }
        }

        public bool add(ToggleButton item)
        {
            if (_list.IndexOf(item) != -1)
            {
                return false;
            }

            _list.Add(item);
            item.addEventListener(EventX.ITEM_CLICK, itemHandle);

            return true;
        }

        public void remove(ToggleButton item)
        {
            item.removeEventListener(EventX.ITEM_CLICK, itemHandle);
        }

        public List<ToggleGroupData> GetGroupDatas()
        {
            List<ToggleGroupData> list=new List<ToggleGroupData>();
            foreach (ToggleButton button in _list)
            {
                list.Add(button.data as ToggleGroupData);
            }
            return list;
        }

        public void setDefaultGroupSelectState(bool selected)
        {
            foreach (ToggleButton button in _list)
            {
                button.selected = selected;
            }
        }

        private void itemHandle(EventX e)
        {
            ToggleButton item = e.target as ToggleButton;
            if (!_isMultipleSelected)//单选
            {
                this.selectedItem = item;
                int index = (selectedItem.data as ToggleGroupData).index;
                for (int i = 0; i < _list.Count; i++)
                {
                    if (i != index)
                    {
                        _list[i].SelectedWithoutEvent(false);
                    }
                }
            }
            this.simpleDispatch(CHANGE_INDEX, item.data);
        }

    }
}