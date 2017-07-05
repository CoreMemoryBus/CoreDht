using CoreMemoryBus.Messages;
using Newtonsoft.Json;

namespace CoreDht.Utils
{
    /// <summary>
    /// The MessageSerializer converts messages into a data structure suitable for transport. In this case JSON.
    /// We'll pursue a more generic approach to accomodate other buffering technologies in a future release.
    /// </summary>
    public class MessageSerializer : IMessageSerializer
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public string Serialize(Message message)
        {
            return JsonConvert.SerializeObject(message, _settings);
        }

        public TMessage Deserialize<TMessage>(string json) where TMessage : Message
        {
            return JsonConvert.DeserializeObject<TMessage>(json, _settings);
        }
    }
}