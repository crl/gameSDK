using System;

namespace gameSDK
{
    [Serializable]
    public class SkillPointVO
    {
        public SkillEvent evt;

        /// <summary>
        /// 开始时间
        /// </summary>
        public int startTime;

        /// <summary>
        /// 是否是空数据;
        /// </summary>
        public bool isEmpty
        {
            get { return evt==null; }
        }

        public bool addEvent(SkillEvent value)
        {
            if (value == null || evt!=null)
            {
                return false;
            }
            evt = value;
            return true;
        }

        public bool removeEvent(SkillEvent value)
        {
            if (evt != value)
            {
                return false;
            }
            evt = null;
            return true;
        }
    }
}