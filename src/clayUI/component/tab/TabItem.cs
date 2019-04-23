using foundation;
using gameSDK;
using UnityEngine;
using UnityEngine.UI;

namespace clayui
{
    public class TabItem : SkinBase, ITabItem, IFocus
    {
        protected Image _selectImage;
        protected Text _selectText;
        protected Text _text;
        protected Image _image;

        public PanelDelegate panelDelegate;
        public string mediatorName;
        public float crossFadeTime = 0.3f;

        public foundation.Gradient textGradient;
        public Outline textOutline;

        private bool _isShow = false;

        public int index
        {
            get; set;
        }
        public GameObject target
        {
            get;
            set;
        }


        public string label
        {
            set
            {
                if (_text != null)
                {
                    _text.text = value;
                }
                if (_selectText != null)
                {
                    _selectText.text = value;
                }
            }
        }

        private ClayButton tabBtn;

        protected override void bindComponents()
        {
            _image = _skin.GetComponent<Image>();
            if (_image != null)
            {
                if (tabBtn == null)
                {
                    tabBtn = new ClayButton(_skin);
                }
                else
                {
                    tabBtn.skin = _skin;
                }
                tabBtn.tweenScale = false;
                tabBtn.addEventListener(EventX.CLICK, clickHandle);
                Transform t = _image.transform.Find("selectImage");
                if (t != null)
                {
                    _selectImage = t.GetComponent<Image>();
                    if (_selectImage != null)
                    {
                        _selectImage.enabled = true;
                    }
                }
                Transform textTra = _image.transform.Find("Text");
                if (textTra != null)
                {
                    _text = textTra.GetComponent<Text>();
                }
                if (_text != null)
                {
                    textGradient = _text.GetComponent<foundation.Gradient>();
                    textOutline = _text.GetComponent<Outline>();
                }
                Transform selectTextTra = _image.transform.Find("selectText");
                if (selectTextTra != null)
                {
                    _selectText = selectTextTra.GetComponent<Text>();
                }
            }
            hide();
        }

        public Image image
        {
            get { return _image; }
            set
            {
                if (value)
                {
                    skin = value.gameObject;
                }
                else
                {
                    skin = null;
                }
            }
        }

        public override GameObject skin
        {
            get { return _skin; }
            set
            {
                if (_skin == value)
                {
                    return;
                }

                if (_skin != null)
                {
                    unbindComponents();
                }
                _skin = value;
                if (_skin)
                {
                    prebindComponents();
                    bindComponents();
                    postbindComponents();
                }
            }
        }


        private void clickHandle(EventX e)
        {
            if (enabled == false)
            {
                return;
            }
            clickTween();
            simpleDispatch(EventX.CLICK);
        }
        
        public bool isShow
        {
            get
            {
                if (panelDelegate != null)
                {
                    return panelDelegate.isActive;
                }

                if(string.IsNullOrEmpty(mediatorName) == false) { 
                   IMediator mediator=Facade.GetInstance().getMediator(mediatorName);
                    if (mediator != null)
                    {
                        return mediator.getView().isShow;
                    }
                }
                return _isShow;
            }
        }

        public virtual void hide()
        {
            _isShow = false;
            if (panelDelegate != null)
            {
                panelDelegate.hide();
            }
            else if (string.IsNullOrEmpty(mediatorName) == false)
            {
                Facade.ToggleMediator(mediatorName,0);
            }
            if (_selectImage != null)
            {
                _selectImage.CrossFadeAlpha(0, crossFadeTime, true);
                _selectImage.gameObject.SetActive(false); 
            }
            else if (_image != null)
            {
                _image.color= ColorUtils.ToColor(0x888888FF);
            }

            if (_text != null && _selectText != null)
            {
                _selectText.enabled = false;
                _text.enabled = true;
            }
            else
            {
                if (textGradient != null)
                {
                    textGradient.topColor = new Color(192 / 255f, 155f / 255, 109 / 255f, 1);
                    textGradient.bottomColor = new Color(214 / 255f, 182f / 255, 157f / 255, 1);
                }
            }
        }

        public virtual void show(GameObject container=null)
        {
            _isShow = true;
            if (panelDelegate != null)
            {
                panelDelegate.show();
            }else if (string.IsNullOrEmpty(mediatorName)==false)
            {
                Facade.ToggleMediator(mediatorName,1);
            }
            if (_selectImage != null)
            {
                _selectImage.gameObject.SetActive(true);
                _selectImage.CrossFadeAlpha(1.0f, crossFadeTime, true);
            }
            else if (_image != null)
            {
                _image.color = Color.white;
            }

            if (_text != null && _selectText != null)
            {
                _selectText.enabled = true;
                _text.enabled = false;
            }
            else
            {
                if (textGradient != null)
                {
                    textGradient.topColor = new Color(1, 1, 1, 1);
                    textGradient.bottomColor = new Color(1, 216f / 255, 114f / 255, 1);
                }
            }
        }


        override protected void clickTween()
        {
        }

        override public void SetFocus(bool v)
        {
            if (_image == null) return;
            FocusGameObject mono = _image.GetComponent<FocusGameObject>();
            if (mono != null)
            {
                mono.setFocus(v);
            }
        }
    }
}