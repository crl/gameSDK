using System;
using System.Collections.Generic;
using UnityEngine;

namespace gameSDK
{
    public class PathArrowUtil
    {
        private static Stack<GameObject> pools=new Stack<GameObject>();
        public static GameObject prefab;

        private static List<GameObject> running=new List<GameObject>();

//        private static float minSize = 3.0f;

        public static void Clear()
        {
            int len = running.Count;
            if (len > 0)
            {
                GameObject go = null;
                for (int i = 0; i < len; i++)
                {
                    go = running[i];
                    if (go != null)
                    {
                        go.SetActive(false);
                        pools.Push(go);
                    }
                }
                running.Clear();
            }
        }

        private static List<Vector3> tempList=new List<Vector3>(); 
        public static void Show(List<Vector3> list,Vector3 fromVector3,Vector3 endVector3)
        {
            tempList.Clear();
            tempList.Add(fromVector3);
            foreach (Vector3 vector3 in list)
            {
                tempList.Add(vector3);
            }
            tempList.Add(endVector3);
            Show(tempList);
        }

        public static void Show(List<Vector3> list,bool splite=true)
        {
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>("Arrow");
                if (prefab == null)
                {
                    return;
                }
            }

            Clear();
            int len = list.Count;
            if (len == 0)
            {
                return;
            }

            if (splite)
            {
                list = CornersSplicer.SplicePoint(list, 2);
            }

            int num = list.Count;
            if (num < 2)
            {
                return;
            }
            for (int i = 0; i < num; i++)
            {
                Vector3 forward;
                if (i < num - 1)
                {
                    forward = (list[i + 1] - list[i]).normalized;
                }
                else
                {
                    forward = (list[i] - list[i-1]).normalized;
                }
                createArrow(list[i], list[i] + forward);
            }
            return;
        }

        private static void createArrow(Vector3 itemPostion,Vector3 lookAt)
        {
            GameObject go = null;
            if (pools.Count > 0)
            {
                go = pools.Pop();
                if (go != null)
                {
                    go.SetActive(true);
                }
            }

            if (go == null)
            {
                go = GameObject.Instantiate(prefab);
            }
            running.Add(go);

            go.transform.position = itemPostion;
            go.transform.LookAt(lookAt, Vector3.up);
            go.transform.Rotate(Vector3.up, 90);
            go = null;
        }
    }
}