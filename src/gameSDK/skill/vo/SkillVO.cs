using System;
using System.Collections;
using System.Collections.Generic;
using foundation;

namespace gameSDK
{
    [Serializable]
    public class SkillTimeLineVO:IAmfSetMember
    {
        public List<SkillLineVO> lines=new List<SkillLineVO>();

        public void addLine(SkillLineVO lineVo)
        {
            lines.Add(lineVo);
        }

        public void clear()
        {
            lines.Clear();
        }

        public void removeLine(SkillLineVO lineVo)
        {
            int index=lines.IndexOf(lineVo);
            if (index != -1)
            {
                lines.RemoveAt(index);
            }
        }

        public void __AmfSetMember(string key, object value)
        {
            if (key == "lines")
            {
                AmfHelper.copyIList(lines, value as IList);
            }
        }
    }
}