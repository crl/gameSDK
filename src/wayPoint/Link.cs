namespace gameSDK
{
    public class Link
    {
        public Node node;
        public float cost;

        public Link(Node node, float cost)
        {
            this.node = node;
            this.cost = cost;
        }
    }
}