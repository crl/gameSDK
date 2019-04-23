using foundation;
using System.Collections.Generic;
using UnityEngine;

namespace gameSDK
{
    /// <summary>
    /// 移动碰撞检查
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="percent"></param>
    /// <returns></returns>
    public delegate Vector3 MoveCheckCollider(Vector3 start, Vector3 end, float percent);
    public class MoveEvent:SkillEvent,IAmfSetMember
    {
        public static MoveCheckCollider moveCheckCollider;

        public Vector3 position;
        public EventTargetType type= EventTargetType.Effect;
       
        /// <summary>
        /// 完成后返回原始坐标
        /// </summary>
        public bool reback = false;
        public bool isInterpolation = true;
        /// <summary>
        /// 是否按此速度进行偏移
        /// </summary>
        public bool isSpeed = false;

        /// <summary>
        /// 检查碰撞
        /// </summary>
        public bool checkCollider = false;

        /// <summary>
        /// 移动方式
        /// </summary>
        //public EaseType easeType= EaseType.Linear;

        override public ISkillEvent clone()
        {
            MoveEvent e = new MoveEvent();
            e.type = type;
            e.position = position;
            e.reback = reback;
            e.checkCollider = checkCollider;
            //e.easeType = easeType;
            e.isInterpolation = isInterpolation;
            return e;
        }

        protected void moveLocal(BaseObject caster, Vector3 offset)
        {
            if (caster == null)
            {
                return;
            }

            Vector3 pos = caster.transform.localToWorldMatrix.MultiplyPoint(offset);
            pos = caster.getNavPos(pos);

            caster.transform.position = pos;
        }
        
        private ASDictionary<BaseObject, Vector3> _actorDirect=new ASDictionary<BaseObject, Vector3>();
        public override void enter()
        {
            BaseObject caster = baseSkill.getCaster();
            if (caster == null)
                return;
            if (isInterpolation==false)
            {
                switch (type)
                {
                    case EventTargetType.Caster:
                        moveLocal(caster, position);
                        break;
                    case EventTargetType.Target:
                        List<BaseObject> targetList = baseSkill.getTargetList();
                        foreach (BaseObject item in targetList)
                        {
                            moveLocal(item, position);
                        }

                        break;
                    case EventTargetType.Effect:
                        foreach (BaseObject item in line.effectList)
                        {
                            moveLocal(item, position);
                        }
                        break;
                }
            }
            else
            {
                switch (type)
                {
                    case EventTargetType.Caster:
                        if (caster != null)
                        {
                            _actorDirect.Add(caster, caster.position);
                        }
                        break;

                    case EventTargetType.Target:
                        List<BaseObject> targetList = baseSkill.getTargetList();
                        foreach (BaseObject item in targetList)
                        {
                            if (item != null)
                            {
                                _actorDirect.Add(item, item.position);
                            }
                        }
                        break;

                    case EventTargetType.Effect:
                        List<BaseObject> effectList = line.effectList;
                        foreach (BaseObject item in effectList)
                        {
                            if (item != null)
                            {
                                _actorDirect.Add(item, item.position);
                            }
                        }
                        break;
                }
            }
        }

        public override void update(float avg=0.0f)
        {
            if (isInterpolation == false)
            {
                return;
            }

            switch (type)
            {
                case EventTargetType.Caster:
                    move(baseSkill.getCaster(), avg);
                    break;

                case EventTargetType.Target:
                    List<BaseObject> targetList = baseSkill.getTargetList();
                    foreach (BaseObject item in targetList)
                    {
                        move(item, avg);
                    }
                    break;

                case EventTargetType.Effect:
                    List<BaseObject> effectList = line.effectList;
                    foreach (BaseObject item in effectList)
                    {
                        move(item, avg);
                    }
                    break;
            }
        }

        private float _oldavg = -1.0f;

        private void move(BaseObject baseObject, float avg)
        {
            if (avg < _oldavg)
            {
                return;
            }
            _oldavg = avg;
            if (baseObject == null)
            {
                return;
            }
            Vector3 startPosition = baseObject.transform.position;
            Vector3 pos=Vector3.zero;
            if (isSpeed)
            {
                pos = baseObject.transform.localToWorldMatrix.MultiplyPoint(position * Time.deltaTime);
            }
            else
            {
                _actorDirect.TryGetValue(baseObject, out startPosition);
                //pos = Easing.Ease(easeType, startPosition, startPosition + baseObject.rotation * position, avg);// startPosition + (baseObject.rotation * position * avg);
            }

            if (moveCheckCollider != null)
            {
                pos = moveCheckCollider(startPosition, pos, avg);
            }
            else
            {
                pos = baseObject.getNavPos(pos);
            }
            baseObject.transform.position = pos;
        }


        public override void exit()
        {
            if (isInterpolation == false)
            {
                return;
            }

            if (reback)
            {
                _oldavg = -1.0f;

                switch (type)
                {
                    case EventTargetType.Caster:
                        move(baseSkill.getCaster(), 0);
                        break;

                    case EventTargetType.Target:
                        List<BaseObject> targetList = baseSkill.getTargetList();
                        foreach (BaseObject item in targetList)
                        {
                            move(item, 0);
                        }

                        break;

                    case EventTargetType.Effect:
                        List<BaseObject> effectList = line.effectList;
                        foreach (BaseObject item in effectList)
                        {
                            move(item, 0);
                        }
                        break;
                }
            }

            _actorDirect.Clear();
        }

        public void __AmfSetMember(string key, object value)
        {
            if (key == "position")
            {
                position = AmfHelper.getVector2(value);
            }else if (key == "type")
            {
                type = (EventTargetType) value;
            }
            else if (key == "easeType")
            {
                //easeType = (EaseType)value;
            }
        }
    }
}