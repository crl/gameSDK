using System;
using System.Collections.Generic;
using foundation;
using UnityEngine;

namespace gameSDK
{
    public class BaseObjectManager: FoundationBehaviour
    {
        protected ASDictionary<int, BaseObject> _allInstanceIdMapping = new ASDictionary<int, BaseObject>();

        protected static Dictionary<ObjectType, Type> _actorMapping = new Dictionary<ObjectType, Type>();
        protected static Dictionary<ObjectType, GameObject> _actorPrefab = new Dictionary<ObjectType, GameObject>();
        public static void RegistCreateType<T>(ObjectType type) where T : BaseObject
        {
            Type cls = typeof(T);
            if (_actorMapping.ContainsKey(type))
            {
                _actorMapping[type] = cls;
            }
            else
            {
                _actorMapping.Add(type, cls);
            }
        }

        public static void RegistCreatePrefab(ObjectType type, GameObject prefab)
        {
            if (_actorPrefab.ContainsKey(type))
            {
                _actorPrefab[type] = prefab;
            }
            else
            {
                _actorPrefab.Add(type, prefab);
            }
        }

        public virtual bool __addByInstanceID(BaseObject baseObject, ObjectType objectType)
        {
            BaseObject old;
            int id = baseObject.GetInstanceID();
            if (_allInstanceIdMapping.TryGetValue(id, out old))
            {
                DebugX.Log("加入objID重复");
                return false;
            }

            _allInstanceIdMapping.Add(id, baseObject);

            baseObject.__objectType = objectType;
            baseObject.__actorManager = this;

            return true;
        }

        public virtual BaseObject removeByInstanceID(int instanceID)
        {
            if (instanceID == -1)
            {
                return null;
            }

            BaseObject baseObject = null;
            if (_allInstanceIdMapping.TryGetValue(instanceID, out baseObject) == false)
            {
                return null;
            }

            _allInstanceIdMapping.Remove(instanceID);

            return baseObject;
        }

        public T getByInstanceID<T>(int InstanceID) where T : BaseObject
        {
            BaseObject value = null;
            _allInstanceIdMapping.TryGetValue(InstanceID, out value);
            return value as T;
        }

    }
}