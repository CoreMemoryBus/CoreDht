using System;
using CoreDht.Utils.Hashing;
using CoreDht.Utils.Messages;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;
using CoreMemoryBus.Handlers;

namespace CoreDht.Utils
{
    public class RoutableRepository<TRepoItem> 
        : Repository<ConsistentHash, TRepoItem>
        , IHandle<Message> 
        where TRepoItem : IPublisher
    {
        protected RoutableRepository(Func<Message, TRepoItem> repoItemFactory = null) : base(repoItemFactory)
        { }

        public void Handle(Message msg)
        {
            var repoItemMessage = msg as IRoutableMessage;
            if (repoItemMessage != null)
            {
                var routingHash = repoItemMessage.RoutingTarget;
                TRepoItem repoItem;
                if (RepoItems.TryGetValue(routingHash, out repoItem))
                {
                    repoItem.Publish(msg);
                    return;
                }

                if (TriggerMessageTypes.Contains(msg.GetType()))
                {
                    var newRepoItem = RepoItemFactory(msg);
                    RepoItems[routingHash] = newRepoItem;
                    newRepoItem.Publish(msg);
                }
            }
        }
    }
}