using System;
using System.Collections.Generic;
using foundation;
using UnityEngine;

namespace gameSDK
{
    [Serializable]
    public class PlayAnimEvent : SkillEvent
    {
        public string aniName;

        /// <summary>
        /// 是否强制切换
        /// </summary>
        public bool isForce = true;

        public float offsetAvg = 0.0f;
        public PlayAnimEvent()
        {

        }

        public override ISkillEvent clone()
        {
            PlayAnimEvent e=new PlayAnimEvent();
            e.aniName = this.aniName;
            e.isForce = this.isForce;
            e.offsetAvg = this.offsetAvg;
            return e;
        }

        public override void firstStart()
        {
            if (string.IsNullOrEmpty(aniName))
            {
                return;
            }

            List<BaseObject> resultList=null;
            switch (line.targetType)
            {
                case EventTargetType.Caster:
                    BaseObject caster = baseSkill.getCaster();
                    if (caster != null)
                    {
                        resultList=new List<BaseObject>(){caster};
                    }
                    break;

                case EventTargetType.Target:
                    resultList = baseSkill.getTargetList();
                    break;
                case EventTargetType.Effect:
                    resultList = line.effectList;
                    break;
            }
            if (resultList != null)
            {
                int len = resultList.Count;
                for (int i = 0; i < len; i++)
                {
                    BaseObject target = resultList[i];
                    if (target)
                    {
                        if (isForce)
                        {
                            target.playAnim(aniName, 0, offsetAvg);
                        }
                        else
                        {
                            if (offsetAvg != 0.0f)
                            {
                                target.playAnim(aniName, 0, offsetAvg);
                            }
                            else
                            {
                                target.playAnim(aniName);
                            }
                        }
                    }
                }
            }
        }
    }
}