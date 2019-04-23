using System.Collections.Generic;
using UnityEngine;

namespace gameSDK
{
    public class Node
    {
        public float f; //总评估值
        public float g; //到起点的值
        public Node parent;
        public float x;
        public float y;
        public int version = 1;
        public List<Link> links = new List<Link>();

        internal int index = -1;
        public Node(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float distance(Node node)
        {
            float dx = x - node.x;
            float dy = y - node.y;
            return Mathf.Sqrt(dx*dx + dy*dy);
        }
    }
}