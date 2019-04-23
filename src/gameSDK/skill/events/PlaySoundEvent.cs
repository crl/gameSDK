using System;
using System.Collections.Generic;
using foundation;

namespace gameSDK
{
    [Serializable]
    public class PlaySoundEvent:SkillEvent
    {
        public string m_sound1 = "";
        public string m_sound2 = "";
        public string m_sound3 = "";
        public string m_sound4 = "";
        ///仅主角
        public bool isOnlyHero = true;

        /// <summary>
        /// 一次
        /// </summary>
        public bool isOnce = true;
        public PlaySoundEvent()
        {
        }

        public override ISkillEvent clone()
        {
            PlaySoundEvent e=new PlaySoundEvent();
            e.isOnlyHero = this.isOnlyHero;
            e.isOnce = this.isOnce;
            e.m_sound1 = this.m_sound1;
            e.m_sound2 = this.m_sound2;
            e.m_sound3 = this.m_sound3;
            e.m_sound4 = this.m_sound4;
            return e;
        }

        protected SoundClip m_sound = null;

        public override void firstStart()
        {
            ///仅主角
            if (isOnlyHero)
            {
                if (baseSkill.getExData().isHero==false)
                {
                    return;
                }
            }

            List<string> tempLst = new List<string>();
            if (string.IsNullOrEmpty(m_sound1) == false)
                tempLst.Add(m_sound1);
            if (string.IsNullOrEmpty(m_sound2) == false)
                tempLst.Add(m_sound2);
            if (string.IsNullOrEmpty(m_sound3) == false)
                tempLst.Add(m_sound3);
            if (string.IsNullOrEmpty(m_sound4) == false)
                tempLst.Add(m_sound4);

            if (tempLst.Count > 0)
            {
                int idx = UnityEngine.Random.Range(0, tempLst.Count);
                if (isOnce)
                {
                    AbstractApp.soundsManager.playSoundOnce(tempLst[idx], true, false);
                }
                else
                {
                    m_sound = AbstractApp.soundsManager.playSound(tempLst[idx]);
                }
            }
        }

        public override void exit()
        {
            if (m_sound != null)
            {
                m_sound.Stop();
                m_sound = null;
            }
        }
    }
}