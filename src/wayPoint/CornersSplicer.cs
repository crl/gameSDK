using System.Collections.Generic;
using System.Linq;
using foundation;
using UnityEngine;

namespace gameSDK
{
    /// <summary>
    /// 
    /// </summary>
    public class CornersSplicer
    {
        private static Vector3 normalDir;
        private static Vector3 position;
        private static Vector3 dir = Vector3.one;

        private static List<Vector3> tempList=new List<Vector3>();
        public static List<Vector3> Splice(Vector3[] corners, float minSize = 1.0f, List<Vector3> result = null)
        {
            tempList.Clear();
            int len = corners.Length;
            for (int i = 0; i < len; i++)
            {
                tempList.Add(corners[i]);
            }
             
            SplicePoint(tempList, minSize, result);
            return result;
        }


        public static List<Vector3> SplicePoint(List<Vector3> corners, float minSize = 1.0f, List<Vector3> result = null)
        {
            if (result == null)
            {
                result = new List<Vector3>();
            }
            result.Clear();
            if (corners.Count == 0)
            {
                return result;
            }

            for (int i = 1; i < corners.Count; i++)
            {
                if (corners[i] == corners[i - 1])
                {
                    corners.RemoveAt(i);
                    i--;
                }
            }

            int slot = 0;
            int count = corners.Count;

            int total = 0;

            while (slot < count)
            {
                total++;

                if (total > 1000000)
                {
                    DebugX.LogError("total:"+ total);
                    break;
                }
                int resultCount = result.Count;
                if (resultCount == 0)
                {//是第一个，直接添加进列表
                    Vector3 temp = corners[slot];
                    //VectorUtils.getNavPosNear(corners[slot], corners[slot] + Vector3.down * 10, out temp);
                    result.Add(corners[slot]);
                    slot++;
                }
                else
                {
                    Vector3 v = result[resultCount - 1];//开始点
                    Vector3 addForward = (corners[slot] - v).normalized;//朝向
                    float lastLengthSquared = VectorUtils.DistanceSquared(v, corners[slot], false);
                    if (lastLengthSquared < minSize * minSize )
                    {
                        Vector3 temp = corners[slot];
                        //VectorUtils.getNavPosNear(corners[slot], corners[slot] + Vector3.down * 10, out temp);
                        result.Add(temp);
                        slot++;
                    }
                    else
                    {
                        Vector3 temp = v + addForward*minSize;
                        //VectorUtils.getNavPosNear(temp, temp + Vector3.down * 10, out temp);
                        result.Add(temp);
                    }
                }

                if (resultCount > 1000)
                {
                    DebugX.Log("SplicePoint count:"+ resultCount);
                }
            }
            return result;
        }


        public static List<Vector3> Splice(List<Vector3> corners, float minSize = 1.0f, List<Vector3> result = null)
        {
            if (result == null)
            {
                result = new List<Vector3>();
            }

            int len = corners.Count;
            int seg = 1;
            for (int i = 1; i < len; i++)
            {
                position = corners[i - 1];
                if (i < len)
                {
                    dir = corners[i] - position;
                }
                if (dir.sqrMagnitude < 0.01f)
                {
                    continue;
                }
                result.Add(position);

                normalDir = dir;
                normalDir.Normalize();

                seg = Mathf.FloorToInt(dir.magnitude / minSize);

                for (int j = 0; j < seg; j++)
                {
                    result.Add(position + normalDir * minSize * (j + 1));
                }
            }

            result.Add(corners[len - 1]);
            return result;
        }
    }
}