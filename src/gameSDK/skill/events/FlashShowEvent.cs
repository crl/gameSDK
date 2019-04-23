using System.Collections.Generic;
using foundation;
using UnityEngine;

namespace gameSDK
{
    /// <summary>
    /// 闪现
    /// </summary>
    public class FlashShowEvent: SkillEvent
    {
        public bool isShow = true;
        public Vector3 offset=Vector3.zero;
        public bool useTarget = false;

        public override ISkillEvent clone()
        {
            FlashShowEvent e=new FlashShowEvent();
            e.isShow = this.isShow;
            e.offset = this.offset;
            e.useTarget = this.useTarget;
            return e;
        }

        public override void enter()
        {
            base.enter();

            BaseObject caster = baseSkill.getCaster();
            List<BaseObject> targetList = baseSkill.getTargetList();

            switch (line.targetType)
            {
                case EventTargetType.Caster:
                    toggle(caster, isShow);
                    updateOffset(caster);
                    break;

                case EventTargetType.Effect:
                    foreach (BaseObject item in line.effectList)
                    {
                        toggle(item, isShow);
                        updateOffset(item);
                    }

                    break;
                case EventTargetType.Target:
                    foreach (BaseObject item in targetList)
                    {
                        toggle(item, isShow);
                        updateOffset(item);
                    }
                    break;
            }
        }

        private void toggle(BaseObject target,bool isShow)
        {
            if (target == null)
            {
                return;
            }
            Renderer[] renderers= target.gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.enabled = isShow;
            }
        }

        private void updateOffset(BaseObject target)
        {
            if (useTarget)
            {
                SkillExData skillExData = baseSkill.getExData();
                Vector3 v = skillExData.getVector3(SkillExData.FLASH_TARGET);
                if (v != Vector3.zero)
                {
                    target.position = v;
                }
            }
            else if (offset != Vector3.zero)
            {
                Vector3 off = target.transform.TransformPoint(offset);
                target.position = off;
            }
        }

        public override void exit()
        {
            base.exit();

            if (isShow==false)
            {
                switch (line.targetType)
                {
                    case EventTargetType.Caster:
                        BaseObject caster = baseSkill.getCaster();
                        toggle(caster,true);
                        break;

                    case EventTargetType.Effect:
                        foreach (BaseObject lineEffect in line.effectList)
                        {
                            toggle(lineEffect, true);
                        }

                        break;
                    case EventTargetType.Target:
                        List<BaseObject> targetList = baseSkill.getTargetList();
                        if (targetList != null)
                        {
                            foreach (BaseObject baseObject in targetList)
                            {
                                toggle(baseObject, true);
                            }
                        }
                        break;
                }
            }
        }
    }
}