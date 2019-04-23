using System;
using System.Collections.Generic;
using foundation;
using UnityEngine;

namespace gameSDK
{
    public class CameraShakeEvent : SkillEvent, IAmfSetMember
    {
        /// <summary>
        /// 幅度
        /// </summary>
        public float factor = 1f;
        /// <summary>
        /// 周期
        /// </summary>
        public float period = 1f;
        public Vector3 shaderVector = Vector3.zero;

        private float startTime;
        override public ISkillEvent clone()
        {
            CameraShakeEvent e = new CameraShakeEvent();
            e.factor = factor;
            e.period = period;
            e.shaderVector = shaderVector;
            return e;
        }

        public override void enter()
        {
            startTime = Time.time;
            base.enter();
        }

        public override void update(float avg = 0)
        {
            if (baseSkill.getExData().isHero == false)
            {
                return;
            }


            float v = factor * Mathf.Sin((Time.time - startTime) * period * 2 * Mathf.PI);

            if (shaderVector == Vector3.zero)
            {
                BaseApp.cameraController.hitcamByFactor(v);
            }
            else
            {
                BaseApp.cameraController.hitcamByFactor(v,shaderVector);
            }
        }

        public override void exit()
        {
            BaseApp.cameraController.hitcamByFactor(0);
        }
        public void __AmfSetMember(string key, object value)
        {

            if (key == "shaderVector")
            {
                shaderVector = AmfHelper.getVector2(value);
            }
        }
    }
}