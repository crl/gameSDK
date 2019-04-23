using System;
using System.Collections.Generic;
using foundation;
using UnityEngine;

namespace gameSDK
{
    [Serializable]
    public class CameraMoveEvent:SkillEvent,IAmfSetMember
    {
        public Vector3 position=new Vector3(0,1,-7);
        public bool focusGet = true;

        public bool forceChange = false;
        override public ISkillEvent clone()
        {
            CameraMoveEvent e = new CameraMoveEvent();
            e.position = position;
            e.focusGet = focusGet;
            e.forceChange = forceChange;
            return e;
        }

        private AbstractBaseObject preFollow;
        public override void enter()
        {
            AbstractBaseObject caster = baseSkill.getCaster();
            if (caster is ImageObject)
            {
                return;
            }

            if (caster != null)
            {
                BaseApp.cameraController.offset=position;
                if (focusGet)
                {
                    preFollow = BaseApp.cameraController.getFollowObject();
                    BaseApp.cameraController.setFollow(caster, forceChange);
                }
            }
        }


        public override void exit()
        {
            BaseObject caster = baseSkill.getCaster();
            if (caster is ImageObject)
            {
                return;
            }

            if (preFollow != null)
            {
                BaseApp.cameraController.setFollow(preFollow, forceChange);
            }

            BaseApp.cameraController.resetDefault();
        }

        public void __AmfSetMember(string key, object value)
        {

            if (key == "position")
            {
                position = AmfHelper.getVector2(value);
            }
        }
    }
}