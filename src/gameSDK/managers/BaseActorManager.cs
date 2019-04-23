using System;
using System.Collections.Generic;
using foundation;
using UnityEngine;

namespace gameSDK
{
    public class BaseActorManager:BaseObjectManager
    {
        private Dictionary<ObjectType, ASDictionary<int, BaseObject>> _instanceMapping =
            new Dictionary<ObjectType, ASDictionary<int, BaseObject>>();

        public BaseActorManager()
        {

        }

        virtual protected void Start() { 

            RegistCreateType<BaseEffectObject>(ObjectType.Effect);
            RegistCreateType<ImageObject>(ObjectType.PanelAvatar);
            RegistCreateType<BaseObject>(ObjectType.Avatar);
            RegistCreateType<BaseObject>(ObjectType.Npc);

            RegistCreateType<BaseObject>(ObjectType.Hair);
            RegistCreateType<BaseObject>(ObjectType.Wepon);
            RegistCreateType<BaseObject>(ObjectType.Wing);
            RegistCreateType<BaseObject>(ObjectType.Mount);
        }


        public BaseObject createActor(ObjectType objectType, string templeteID = null)
        {
            Type cls = null;
            if (_actorMapping.TryGetValue(objectType, out cls) == false)
            {
                cls = typeof (BaseObject);
            }
            GameObject go = createByPrefab(objectType);
            go.transform.SetParent(AbstractApp.ActorContainer.transform);
            BaseObject baseObject = go.AddComponent(cls) as BaseObject;
            __addByInstanceID(baseObject,objectType);
            if (string.IsNullOrEmpty(templeteID) == false)
            {
                go.name = templeteID;
                baseObject.load(templeteID);
            }

            return baseObject;
        }

        protected virtual GameObject createByPrefab(ObjectType objectType)
        {
            GameObject prefab;
            GameObject go;
            string name = "object_" + objectType.ToString();
            if (_actorPrefab.TryGetValue(objectType, out prefab))
            {
                go = GameObject.Instantiate(prefab) as GameObject;
                go.name= name;
            }
            else
            {
                go = new GameObject(name);
            }
           
            go.transform.SetParent(AbstractApp.ActorContainer.transform, false);
            return go;
        }

        public virtual T createActor<T>(ObjectType objectType = ObjectType.DIY) where T:BaseObject
        {
            GameObject go = createByPrefab(objectType);
            go.transform.SetParent(AbstractApp.ActorContainer.transform);
            T baseObject = go.AddComponent<T>();
            __addByInstanceID(baseObject,objectType);

            return baseObject;
        }
        override public bool __addByInstanceID(BaseObject baseObject,ObjectType objectType)
        {
            BaseObject old;
            int id = baseObject.GetInstanceID();
            if (_allInstanceIdMapping.TryGetValue(id, out old))
            {
                DebugX.Log("加入objID重复");
                return false;
            }

            ASDictionary<int, BaseObject> objectTypeMapping=null;
            if (_instanceMapping.TryGetValue(objectType, out objectTypeMapping)==false)
            {
                objectTypeMapping=new ASDictionary<int, BaseObject>();
                _instanceMapping.Add(objectType,objectTypeMapping);
            }
            _allInstanceIdMapping.Add(id, baseObject);

            baseObject.__objectType = objectType;
            baseObject.__actorManager = this;
            objectTypeMapping.Add(id, baseObject);

            return true;
        }

        override public BaseObject removeByInstanceID(int instanceID)
        {
            if (instanceID == -1)
            {
                return null;
            }

            BaseObject baseObject=null;
            if (_allInstanceIdMapping.TryGetValue(instanceID, out baseObject)==false)
            {
                return null;
            }

            _allInstanceIdMapping.Remove(instanceID);

            ASDictionary<int, BaseObject> objectTypeMapping = null;
            if (_instanceMapping.TryGetValue(baseObject.__objectType, out objectTypeMapping))
            {
                objectTypeMapping.Remove(instanceID);
            }
            return baseObject;
        }

        public List<T> getList<T>(ObjectType objectType = ObjectType.All) where T : BaseObject
        {
            ASDictionary<int, BaseObject> maping = null;
            if (objectType == ObjectType.All)
            {
                maping = _allInstanceIdMapping;
            }
            else
            {
                _instanceMapping.TryGetValue(objectType, out maping);
            }

            if (maping != null)
            {
                List<T> result = new List<T>();
                T obj;
                foreach (int i in maping)
                {
                    obj = maping[i] as T;
                    if (obj != null)
                    {
                        result.Add(obj);
                    }
                }
                return result;
            }
            return null;
        }
    }


}