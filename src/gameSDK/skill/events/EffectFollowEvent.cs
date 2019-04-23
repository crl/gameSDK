using System;
using System.Collections.Generic;
using UnityEngine;

namespace gameSDK
{
    public class EffectFollowEvent : SkillEvent
    {
        public bool scale = true;
        public bool rotation = true;
        public bool position = true;

        public string skeletonName = "";

        public Vector3 positionOffset = Vector3.zero;
        public Vector3 rotationOffset = Vector3.zero;

        private Transform target;

        override public ISkillEvent clone()
        {
            EffectFollowEvent e = new EffectFollowEvent();
            e.scale = scale;
            e.rotation = rotation;
            e.position = position;

            e.skeletonName = skeletonName;
            e.positionOffset = positionOffset;
            e.rotationOffset = rotationOffset;
            return e;
        }

        public override void firstStart()
        {
            target = null;
            BaseObject caster = baseSkill.getCaster();
            if (caster != null)
            {
                target = caster.getSkeleton(skeletonName);
                if (target == null)
                {
                    target = caster.transform;
                }
            }
        }

        public override void update(float avg = 0)
        {
            for (int i = 0, len = line.effectList.Count; i < len; i++)
            {
                BaseObject item = line.effectList[i];
                if (item == null)
                {
                    continue;
                }
                updateItem(item, avg);
            }
        }

        protected void updateItem(BaseObject f, float avg)
        {
            if (target != null)
            {
                if (position)
                {
                    f.transform.position = target.position + positionOffset;
                }

                if (rotation)
                {
                    if (rotationOffset != Vector3.zero)
                    {
                        f.transform.eulerAngles = target.transform.eulerAngles + rotationOffset;
                    }
                    else
                    {
                        f.transform.rotation = target.rotation;
                    }
                }

                if (scale)
                {
                    f.transform.localScale = target.localScale;
                }
            }
        }
    }
}