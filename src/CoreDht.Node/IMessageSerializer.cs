using CoreMemoryBus.Messages;

namespace CoreDht.Node
{
    public interface IMessageSerializer
    {
        string Serialize(Message message);
        TMessage Deserialize<TMessage>(string json) where TMessage : Message;
    }
}