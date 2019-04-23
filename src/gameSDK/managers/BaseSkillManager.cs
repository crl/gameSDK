using System.Collections.Generic;
using foundation;

namespace gameSDK
{
    public class BaseSkillManager: FoundationBehaviour
    {
        public BaseSkill createSkillBy(BaseObject caster, List<BaseObject> targetList, SkillExData exData=null)
        {
            if (exData==null)
            {
                exData = new SkillExData();
            }

            BaseSkill baseSkill = new BaseSkill();
            baseSkill.createBy(caster, targetList, exData);
            return baseSkill;
        }

        public BaseSkill playSkill(string skillPath, BaseObject caster=null, List<BaseObject> targetList=null,
            SkillExData exData = null)
        {
            if (string.IsNullOrEmpty(skillPath))
            {
                return null;
            }
            BaseSkill baseSkill = createSkillBy(caster, targetList, exData);
            baseSkill.load(skillPath);
            return baseSkill;
        }
    }
}