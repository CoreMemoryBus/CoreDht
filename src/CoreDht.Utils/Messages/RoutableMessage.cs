﻿using CoreDht.Utils.Hashing;
using CoreMemoryBus.Messages;

namespace CoreDht.Utils.Messages
{
    public class RoutableMessage : Message, IRoutableMessage
    {
        protected RoutableMessage(ConsistentHash routingTarget)
        {
            RoutingTarget = routingTarget;
        }

        public ConsistentHash RoutingTarget { get; set; }
    }
}