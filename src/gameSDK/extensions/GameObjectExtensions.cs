using System.Collections.Generic;
using UnityEngine;

namespace gameSDK
{
    public static class GameObjectExtensions
    {
        public static void PlaySkillOn(this GameObject self, string skillPath, List<BaseObject> targetList = null, SkillExData exData = null)
        {
            BaseObject baseObject = self.GetComponent<BaseObject>();
            if (baseObject == null)
            {
                baseObject = self.AddComponent<BaseObject>();
                baseObject.skin = self;
                baseObject.fireReadyEvent();
            }

            baseObject.playSkill(skillPath, targetList, exData);
        }
        public static void PlaySingleSkillOn(this GameObject self, string skillPath, List<BaseObject> targetList = null, SkillExData exData = null)
        {
            BaseObject baseObject = self.GetComponent<BaseObject>();
            if (baseObject == null)
            {
                baseObject = self.AddComponent<BaseObject>();
                baseObject.skin = self;
                baseObject.fireReadyEvent();
            }

            baseObject.playSingleSkill(skillPath, targetList, exData);
        }
    }
}