using System;
using System.Collections.Generic;

namespace gameSDK
{
    public class BinaryHeap
    {
        public List<Node> a = new List<Node>();
        public Func<Node, Node, bool> sortFuncRef = null;

        public BinaryHeap(Func<Node, Node, bool> sortFunc = null)
        {
            a.Add(new Node(-1,-1));
            if (sortFunc != null)
            {
                sortFunc = this.justMin;
            }
            this.sortFuncRef = sortFunc;
        }

        private bool justMin(Node x, Node y)
        {
            return x.f < y.f;
        }

        public void ins(Node value)
        {
            int p = a.Count;
            a.Add(value);
            int pp = p >> 1;
            while (p > 1 && sortFuncRef(a[p], a[pp]))
            {
                Node temp = a[p];
                a[p] = a[pp];
                a[pp] = temp;
                p = pp;
                pp = p >> 1;
            }
        }

        public Node pop()
        {
            Node min = a[1];
            a[1] = a[a.Count - 1];
            a.RemoveAt(a.Count - 1);

            int p = 1;
            int l = a.Count;
            int sp1 = p << 1;
            int sp2 = sp1 + 1;
            int minp;

            while (sp1 < l)
            {
                if (sp2 < l)
                {
                    minp = sortFuncRef(a[sp2], a[sp1]) ? sp2 : sp1;
                }
                else
                {
                    minp = sp1;
                }
                if (sortFuncRef(a[minp], a[p]))
                {
                    Node temp = a[p];
                    a[p] = a[minp];
                    a[minp] = temp;
                    p = minp;
                    sp1 = p << 1;
                    sp2 = sp1 + 1;
                }
                else
                {
                    break;
                }
            }
            return min;
        }
    }
}