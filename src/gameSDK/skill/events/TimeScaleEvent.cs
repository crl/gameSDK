using UnityEngine;

namespace gameSDK
{
    public class TimeScaleEvent:SkillEvent
    {
        public float timeScale = 1.0f;

        public float delay = -1;

        private float startTime = 0;
        override public ISkillEvent clone()
        {
            TimeScaleEvent e = new TimeScaleEvent();
            e.timeScale = this.timeScale;
            e.delay = this.delay;
            e.startTime = this.startTime;
            return e;
        }

        public override void enter()
        {
            if (timeScale < 0.002)
            {
                timeScale = 0.002f;
            }
            Time.timeScale = timeScale;
            startTime = Time.time;
        }

        public override void update(
            float avg = 0)
        {
            if (delay > 0 && Time.time - startTime > delay)
            {
                Time.timeScale = 1.0f;
            }
        }
        public override void exit()
        {
            Time.timeScale = 1.0f;
        }
    }
}