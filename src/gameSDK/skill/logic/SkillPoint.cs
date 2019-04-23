namespace gameSDK
{
    public class SkillPoint
    {
        public SkillEvent e;
        public int startTime;

        private float _percent = -1;
        public float percent
        {
            get { return _percent; }
        }

        public SkillPoint(BaseSkill baseSkill,SkillLine line,SkillPointVO vo)
        {
            ISkillEvent skillEvent = vo.evt;
            if (skillEvent.enabled)
            {
                e = (SkillEvent)skillEvent.clone();
                e.init(baseSkill, line, this);
                startTime = vo.startTime;
            }
        }

        public void start(int runCount)
        {
            _percent = 0;
            if (e == null)
            {
                return;
            }
            if (runCount==0)
            {
                e.firstStart();
            }
            e.enter();
        }

        public void update(float percent)
        {
            _percent = percent+0.01f;
            if (e == null)
            {
                return;
            }
            e.update(_percent);
        }

        /// <summary>
        /// 已完成次数(如果层循环时 会调用);
        /// </summary>
        /// <param name="count"></param>
        public void finished(int count)
        {
        }

        public void exit(bool isFroce)
        {
            if (e == null)
            {
                return;
            }
            if (isFroce)
            {
                e.forceStop(_percent);
            }
            else
            {
                _percent = 1.0f;
                e.exit();
            }
        }
    }
}