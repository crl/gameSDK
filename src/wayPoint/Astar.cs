using System.Collections.Generic;
using foundation;
using UnityEngine;

namespace gameSDK
{
    public class AStar
    {
        private BinaryHeap open;
        public Node endNode = new Node(0, 0);
        public Node startNode = new Node(0, 0);
        public List<Node> path;
        private int nowVersion = 1;

        public List<Node> nodes = new List<Node>();

        public AStar()
        {
        }

        public void fromObj(PathsCFG pathCFG)
        {
            if (pathCFG != null)
            {

                List<RefVector2> list = pathCFG.list;
                int len = list.Count;
                for (int i= 0; i< len; i++)
                {
                    RefVector2 pos = list[i];
                    Node node = new Node(pos.x + pathCFG.center.x, pos.y + pathCFG.center.y);
                    node.index =i;

                    nodes.Add(node);
                }
                foreach (KeyVector2 linea in pathCFG.connList)
                {
                    int x=pathCFG.getRefVector2IndexByGUID(linea.x);
                    if (x == -1)
                    {
                        continue;
                    }
                    int y = pathCFG.getRefVector2IndexByGUID(linea.y);
                    if (y == -1)
                    {
                        continue;
                    }
                    addLine(x, y);
                }
            }
        }

        public void addLine(int p0, int p1)
        {
            Node n0 = nodes[p0];
            Node n1 = nodes[p1];
            float cost = euclidian(n0, n1);
            Link link0 = new Link(n1, cost);
            n0.links.Add(link0);
            Link link1 = new Link(n0, cost);
            n1.links.Add(link1);
        }

        public void resetStartNodeLinksByRadius(Node node, float radius)
        {
            node.links.Clear();
            
            foreach (Node n in nodes)
            {
                float cost = n.distance(node);
                if (cost < radius)
                {
                    Link link = new Link(n, cost);
                    node.links.Add(link);
                }
            }
        }

        public void resetEndNodeLinksByRadius(Node node, float radius)
        {
            foreach (Link link in node.links)
            {
                if (link.node.links.Count > 0 && link.node.links[link.node.links.Count - 1].node == node)
                {
                    link.node.links.RemoveAt(link.node.links.Count - 1);
                }
            }
            node.links.Clear();
            
            foreach (Node n in nodes)
            {
                float cost = n.distance(node);
                if (cost < radius)
                {
                    Link link1 = new Link(n, cost);
                    node.links.Add(link1);
                    Link link2 = new Link(node, cost);
                    n.links.Add(link2);
                }
            }
        }

        private bool justMin(Node x, Node y)
        {
            return x.f < y.f;
        }

        public bool findPath()
        {
            nowVersion++;
            open = new BinaryHeap(justMin);
            startNode.g = 0;
            return search();
        }

        public bool search()
        {
            Node node = startNode;
            node.version = nowVersion; //模拟close列表
            while (node != endNode)
            {
  
                if (node == null) return false;
                foreach (Link link in node.links)
                {
                    Node test = link.node;
                    float g = node.g + link.cost;
                    float h = euclidian(test, endNode);
                    float f = g + h;
                    if (test.version == nowVersion)
                    {
                        //已经在open表里
                        if (test.f > f)
                        {
                            //曾经的估值大于当前的估值
                            test.f = f;
                            test.g = g;
                            test.parent = node;
                        }
                    }
                    else
                    {
                        //没有在open表里 插入之
                        test.f = f;
                        test.g = g;
                        test.parent = node;
                        open.ins(test);
                        test.version = nowVersion;
                    }
                }
                if (open.a.Count == 1)
                {
                    //open列表里找不到
                    return false;
                }
                node = open.pop() as Node;
            }

            buildPath();
            return true;
        }

        private void buildPath()
        {
            path = new List<Node>();
            Node node = endNode;
            path.Add(node);

            //
            while (node != startNode)
            {
                node = node.parent;
                path.Add(node);
            }
        }

        public float euclidian(Node node0, Node node1)
        {
            float dx = node0.x - node1.x;
            float dy = node0.y - node1.y;
            return Mathf.Sqrt(dx*dx + dy*dy);
        }

        public void scale(float val)
        {
            foreach (Node node in nodes)
            {
                node.x *= val;
                node.y *= val;
            }
        }
    }
}