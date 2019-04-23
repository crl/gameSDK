using foundation;
using gameSDK;
using UnityEngine.UI;

namespace clayui
{
    public class NumericStepper : EventDispatcher
    {
        protected int _min = 0;
        protected int _max = 10;
        protected int _pad = 1;
        protected int _value;
        protected Text _text;

        public int value
        {
            get { return _value; }
            set
            {
                _value = value;
                invalidate();
            }
        }

        public void setMaxMin(int min = 0, int max = 10, int pad = 1)
        {
            _min = min;
            _max = max;
            _pad = pad;

            _value = _min;
            invalidate();
        }

        public void setAddButton(ClayButton btn)
        {
            btn.addEventListener(EventX.CLICK, onAdd);
            btn.autoClick = true;
        }

        private void onAdd(EventX obj)
        {
            var newValue = _value + _pad;
            if (newValue >= _max)
            {
                newValue = _max;
            }
            if (newValue != _value)
            {
                _value = newValue;
                invalidate();
            }
        }

        public void setMinusButton(ClayButton btn)
        {
            btn.addEventListener(EventX.CLICK, onMinus);
            btn.autoClick = true;
        }

        private void onMinus(EventX obj)
        {
            var newValue = _value - _pad;
            if (newValue < _min)
            {
                newValue = _min;
            }
            if (newValue != _value)
            {
                _value = newValue;
                invalidate();
            }
        }

        public void setText(Text txt)
        {
            _text = txt;
        }

        private void invalidate()
        {
            _text.text = _value.ToString();
            simpleDispatch(EventX.CHANGE);
        }


    }
}
