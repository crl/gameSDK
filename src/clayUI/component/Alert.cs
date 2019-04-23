using foundation;
using gameSDK;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace clayui
{
    public class Alert: AbstractPanel
    {
        private static Stack<Alert> _pool=new Stack<Alert>();
        public static Type defaultAlertSkin = typeof(AlertSkin);
        
        public static string defaultAlertURI = "UIAlert";
        public static string OK_TEXT = "确定";
        public static string NO_TEXT = "取消";
        private AlertSkin _alertSkin;
        private Text _messageTF;
        private ClayButton _okBtn;
        private ClayButton _noBtn;
        private Button _closeBtn;
        private Toggle _neverTipToggle;

        private Action<AlertResult> resultAction;
        
        private Vector3 _oldPosition;

        private Dictionary<string, bool> _neverTipMap = new Dictionary<string, bool>();

        public static string defaultIncludeOut = "story";

        public Alert()
        {
            this._uri = defaultAlertURI;
            _isModel = true;

            _parent = UILocater.PopUpLayer;
            _alertSkin = Activator.CreateInstance(defaultAlertSkin) as AlertSkin;
            includeOut(defaultIncludeOut);
            Facade.AddEventListener(EventX.STATE_CHANGE, onStateChange);
        }

        private void onStateChange(EventX obj)
        {
            changeState(Facade.MapIntKey, Facade.CurrentViewState);
        }

        protected override void bindComponents()
        {
            _alertSkin.skin = skin;
            _okBtn = _alertSkin.okBtn;
            _noBtn = _alertSkin.noBtn;
            _neverTipToggle = _alertSkin.neverTipToggle;

            _closeBtn = _alertSkin.closeBtn;
            if (_closeBtn != null)
            {
                _closeBtn.gameObject.SetActive(true);
                _closeBtn.onClick.AddListener(clickHide);
            }
            _oldPosition = _okBtn.positionXY;

            _messageTF = _alertSkin.messageTF;

            _neverTipToggle.onValueChanged.AddListener(OnTipToggleChange);
            _okBtn.addEventListener(EventX.CLICK, clickHandle);
            _noBtn.addEventListener(EventX.CLICK, clickHandle);
        }

        private void OnTipToggleChange(bool b)
        {
            SetNeverTip(neverTipKey, b);
        }

        private void clickHandle(EventX e)
        {
            if (e.target == _okBtn)
            {
                fireAction(AlertResult.OK);
            }
            else
            {
                fireAction(AlertResult.NO);
            }
        }

        protected override void clickHide()
        {
            fireAction(AlertResult.DEF);
        }

        private void fireAction(AlertResult value)
        {
            hide();
            if (resultAction != null)
            {
                Action<AlertResult> oldAction = resultAction;
                resultAction = null;
                oldAction(value);
            }

            if (sharedInstance != this)
            {
                _pool.Push(this);
            }
        }

      

        private static Alert sharedInstance;
        private static Alert getSharedInstance()
        {
            if (sharedInstance == null)
            {
                sharedInstance = new Alert();
            }
            return sharedInstance;
        }

        public static Alert getInstance()
        {
            Alert instance;
            if (_pool.Count > 0)
            {
                instance = _pool.Pop();
            }
            else
            {
                instance=new Alert();
            }
            return instance;
        }

        public static Alert ShowOK(string message, Action<AlertResult> resultAction = null, int autoHideSecond=-1)
        {
            Alert alert = getSharedInstance();
            alert.show(message, 0, resultAction, autoHideSecond);
            return alert;
        }
        public static Alert ShowOKOrNO(string message, Action<AlertResult> resultAction = null, int autoHideSecond = -1, string neverTipKey = null)
        {
            Alert alert=getSharedInstance();
            alert.show(message, 1, resultAction, autoHideSecond, neverTipKey);
            return alert;
        }

        private string message = "";
        private int type = 0;
        private int autoHideSecond = -1;

        private string okText;
        private string noText;
        private string neverTipKey;


        public void showOK(string message, Action<AlertResult> resultAction = null, int autoHideSecond = -1)
        {
            show(message, 0, resultAction, autoHideSecond);
        }
        public void showOKOrNO(string message, Action<AlertResult> resultAction = null, int autoHideSecond = -1)
        {
            show(message, 1, resultAction, autoHideSecond);
        }
        public void show(string message, int v=0, Action<AlertResult> resultAction=null, int autoHideSecond = -1, string neverTipKey = null)
        {
            this.okText = OK_TEXT;
            this.noText = NO_TEXT;

            setButtonText(OK_TEXT, noText);

            this.message = message;
            this.type = v;
            this.resultAction = resultAction;
            this.neverTipKey = neverTipKey;
            this.autoHideSecond = autoHideSecond;

            if (autoHideSecond > 0)
            {
                CallLater.Add(autoHide, autoHideSecond);
            }
            else
            {
                CallLater.Remove(autoHide);
            }
            this.show(null, _isModel);
        }

        public void setButtonText(string okText="", string noText="")
        {
            if (string.IsNullOrEmpty(okText) == false)
            {
                this.okText = okText;
                if (_okBtn != null)
                {
                    _okBtn.text = okText;
                }
            }
            if (string.IsNullOrEmpty(noText) == false)
            {
                this.noText = noText;

                if (_noBtn != null)
                {
                    _noBtn.text = noText;
                }
            }
        }

        private bool GetNeverTip(string key)
        {
            if (!_neverTipMap.ContainsKey(key))
            {
                _neverTipMap.Add(key, false);
            }
            return _neverTipMap[key];
        }

        private void SetNeverTip(string key, bool b)
        {
            if (_neverTipMap.ContainsKey(key))
            {
                _neverTipMap[key] = b;
            }
            else
            {
                _neverTipMap.Add(key, b);
            }
        }

        private void autoHide()
        {
            clickHide();
        }

        override protected void updateView(EventX e=null)
        {
            if (_skin != null)
            {
                if (type == 0)
                {
                    _noBtn.skin.SetActive(false);

                    _okBtn.positionXY = new Vector3(0, _oldPosition.y);
                }
                else
                {
                    _noBtn.skin.SetActive(true);
                    _okBtn.positionXY = _oldPosition;
                }

                if (neverTipKey != null)
                {
                    if (GetNeverTip(neverTipKey))
                    {
                        fireAction(AlertResult.OK);
                        return;
                    }
                    _neverTipToggle.gameObject.SetActive(true);
                    _neverTipToggle.isOn = false;
                }
                else
                {
                    _neverTipToggle.gameObject.SetActive(false);
                }

                this._messageTF.text = message;
            }

        }
    }


    public class AlertSkin:SkinBase
    {
        public Text messageTF;
        public ClayButton okBtn;
        public ClayButton noBtn;
        public Button closeBtn;
        public Toggle neverTipToggle;

        protected override void bindComponents()
        {
            okBtn = new ClayButton(getGameObject("sureBtn"));
            noBtn = new ClayButton(getGameObject("cancelBtn"));
            //noBtn.clickSoundName = ClayButton.CancleSound;
            closeBtn = getButton("background/AlertBg/background/closeBtn");
            messageTF = getText("message");
            Text titleTF = getText("background/AlertBg/background/titleBg/Text");
            titleTF.text = "提  示";
            neverTipToggle = getComponent<Toggle>("neverTipToggle");
        }
    }
}