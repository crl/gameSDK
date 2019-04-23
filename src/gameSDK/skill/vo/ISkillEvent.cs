namespace gameSDK
{
    public interface ISkillEvent
    {
        void init(BaseSkill skill,SkillLine line,SkillPoint point);
        bool enabled { get; set; }

        /// <summary>
        /// 复制一份数据;
        /// </summary>
        /// <returns></returns>
        ISkillEvent clone();
    }


    public enum SkillEventState
    {
        ENTER,
        UPDATE,
        EXIT,
        FIRST_START,
        FORCE_EXIT
    }

}