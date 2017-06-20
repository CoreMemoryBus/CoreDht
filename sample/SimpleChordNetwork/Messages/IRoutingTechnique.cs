namespace SimpleChordNetwork.Messages
{
    public interface IRoutingTechnique
    {
        RoutingTechnique Technique { get; set; }
    }

    public enum RoutingTechnique
    {
        Successor,
        Chord,
    }

}