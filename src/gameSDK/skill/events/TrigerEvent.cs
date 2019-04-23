using System.Collections.Generic;
using foundation;

namespace gameSDK
{
    public enum SkillEventType
    {
        Fight=0,
        AOE=1,
        Buff=2,
        Lock=3,
        Unlock=4,
        DIY=5,
        Collider=6
    }

    public class TrigerEvent : SkillEvent, IAmfSetMember
    {
        public const string EVENT_PREFIX = "Skill_";
        public const string EFFECT_CREATE = "Skill_Create";
        public string eventType = "trigger";

        /// <summary>
        /// 0为空事件
        /// </summary>
        public SkillEventType type = SkillEventType.DIY;

        public override ISkillEvent clone()
        {
            TrigerEvent e=new TrigerEvent();
            e.eventType = this.eventType;
            e.type = this.type;

            return e;
        }

        public override void enter()
        {
            switch (type)
            {
                case SkillEventType.DIY:
                    baseSkill.simpleDispatch(EVENT_PREFIX + eventType, baseSkill);
                    break;
                default:
                    baseSkill.simpleDispatch(EVENT_PREFIX + type, baseSkill);
                    break;
            }
        }

        public override void exit()
        {
            if (type == SkillEventType.Lock)
            {
                baseSkill.simpleDispatch(EVENT_PREFIX + SkillEventType.Unlock, baseSkill);
            }
        }


        public void __AmfSetMember(string key, object value)
        {

            if (key == "type")
            {
                this.type = (SkillEventType) value;
            }
        }
    }
}