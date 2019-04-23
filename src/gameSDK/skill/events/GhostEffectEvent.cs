using System.Collections.Generic;

namespace gameSDK
{
    public class GhostEffectEvent:SkillEvent
    {
        //持续时间
        public float duration = 2f;
        //创建新残影间隔
        public float interval = 0.1f;

        public bool onPositionChange = true;


        public override ISkillEvent clone()
        {
            GhostEffectEvent e=new GhostEffectEvent();
            e.duration = this.duration;
            e.interval = this.interval;
            e.onPositionChange = this.onPositionChange;
            return e;
        }

        public override void enter()
        {
            BaseObject caster = baseSkill.getCaster();
            if (caster != null)
            {
                GhostObject ghostObject = caster.GetComponent<GhostObject>();
                if (ghostObject == null)
                {
                    ghostObject = caster.gameObject.AddComponent<GhostObject>();
                }
                if (ghostObject.enabled == false)
                {
                    ghostObject.enabled = true;
                }
                ghostObject.duration = duration;
                ghostObject.interval = interval;
                ghostObject.onPositionChange = onPositionChange;
            }
        }

        public override void exit()
        {
            BaseObject caster = baseSkill.getCaster();
            if (caster)
            {
                GhostObject ghostObject = caster.GetComponent<GhostObject>();
                if (ghostObject != null)
                {
                    ghostObject.enabled = false;
                }
            }
        }
    }
}