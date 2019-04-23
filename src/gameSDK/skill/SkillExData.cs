using foundation;
using UnityEngine;

namespace gameSDK
{
    public class SkillExData:ASDictionary
    {
        public const string EFFECT_TARGET="et";
        public const string FLASH_TARGET = "ft";

        public Vector3 position;
        public Vector3 eulerAngles;
        public Vector3 scale;

        public GameObject parent;
        public bool canSkip = false;

        public bool needFloor = false;
        public bool limited = false;

        public bool isHero = false;
        public LayerMask forceLayer = -1;

        public void copyTo(SkillExData skill)
        {
            foreach (string key in this)
            {
                skill[key] = this[key];
            }

            skill.position = this.position;
            skill.eulerAngles = this.eulerAngles;
            skill.scale = this.scale;

            skill.isHero = this.isHero;
            skill.needFloor = this.needFloor;
            skill.limited = this.limited;
            skill.forceLayer = this.forceLayer;
            skill.canSkip = this.canSkip;

            skill.parent = this.parent;
        }
    }
}
