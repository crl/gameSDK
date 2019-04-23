using System;
using System.Collections.Generic;
using foundation;
using UnityEngine;

namespace gameSDK
{
    public class BaseObject : AbstractBaseObject, IRayHitReceiver, ISkinable
    {
        public static string PREFIX = "avatar";
        public bool hasProgress = false;
        protected string defaultAnimationName = "idle01";
        /// <summary>
        /// 用于如果坐在马上
        /// </summary>
        /// <returns></returns>
        public virtual Vector3 getRealPosition()
        {
            return transform.position;
        }

        [HideInInspector] public float destroyDelay = 0.1f;
        [HideInInspector] public LayerMask layer = -1;
        public string prefix = PREFIX;

        internal ObjectType __objectType;

        internal BaseObjectManager __actorManager;
        protected LoaderXDataType loaderXDataType = LoaderXDataType.PREFAB;
        private AssetResource skinResource;
        protected Action<EventX> readyHandle;
        /// <summary>
        /// 当前对像 所有引用的技能文件 可在回收时 一并回收
        /// </summary>
        protected List<BaseSkill> refSkillList = new List<BaseSkill>();
        protected bool _isReady = false;
     
        protected SkillExData _skillExData;

        protected Animator _animator;
        protected AnimatorOverrideController _overrideController;

        protected UnitCFG _unitCFG;
        /// <summary>
        /// 所在面板id;
        /// </summary>
        [HideInInspector] public int ownerPanelID = -1;

        [HideInInspector] public string resourceRootDir;
        [HideInInspector] public int loadPriority = 0;
        [HideInInspector] public uint retryCount = 0;
        [HideInInspector] public GameObject parent;

        public BaseObject()
        {
            this.resourceRootDir = PathDefine.avatarPath;
        }

        public virtual Vector3 getNavPos(Vector3 value)
        {
            return value;
        }

        protected bool _isInCharge = false;

        /// <summary>
        /// 是否在冲撞状态(用于摄像机不跟随, todo是否能靠相机延迟跟随来解决)
        /// </summary>
        public bool isInChange
        {
            get { return _isInCharge; }
            set { _isInCharge = value; }
        }

        public Transform getSkeleton(string skeletonName)
        {
            if (string.IsNullOrEmpty(skeletonName))
            {
                return null;
            }
            return transform.RecursivelyFind(skeletonName);
        }

        /// <summary>
        /// 切换渲染层级
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="layer"></param>
        protected virtual void setContentLayer(Transform transform, LayerMask layer)
        {
            transform.gameObject.SetLayerRecursively(layer);
        }

        public bool addReayHandle(Action<EventX> handle)
        {
            if (_isReady)
            {
                handle(new EventX(EventX.READY));
                return true;
            }

            readyHandle += handle;
            return true;
        }
        public bool removeReayHandle(Action<EventX> handle)
        {
            if (_isReady)
            {
                return false;
            }

            readyHandle -= handle;
            return true;
        }

        public bool removeAllReayHandle()
        {
            if (_isReady)
            {
                return false;
            }
            readyHandle = null;
            return true;
        }

        public virtual SkillExData getSkillExData()
        {
            if (_skillExData == null)
            {
                _skillExData = new SkillExData();
            }
            _skillExData.position = position;
            _skillExData.eulerAngles = eulerAngles;
            _skillExData.scale = scale;

            return _skillExData;
        }
        public void setSkillExData(string key, object value)
        {
            if (_skillExData == null)
            {
                _skillExData = getSkillExData();
            } 
            _skillExData.Add(key, value);
        }

        public virtual BaseSkill playSkill(string skillPath, List<BaseObject> targetList = null, SkillExData exData = null)
        {
            BaseSkill baseSkill = getSkill(targetList, exData);
            baseSkill.load(skillPath);
            return baseSkill;
        }

        private ASDictionary<string, BaseSkill> singleBaseSkills;
        public virtual BaseSkill playSingleSkill(string skillPath, List<BaseObject> targetList = null, SkillExData exData = null)
        {
            if (singleBaseSkills == null)
            {
                singleBaseSkills = new ASDictionary<string, BaseSkill>();
            }
            BaseSkill baseSkill;
            if (singleBaseSkills.TryGetValue(skillPath,out baseSkill))
            {
                baseSkill.stop();
            }

            baseSkill = getSkill(targetList, exData);
            AssetsManager.bindEventHandle(baseSkill,onPlaySingleSkillHandle,true);
            singleBaseSkills.Add(skillPath, baseSkill);
            baseSkill.load(skillPath);
            return baseSkill;
        }

        protected virtual void onPlaySingleSkillHandle(EventX e)
        {
            BaseSkill baseSkill = (BaseSkill)e.target;
            AssetsManager.bindEventHandle(baseSkill,onPlaySingleSkillHandle, false);
            string uri = baseSkill.getURI();
            singleBaseSkills.Remove(uri);
        }

        public virtual string guid
        {
            get { return GetInstanceID().ToString(); }
        }

        public BaseSkill getSkill(List<BaseObject>targetList = null, SkillExData exData = null){
            if (exData == null)
            {
                exData = getSkillExData();
            }
            BaseSkill skill =BaseApp.skillManager.createSkillBy(this,targetList,exData);
			skill.parent=parent;
			skill.loadPriority=loadPriority;
			return skill;
		}

        internal void addSkill(BaseSkill baseSkill)
        {
            if (baseSkill != null)
            {
                refSkillList.Add(baseSkill);
            }
        }

        public virtual Vector3 position
        {
            set { transform.position = value; }
            get { return transform.position; }
        }

        public virtual void movePropertyUpdatePosition(Vector3 value)
        {
            position = value;
        }

        public virtual Vector3 localPosition
        {
            set { transform.localPosition = value; }
            get { return transform.localPosition; }
        }

        public virtual bool isReady
        {
            get { return _isReady; }
        }

  
        public virtual Quaternion rotation
        {
            get { return transform.rotation; }
            set { transform.rotation = value; }
        }

        public virtual float rotationY
        {
            get { return transform.eulerAngles.y; }
            set
            {
                Vector3 v = transform.eulerAngles;
                v.y = value;
                transform.eulerAngles = v;
            }
        }

        public virtual float rotationX
        {
            get { return transform.eulerAngles.x; }
            set
            {
                Vector3 v = transform.eulerAngles;
                v.x = value;
                transform.eulerAngles = v;
            }
        }

        public virtual Vector3 eulerAngles
        {
            get { return transform.eulerAngles; }
            set { transform.eulerAngles = value; }
        }

        public virtual Vector3 scale
        {
            get { return transform.localScale; }
            set
            {
                if (value == Vector3.zero)
                {
                    value=Vector3.one;
                }
                transform.localScale = value;
            }
        }

        public virtual void setScale(float v)
        {
            if (v == 0.0f)
            {
                v = 1.0f;
            }
            transform.localScale = Vector3.one*v;
        }

        protected float _skinScale = 1.0f;
        public virtual void setSkinScale(float v)
        {
            if (v == 0.0f)
            {
                v = 1.0f;
            }

            _skinScale =v;

            if (_skin != null)
            {
                _skin.transform.localScale=Vector3.one*_skinScale;
            }
        }

        public virtual void setColor(Color color)
        {
            if (_skin!=null)
            {
                Renderer[] rs = _skin.GetComponentsInChildren<Renderer>();
                foreach (var r in rs)
                {
                    r.material.SetColor("_Color", color);
                }
            }
        }

        public T GetComponent<T>(string path, GameObject go = null) where T : Component
        {
            if (go == null)
            {
                go = _skin;
            }
            return UIUtils.GetComponent<T>(go, path);
        }

        public virtual GameObject GetGameObject(string name, GameObject go = null)
        {
            if (go == null)
            {
                go = _skin;
            }
            if (go == null)
            {
                return null;
            }
            Transform transform = go.transform.Find(name);
            if (transform != null)
            {
                return transform.gameObject;
            }

            return null;
        }

        /// <summary>
        /// 替换shader
        /// </summary>
        /// <param name="srcShaderName"></param>
        /// <param name="replaceShaderName"></param>
        /// <returns></returns>
        public virtual bool setRendererShader(string srcShaderName,string descShaderName="")
        {
            Shader shader = RenderUtils.FindShader(srcShaderName);
            if (shader == null)
            {
                return false;
            }

            bool hasReplace = string.IsNullOrEmpty(descShaderName)==false;
            Renderer[] rs = _skin.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in rs)
            {
                if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer)
                {
                    foreach (Material material in renderer.materials)
                    {
                        if (hasReplace)
                        {
                            if (material.shader == null || material.shader.name != descShaderName)
                            {
                                continue;
                            }
                        }
                        doSetRendererShader(material, shader);
                    }
                }
            }
            return true;
        }

        protected virtual void doSetRendererShader(Material material, Shader shader)
        {
            material.shader = shader;
        }

        public virtual bool setShaderKeyword(string keyword, bool state)
        {
            if (_skin == null)
            {
                return false;
            }
            Renderer[] rs = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in rs)
            {
                foreach (Material material in renderer.materials)
                {
                    RenderUtils.SetKeyword(material, keyword, state);
                }
            }
            return true;
        }

        public virtual bool setShaderFloatValue(string name, float value)
        {
            if (_skin == null)
            {
                return false;
            }
            Renderer[] rs = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in rs)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.HasProperty(name))
                    {
                        material.SetFloat(name, value);
                    }
                }
            }
            return true;
        }

        public virtual bool resetShaderFloatValue(string name)
        {
            if (_skin == null)
            {
                return false;
            }
            int hash = Shader.PropertyToID(name);
            Renderer[] rs = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in rs)
            {
                int currentLen = renderer.materials.Length;
                int rawLen = renderer.sharedMaterials.Length;
                if (currentLen != rawLen)
                {
                    continue;
                }

                for (int i = 0; i < currentLen; i++)
                {
                    Material material = renderer.materials[i];
                    if (material.HasProperty(hash))
                    {
                        float v = renderer.sharedMaterials[i].GetFloat(hash);
                        material.SetFloat(hash, v);
                    }
                }
            }
            return true;
        }


        private bool _isHightLight=false;
        public void setHightLight(bool v)
        {
            if (_isHightLight == v)
            {
                return;
            }
            _isHightLight = v;
            doSetHightLight(v);
        }

        protected virtual void doSetHightLight(bool v)
        {
            
        }
        private string _skinURI;
        public virtual void load(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                return;
            }

            if (_skinURI == uri)
            {
                return;
            }
            _skinURI = uri;

            if (skinResource != null)
            {
                AssetsManager.bindEventHandle(skinResource, onSkinResourceHandle, false);
                skinResource.removeEventListener(EventX.PROGRESS, onProgressHandle);
                skinResource.release();
                skinResource = null;
            }
            _isReady = false;
            string url = getURL(uri);

            if (AssetsManager.routerResourceDelegate != null)
            {
                skinResource = AssetsManager.routerResourceDelegate(url, uri, prefix);
            }
            if (skinResource == null)
            {
                skinResource = AssetsManager.getResource(url, loaderXDataType);
            }
            skinResource.retain();
            AssetsManager.bindEventHandle(skinResource, onSkinResourceHandle);
            if (hasProgress)
            {
                skinResource.addEventListener(EventX.PROGRESS, onProgressHandle);
            }

            skinResource.load();
        }

        public string skinURI
        {
            get { return _skinURI; }
        }
        public string skinURL
        {
            get { return getURL(_skinURI); }
        }

        protected virtual string getURL(string uri)
        {
            if (loaderXDataType == LoaderXDataType.PREFAB)
            {
                return resourceRootDir + prefix + "/" + uri + PathDefine.U3D;
            }
            else
            {
                return "Prefabs/" + uri;
            }
        }

        protected override void recycleSkin()
        {
            if (_skin != null)
            {
                PoolItem poolItem = _skin.GetComponent<PoolItem>();
                if (poolItem != null)
                {
                    poolItem.recycle(0);
                }
            }
        }
        protected PoolItem poolItem;
        private void onSkinResourceHandle(EventX e)
        {
            if (gameObject == null)
            {
                DebugX.Log("onSkinHandle is destory");
                return;
            }
            AssetResource resource = e.target as AssetResource;
            AssetsManager.bindEventHandle(resource, onSkinResourceHandle, false);
            if (e.type == EventX.FAILED)
            {
                onSkinResourceFailHandle(e);
                return;
            }

            poolItem = resource.getPoolItemFromPool<PoolItem>();
            GameObject go = poolItem.gameObject;

            skin = go;
            fireReadyEvent();
        }

        protected virtual void onSkinResourceFailHandle(EventX e)
        {
            
        }

        protected virtual void onProgressHandle(EventX e)
        {
            this.dispatchEvent(e);
        }

        public void fireReadyEvent() { 
            _isReady = true;

            if (readyHandle != null)
            {
                readyHandle(new EventX(EventX.READY));
                readyHandle = null;
            }
            this.simpleDispatch(EventX.READY, _skin);
        }

        protected override void prebindComponents()
        {
            base.prebindComponents();
            _skin.transform.localPosition = Vector3.zero;
            _skin.transform.localScale = Vector3.one * _skinScale;
            _skin.transform.localRotation = Quaternion.identity;
        }

        protected override void bindComponents()
        {
            base.bindComponents();
            _unitCFG = _skin.GetComponent<UnitCFG>();
            if (_unitCFG)
            {
                bindUnitConfig(_unitCFG);
            }

            Animator skinAnimator = _skin.GetComponent<Animator>();
            if (skinAnimator)
            {
                _animator = gameObject.GetOrAddComponent<Animator>();
                AnimatorClipRef animatorClipRef = _skin.GetComponentInChildren<AnimatorClipRef>();
                bindAnimator(skinAnimator, animatorClipRef);
            }
        }

        protected virtual void bindAnimator(Animator skinAnimator, AnimatorClipRef animatorClipRef)
        {
            if (skinAnimator != _animator)
            {
                _animator.logWarnings = isDebug;
                List<AnimatorControllerParameterValueStore> stores =
                    AnimatorControllerParameterValueStore.GetValue(_animator);
                _animator.avatar = skinAnimator.avatar;
                _animator.applyRootMotion = skinAnimator.applyRootMotion;
                _animator.cullingMode = skinAnimator.cullingMode;
                _animator.updateMode = skinAnimator.updateMode;

                skinAnimator.enabled = false;

                _animator.Rebind();
                AnimatorControllerParameterValueStore.SetValue(stores, _animator);
            }

            RuntimeAnimatorController templteAnimatorController = null;
            if (animatorClipRef != null)
            {
                templteAnimatorController = animatorClipRef.controller;
#if UNITY_EDITOR
                RuntimeAnimatorController e = getEditorAnimatorController(animatorClipRef, skinAnimator);
                if (e)
                {
                    templteAnimatorController = e;
                }
#endif
            }

            if (templteAnimatorController == null)
            {
                templteAnimatorController = skinAnimator.runtimeAnimatorController;
            }

            if (templteAnimatorController != null)
            {
                if (_overrideController == null)
                {
                    _overrideController = new AnimatorOverrideController();
                }
                _overrideController.runtimeAnimatorController = templteAnimatorController;

                if (animatorClipRef != null)
                {
                    Dictionary<string, AnimationClip> overrideMapping = getOverrideAnimationMapping(animatorClipRef);
                    bindAnimatorClipRef(_overrideController, overrideMapping);
                }
                _animator.runtimeAnimatorController = _overrideController;

                AnimatorStateMachineImplement animatorStateMachineImplement = getAnimatorStateMachineImplement();
                if (animatorStateMachineImplement != null)
                {
                    animatorStateMachineImplement.implement(_animator, this.gameObject);
                }
            }
        }

        public Animator getAnimator()
        {
            return _animator;
        }

        protected virtual AnimatorStateMachineImplement getAnimatorStateMachineImplement()
        {
            return null;
        }

        protected virtual RuntimeAnimatorController getEditorAnimatorController(AnimatorClipRef animatorClipRef, Animator skinAnimator)
        {
            return null;
        }

        /// <summary>
        /// 取得动画可替换列表
        /// </summary>
        /// <param name="animatorClipRef"></param>
        /// <returns></returns>
        protected virtual Dictionary<string, AnimationClip> getOverrideAnimationMapping(AnimatorClipRef animatorClipRef)
        {
            Dictionary<string, AnimationClip> result = new Dictionary<string, AnimationClip>();
            string clipName;
            string namePrefix = "Base Layer."; 
            string fullName;
            foreach (AnimationClip animationClip in animatorClipRef.placeholderClips)
            {
                if (animationClip == null)
                {
                    continue;
                }
                clipName = animationClip.name;
                result[clipName] = animationClip;

                fullName = namePrefix+ clipName;
                AnimatorStateMachineImplement.AddStringToHash(fullName);
            }
            foreach (AnimationClip animationClip in animatorClipRef.animationClips)
            {
                if (animationClip == null)
                {
                    Debug.LogWarning(_skin.name + ": animatorClipRef has Null ");
                    continue;
                }
                clipName = animationClip.name;
                result[clipName] = animationClip;

                fullName = namePrefix + clipName;
                AnimatorStateMachineImplement.AddStringToHash(fullName);
            }
            return result;
        }

        /// <summary>
        /// 绑定替换列表
        /// </summary>
        /// <param name="overrideController"></param>
        /// <param name="overrideMapping"></param>
        protected virtual void bindAnimatorClipRef(AnimatorOverrideController overrideController,
            Dictionary<string, AnimationClip> overrideMapping)
        {
            List<KeyValuePair<AnimationClip, AnimationClip>> oldList =
                new List<KeyValuePair<AnimationClip, AnimationClip>>();
            List<KeyValuePair<AnimationClip, AnimationClip>> newList =
                new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(oldList);

            foreach (KeyValuePair<AnimationClip, AnimationClip> item in oldList)
            {
                if (item.Key == null)
                {
                    continue;
                }

                string clipName = item.Key.name;
                AnimationClip repleaceClip=null;
                if (overrideMapping.TryGetValue(clipName, out repleaceClip))
                {
                    newList.Add(new KeyValuePair<AnimationClip, AnimationClip>(item.Key, repleaceClip));
                }
                else
                {
                    repleaceClip = nonExistentBindAnimator(item, overrideMapping);
                    if (repleaceClip != null)
                    {
                        newList.Add(new KeyValuePair<AnimationClip, AnimationClip>(item.Key, repleaceClip));
                    }
                }
            }
            overrideController.ApplyOverrides(newList);
        }

        /// <summary>
        /// 从overrideMapping找不到替换时，如何处理
        /// </summary>
        /// <param name="oldValuePair">老的替换方式</param>
        /// <param name="overrideMapping">可替换列表</param>
        /// <returns></returns>
        protected virtual AnimationClip nonExistentBindAnimator(KeyValuePair<AnimationClip, AnimationClip> oldValuePair,
            Dictionary<string, AnimationClip> overrideMapping)
        {
            AnimationClip repleaceClip;
            overrideMapping.TryGetValue(defaultAnimationName, out repleaceClip);
            return repleaceClip;
        }

        public virtual bool replaceAnimationClip(string name, string newClipName)
        {
            if (_overrideController != null && _skin!=null)
            {
                AnimatorClipRef _animatorClipRef = _skin.GetComponent<AnimatorClipRef>();
                AnimationClip clip;
                if (_animatorClipRef != null && _animatorClipRef.TryGetValue(newClipName, out clip))
                {
                    _overrideController[name] = clip;
                    return true;
                }
            }
            return false;
        }

        protected Dictionary<Renderer, MeshStore> rendererShadowStores;
        public virtual bool addRendererShadow(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            if (rendererShadowStores == null)
            {
                rendererShadowStores = new Dictionary<Renderer, MeshStore>();
            }

            bool has = false;
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            foreach (Renderer render in renderers)
            {
                if (render is MeshRenderer || render is SkinnedMeshRenderer)
                {
                    has = true;
                    if (rendererShadowStores.ContainsKey(render)) continue;
                    MeshStore meshStore = getNewMeshStore();
                    meshStore.render = render;
                    meshStore.owner = render.gameObject.transform;
                    if (render is SkinnedMeshRenderer)
                    {
                        meshStore.skinnedMeshRenderer = render as SkinnedMeshRenderer;
                    }
                    else
                    {
                        MeshFilter meshFilter = render.GetComponent<MeshFilter>();
                        if (meshFilter != null)
                        {
                            meshStore.mesh = meshFilter.sharedMesh;
                        }
                    }

                    rendererShadowStores.Add(render, meshStore);
                }
            }
            return has;
        }

        protected virtual MeshStore getNewMeshStore()
        {
            return new MeshStore();
        }

        public virtual bool removeRendererShadow(GameObject go)
        {
            if (go == null || rendererShadowStores==null)
            {
                return false;
            }
            MeshStore meshStore;
            bool has = false;
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                if (rendererShadowStores.TryGetValue(renderer, out meshStore))
                {
                    has = true;
                    meshStore.dispose();
                    rendererShadowStores.Remove(renderer);
                }
            }
            return has;
        }


        public virtual bool OnRayHit(RaycastHit hit)
        {
            return true;
        }

        public virtual bool TryRayHitMore(RaycastHit hit)
        {
            return false;
        }

        public virtual void OnRayHitSelf()
        {
            
        }

        public virtual void dispose(float delayTime=0f)
        {
            if (isDisposing)
            {
                return;
            }
            if (__actorManager != null)
            {
                __actorManager.removeByInstanceID(this.GetInstanceID());
            }
            if (this.gameObject != null)
            {
                ///2018.0807应该算是unity的bug,_skin的animator的enabled会被Destroy给禁了
                if (_skin != null)
                {
                    ///走skin流程;
                    skin = null;
                }
                GameObject.Destroy(this.gameObject, delayTime);
            }
        }
     

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }

        protected virtual void Update()
        {
        }

        public virtual void playAnim(int stateID, int layer=0, float normalizedTime=float.NegativeInfinity)
        {
            if (_animator != null && _animator.HasState(layer, stateID))
            {
                _animator.Play(stateID, layer, normalizedTime);
            }
        }

        /// <summary>
        /// 不想用stateID,因为stateID,在基础层必须加BaseLayer.xx,所以用直接的名称会更方便;
        /// </summary>
        /// <param name="stateName"></param>
        /// <param name="layer"></param>
        /// <param name="normalizedTime"></param>
        public virtual void playAnim(string stateName, int layer = 0, float normalizedTime = float.NegativeInfinity)
        {
            if (_animator != null)
            {
                _animator.Play(stateName, layer, normalizedTime);
            }
        }

        override protected void onDestroy()
        {
            if (_skin != null)
            {
                ///走skin流程;
                skin = null;
            }
            int len = refSkillList.Count;
            if (len > 0)
            {
                BaseSkill skill;
                while (refSkillList.Count > 0)
                {
                    skill = refSkillList[0];
                    refSkillList.RemoveAt(0);
                    skill.Dispose();
                }
                if (singleBaseSkills != null)
                {
                    singleBaseSkills.Clear();
                }
            }

            if (rendererShadowStores != null)
            {
                foreach (KeyValuePair<Renderer, MeshStore> rendererShadowStore in rendererShadowStores)
                {
                    rendererShadowStore.Value.dispose();
                }
                rendererShadowStores.Clear();
            }

            if (skinResource != null)
            {
                AssetsManager.bindEventHandle(skinResource, onSkinResourceHandle, false);
                skinResource.removeEventListener(EventX.PROGRESS, onProgressHandle);
                skinResource.release();
                skinResource = null;
            }

            if (_animator != null)
            {
                _animator.runtimeAnimatorController = null;
                _animator = null;
            }
            if (_overrideController != null)
            {
                _overrideController.runtimeAnimatorController = null;
                _overrideController = null;
            }

            if (__actorManager != null)
            {
                __actorManager.removeByInstanceID(this.GetInstanceID());
            }
        }

        public Vector3 getForwardPosition(float len = 1)
        {
            return transform.position + (transform.forward*len);
        }
    }
}