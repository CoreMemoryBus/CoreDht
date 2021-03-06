﻿using System;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

namespace CoreDht.Node
{
    /// <summary>
    /// This class provides basic common implementations. Really for framework authors, not application developers
    /// </summary>
    public class AwaitHandler
    {
        protected readonly IMessageBus MessageBus;
        protected readonly Action<string> Logger;

        protected AwaitHandler(IMessageBus messageBus, Action<string> logger)
        {
            MessageBus = messageBus;
            Logger = logger;
        }

        protected interface IResponseAction
        {
            bool TryExecuteAction(Message replyMessage);
        }

        protected class ResponseAction<TMessage> : IResponseAction where TMessage : Message
        {
            private readonly Action<TMessage> _action;

            public ResponseAction(Action<TMessage> action)
            {
                _action = action;
            }

            public bool TryExecuteAction(Message replyMessage)
            {
                var reply = replyMessage as TMessage;
                if (reply != null)
                {
                    _action(reply);
                    return true;
                }
                return false;
            }
        }
    }
}
