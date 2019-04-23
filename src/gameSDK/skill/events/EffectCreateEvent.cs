using System.Collections.Generic;
using foundation;
using UnityEngine;

namespace gameSDK
{
    public class EffectCreateEvent:SkillEvent
    {
        public string effectPath;
        public bool isBindSkeleton = true;
        public string skeletonName = "";
        public bool useTarget = false;
        public bool isCollider = false;
        /// <summary>
        /// 默认使用
        /// </summary>
        public bool useCasterScale = true;

        /// <summary>
        /// 是否一次性定位而已
        /// </summary>
        public bool isBindOnce = false;

        /// <summary>
        /// 是否也绑定旋转
        /// </summary>
        public bool hasRotation = true;

        /// <summary>
        /// 是否使用Target的图层
        /// </summary>
        public bool useTargetLayer = false;

        /// <summary>
        /// 播放速度
        /// </summary>
        public float particlePlaybackSpeed = 1.0f;

        public Vector3 offset=Vector3.zero;
        public Vector3 offRotation = Vector3.zero;
        public EffectCreateEvent()
        {
        }


        public override ISkillEvent clone()
        {
            EffectCreateEvent e=new EffectCreateEvent();
            e.effectPath = this.effectPath;
            e.isBindOnce = this.isBindOnce;
            e.hasRotation = this.hasRotation;
            e.isBindSkeleton = this.isBindSkeleton;
            e.isCollider = this.isCollider;
            e.offRotation = this.offRotation;
            e.offset = this.offset;
            e.particlePlaybackSpeed = this.particlePlaybackSpeed;
            e.skeletonName = this.skeletonName;
            e.useTarget = this.useTarget;
            e.useTargetLayer = this.useTargetLayer;

            return e;
        }

        override public void firstStart()
        {
            if (string.IsNullOrEmpty(effectPath))
            {
                return;
            }
            List<BaseObject> targetList = baseSkill.getTargetList();
            BaseObject caster = baseSkill.getCaster();
            BaseObject effectObject;
            switch (line.targetType)
            {
                case EventTargetType.Caster:
                    if (caster != null)
                    {
                        effectObject = createEffect(caster, effectPath, useTargetLayer,particlePlaybackSpeed);
                        autoBind(effectObject, caster);
                    }
                    else
                    {
                        effectObject=createByEffect(caster);
                    }
                    break;
                case EventTargetType.Target:
                    if (targetList != null && targetList.Count > 0)
                    {
                        int len = targetList.Count;
                        for (int i = 0; i < len; i++)
                        {
                            BaseObject target = targetList[i];
                            if (target == null)
                            {
                                continue;
                            }
                            effectObject = createEffect(target, effectPath, useTargetLayer,particlePlaybackSpeed);
                            autoBind(effectObject, target);
                        }
                    }
                    break;

                case EventTargetType.Effect:
                    effectObject = createByEffect(caster);
                    break;
            }

            baseSkill.effectObjectEventStateUpdate(line, this, SkillEventState.FIRST_START);
        }

        private BaseObject createByEffect(BaseObject baseObject)
        {
            BaseObject effectObject = createEffect(baseObject, effectPath, true, particlePlaybackSpeed);

            transformByExDat(effectObject);

            effectObject.addReayHandle(e =>
            {
                trailRest(effectObject);
            });

            checkCollider(effectObject);
            return effectObject;
        }

        /// <summary>
        /// 偏移位置
        /// </summary>
        /// <param name="effectObject"></param>
        /// <param name="skillExData"></param>
        private void transformByExDat(BaseObject effectObject)
        {
            SkillExData skillExData = baseSkill.getExData();
            Vector3 position = skillExData.position;
            if (useTarget)
            {
                if (skillExData.ContainsKey(SkillExData.EFFECT_TARGET))
                {
                    position = skillExData.getVector3(SkillExData.EFFECT_TARGET);
                }
            }
            Vector3 scale = skillExData.scale;
            bool hasScale = true;
            if (scale == Vector3.zero)
            {
                hasScale = false;
                scale = Vector3.one;
            }

            Vector3 rotation = skillExData.eulerAngles;
            Matrix4x4 matrix4X4 = Matrix4x4.TRS(position, Quaternion.Euler(rotation), scale);
            effectObject.position = matrix4X4.MultiplyPoint(offset);

            effectObject.transform.localEulerAngles = rotation + offRotation;

            if (hasScale)
            {
                effectObject.scale = scale;
            }
        }

        private void autoBind(BaseObject effectObject, BaseObject target)
        {
            if (isBindSkeleton)
            {
                Transform skeleton = target.getSkeleton(skeletonName);
                if (skeleton == null)
                {
                    skeleton = target.transform;
                }

                if (isBindOnce)
                {
                    effectObject.transform.position = skeleton.TransformPoint(offset);

                    if (hasRotation)
                    {
                        Quaternion q = skeleton.rotation;
                        effectObject.transform.eulerAngles = q.eulerAngles + offRotation;
                    }
                }
                else
                {
                    effectObject.transform.SetParent(skeleton, false);
                    effectObject.transform.localPosition = offset;
                    if (hasRotation)
                    {
                        effectObject.transform.localEulerAngles = offRotation;
                    }
                }
            }
            else
            {
                transformByExDat(effectObject);
            }
            effectObject.addReayHandle(e =>
            {
                trailRest(effectObject);
            });
            checkCollider(effectObject);
        }

        private void trailRest(BaseObject effectObject)
        {
            GameObject go = effectObject.skin;
            if (go == null)
            {
                return;
            }
            ///清理拖尾
            TrailRenderer[] trailRenderers = effectObject.GetComponentsInChildren<TrailRenderer>();
            if (trailRenderers.Length > 0)
            {
                foreach (TrailRenderer trailRenderer in trailRenderers)
                {
                    trailRenderer.Reset(BaseApp.Instance);
                }
            }

            ParticleSystem[] particleSystems = effectObject.GetComponentsInChildren<ParticleSystem>();
            if (particleSystems.Length > 0)
            {
                foreach (ParticleSystem particleSystem in particleSystems)
                {
                    particleSystem.SetActive(false);
                }

                foreach (ParticleSystem particleSystem in particleSystems)
                {
                    particleSystem.SetActive(true);
                }
            }
        }

        private void checkCollider(BaseObject effectObject)
        {
            if (isCollider)
            {
                baseSkill.simpleDispatch(TrigerEvent.EVENT_PREFIX + SkillEventType.Collider, effectObject);
            }
        }

        public override void update(float avg = 0)
        {
            baseSkill.effectObjectEventStateUpdate(line, this, SkillEventState.UPDATE);
            base.update(avg);
        }

        public override void exit()
        {
            baseSkill.effectObjectEventStateUpdate(line, this, SkillEventState.EXIT);
            base.exit();
        }

        protected virtual BaseObject createEffect(BaseObject target, string effectPath, bool useTargetLayer, float playbackSpeed = 1.0f)
        {
            BaseEffectObject effectObject = BaseApp.effectManager.createEffect();
            line.addEffect(effectObject);

            SkillExData skillExData = baseSkill.getExData();
            if (skillExData.forceLayer != -1)
            {
                effectObject.layer = skillExData.forceLayer;
            }
            else if (useTargetLayer == true && target != null)
            {
                effectObject.layer = target.layer;
            }
            effectObject.name = line.name;
            effectObject.playbackSpeed = playbackSpeed;
            effectObject.load(effectPath);

            baseSkill.simpleDispatch(TrigerEvent.EFFECT_CREATE, effectObject);

            return effectObject;
        }
    }
}