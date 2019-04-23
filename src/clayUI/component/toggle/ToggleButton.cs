using foundation;
using gameSDK;
using UnityEngine;
using UnityEngine.UI;

namespace clayui
{
    public class ToggleGroupData
    {
        public int index;
        public bool seleted;
        public bool enabled;
    }

    public class ToggleButton:SkinBase
    {
        private ClayButton selectBtn;
        private Image _checkImage;
        private Image _checkBgImage;

        private bool _selected = false;

        public bool selected
        {
            get { return _selected; }
            set
            {
                if (_selected != value)
                {
                    _selected = value;
                    updateView();
                    if (data != null) (data as ToggleGroupData).seleted = _selected;
                    this.simpleDispatch(EventX.CHANGE);
                }
            }
        }

        public ToggleButton(GameObject _skin, bool defaultSeleted = false)
        {
            this.skin = _skin;
            selected = defaultSeleted;
        }

        public ToggleButton(GameObject _skin, int index,bool defaultSeleted=false)
        {
            this.skin = _skin;
            ToggleGroupData toggleData = new ToggleGroupData();
            toggleData.index = index;
            toggleData.seleted = defaultSeleted;
            toggleData.enabled = true;
            data = toggleData;

            selected = defaultSeleted;
        }

        protected override void bindComponents()
        {
            selectBtn=new ClayButton(getGameObject("Background"));
            selectBtn.addEventListener(EventX.CLICK, clickHandle);

            _checkBgImage = getImage("Background");

            _checkImage = getImage("Background/Checkmark");
            _checkImage.gameObject.SetActive(true);
        }

        private void clickHandle(EventX e)
        {
            if (enabled)
            {
                _selected = !_selected;
                if (data != null) (data as ToggleGroupData).seleted = _selected;
                updateView();
            }
            this.simpleDispatch(EventX.ITEM_CLICK);
        }

        override protected void updateView(EventX e=null)
        {
            _checkImage.SetActive(selected);
        }

        protected override void doEnabled()
        {
            if (data != null) (data as ToggleGroupData).enabled = enabled;

            _checkBgImage.material = UIUtils.CreatShareGrayMaterial();
            _checkImage.material = UIUtils.CreatShareGrayMaterial();
            if (enabled)
            {
                _checkBgImage.color = Color.white;
                _checkImage.color = Color.white;
            }
            else
            {
                _checkBgImage.color = new Color(0, 1, 1, 1);
                _checkImage.color = new Color(0, 1, 1, 1);
            }
        }

        public void SelectedWithoutEvent(bool select)
        {
            if (_selected != select)
            {
                _selected = select;
                updateView();
                if (data != null) (data as ToggleGroupData).seleted = _selected;
            }
        }

    }
}