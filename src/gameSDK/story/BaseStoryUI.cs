using foundation;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace gameSDK
{
    public class BaseStoryUIImplement : AbstractPanel,IStoryUI
    {
        private Text nameTF;
        private Text messageTF;
        private Image image;
        public RawImage renderImage;
        public BaseObject baseObject;

        private RenderViewItem renderViewItem;
        public BaseStoryUIImplement()
        {
            this._uri = "UIBossTalk";
        }

        protected override void bindComponents()
        {
            nameTF = getText("name");
            messageTF = getText("message");

            image = getImage("Image");

            renderImage = getRawImage("renderTexture");

            baseObject = BaseApp.actorManager.createActor(ObjectType.PanelAvatar);
            baseObject.name = "imageAvatar";
            baseObject.rotationY = 180;
            renderViewItem = BaseApp.twoDRender.addToRender(renderImage, baseObject,
                new Vector3(0f, -1.4f, 0));
            if (renderViewItem != null)
            {
                renderViewItem.canRotation = false;
            }

            bindUIEventListener();
        }

        protected virtual void bindUIEventListener()
        {
            if (image != null)
            {
                Get(image).addEventListener(MouseEventX.CLICK, clickHandle);
            }
        }

        private void autoHide()
        {
            clickHandle(null);
        }
        protected void clickHandle(EventX e)
        {
            hide();
        }
/*
       public void StartTalk(Cutscene playableDirector, OverlayText talkerPlayable)
       {
           this.overlayText = talkerPlayable;
            if (_skin == null)
            {
                if (BaseApp.UICanvas)
                {
                    Transform skinTransform = BaseApp.UICanvas.transform.Find("UITalk");
                    if (skinTransform != null)
                    {
                        skin = skinTransform.gameObject;
                    }
                }
                if (skin == null)
                {
#if UNITY_EDITOR
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Editor/UI/UITalk.prefab");
                    if (prefab != null)
                    {
                        skin = GameObject.Instantiate(prefab);
                    }
                    else
                    {
                        Debug.Log("Editor UITalk 不存在!");
                    }
#endif
                }
            }

            if (_skin != null)
            {
                _isReady = true;
            }

            this.show();
        }
  */

//        public void ClearTalk(Cutscene playableDirector, OverlayText talkerPlayable)
//        {
//            this.overlayText = null;
//            hide();
//        }


        public override void hide(EventX e = null)
        {
            base.hide(e);
            BaseApp.twoDRender.stop(renderImage);
        }


    // private OverlayText overlayText;
        protected override void updateView(EventX e)
        {
           // this.nameTF.text = overlayText.name;
            /*this.messageTF.text = overlayText.talk;
            if (string.IsNullOrEmpty(overlayText.modelKey) == false)
            {
                this.baseObject.load(overlayText.modelKey);
                renderViewItem.setOffset(this.baseObject.gameObject,
                    new Vector3(overlayText.offsetScale.x, overlayText.offsetScale.y));
                float z = overlayText.offsetScale.z;
                if (z > 0)
                {
                    this.baseObject.transform.localScale = Vector3.one* overlayText.offsetScale.z;
                }
                else
                {
                    this.baseObject.transform.localScale = Vector3.one;
                }
            }*/

            BaseApp.twoDRender.start(renderImage);

            CallLater.Add(autoHide, 5.0f);
        }
    }
}