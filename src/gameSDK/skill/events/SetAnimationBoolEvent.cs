using System;
using UnityEngine;

namespace gameSDK
{
    [Serializable]
    public class SetAnimationBoolEvent : SkillEvent
    {
        public string key;
        public bool value = true;

        public bool resetDefault = true;
        public override ISkillEvent clone()
        {
            SetAnimationBoolEvent e=new SetAnimationBoolEvent();
            e.key = key;
            e.value = value;
            return e;
        }

        private Animator animator;
        private bool rawValue;
        public override void firstStart()
        {
            BaseObject cast = baseSkill.getCaster();
            if (cast != null)
            {
                animator = cast.getAnimator();
                if (animator != null)
                {
                    rawValue = animator.GetBool(key);
                    animator.SetBool(key, value);
                }
            }

            base.enter();
        }

        public override void enter()
        {
            if(animator != null)
            {
                animator.SetBool(key, value);
            }
        }

        public override void exit()
        {
            if (animator != null && resetDefault)
            {
                animator.SetBool(key, rawValue);
            }
        }
    }
}