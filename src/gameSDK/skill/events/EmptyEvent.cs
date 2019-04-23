using System.Collections.Generic;

namespace gameSDK
{

    public class EmptyEvent : SkillEvent
    {
        public EmptyEvent()
        {
        }
        public override ISkillEvent clone()
        {
            return this;
        }
    }

}