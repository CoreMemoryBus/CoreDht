using CoreMemoryBus.Messages;

namespace CoreDht.Utils
{
    public interface IMessageSerializer
    {
        string Serialize(Message message);
        TMessage Deserialize<TMessage>(string json) where TMessage : Message;
    }
}