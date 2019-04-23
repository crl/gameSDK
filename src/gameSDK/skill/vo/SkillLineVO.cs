using System;
using System.Collections;
using System.Collections.Generic;
using foundation;

namespace gameSDK
{
    [Serializable]
    public class SkillLineVO:IAmfSetMember
    {
        public List<SkillPointVO> points = new List<SkillPointVO>();
        public int playCount = 0;
        public string typeFullName="";
        public bool enabled = true;

        public EventTargetType targetType = EventTargetType.Caster;
        public string name;

        public void addPoint(SkillPointVO value)
        {
            points.Add(value);
        }

        public void insert(int index, SkillPointVO pointVO)
        {
            points.Insert(index, pointVO);
        }

        public void removePoint(SkillPointVO pointVo)
        {
            int index=points.IndexOf(pointVo);
            if (index != -1)
            {
                points.RemoveAt(index);
            }
        }

        public void __AmfSetMember(string key, object value)
        {
            if (key == "points")
            {
                AmfHelper.copyIList(points, value as IList);
            
            }else if (key == "targetType")
            {
                targetType = (EventTargetType) value;
            }
        }

        public void copyFrom(SkillLineVO lineVO)
        {
            this.points = lineVO.points;
            this.typeFullName = lineVO.typeFullName;
            this.targetType = lineVO.targetType;
            this.playCount = lineVO.playCount;
            this.name = lineVO.name;
            this.enabled = lineVO.enabled;
        }
    }
}