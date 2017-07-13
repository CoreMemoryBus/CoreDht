namespace CoreDht.Node
{
    public class SuccessorTable : RoutingTable
    {
        public SuccessorTable(NodeInfo identity, int tableLength) : base(tableLength)
        {
        }
    }
}