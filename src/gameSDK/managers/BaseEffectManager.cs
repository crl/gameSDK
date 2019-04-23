using System;
using foundation;
using UnityEngine;

namespace gameSDK
{
    public class BaseEffectManager: BaseObjectManager
    {
        public static Type defaultType=typeof(BaseEffectObject);


        public BaseEffectObject load(string path)
        {
            BaseEffectObject retObj = createEffect();
            retObj.load(path);

            return retObj;
        }
        public BaseEffectObject load(string path,Vector3 position,Quaternion rotation)
        {
            BaseEffectObject retObj = load(path);
            retObj.transform.position = position;
            retObj.transform.rotation = rotation;

            return retObj;
        }

        public BaseEffectObject createEffect(string templeteID = null)
        {
            GameObject go = new GameObject();
            go.name = "effect";
            go.transform.SetParent(BaseApp.EffectContainer.transform);

            BaseEffectObject baseObject = go.AddComponent(defaultType) as BaseEffectObject;
            __addByInstanceID(baseObject, ObjectType.Effect);
            if (string.IsNullOrEmpty(templeteID) == false)
            {
                go.name = templeteID;
                baseObject.load(templeteID);
            }
            return baseObject;
        }

        public T createEffect<T>(string templeteID = null) where T:BaseEffectObject
        {
            GameObject go = new GameObject();
            go.name = "effect";
            go.transform.SetParent(BaseApp.EffectContainer.transform);

            T baseObject = go.AddComponent<T>();
            __addByInstanceID(baseObject, ObjectType.Effect);
            if (string.IsNullOrEmpty(templeteID) == false)
            {
                go.name = templeteID;
                baseObject.load(templeteID);
            }

            return baseObject;
        }
    }
}