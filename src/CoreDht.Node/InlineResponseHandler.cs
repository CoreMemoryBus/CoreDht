using System;
using System.Collections.Generic;
using System.Linq;
using CoreDht.Node.Messages.Internal;
using CoreDht.Utils;
using CoreMemoryBus;
using CoreMemoryBus.Messages;
using CoreMemoryBus.Messaging;

namespace CoreDht.Node
{
    /// <summary>
    /// InlineReponseHandler orchestrates complex request, retry and response operations using a simple fluent syntax. The handler manages all intermediate state while waiting 
    /// for responses and will remove itself from the message bus when complete. Operations may cancel as a result of a timeout or may need to be re-attempted as the target cannot process
    /// a particular message at the nominated time. Check cases for broken connections as well as timeouts.
    /// </summary>
    public class InlineResponseHandler
        : AwaitHandler
        , IHandle<Message>
        , IHandle<CancelOperation>
        , IHandle<OperationComplete>
    {
        private Action _initAction;
        private Action _continuation;
        private Action _finalAction;
        private CorrelationId _parentCorrelation;
        private readonly Dictionary<CorrelationId, IResponseAction> _responseActions = new Dictionary<CorrelationId, IResponseAction>();

        public InlineResponseHandler(MemoryBus messageBus, Action<string> logger)
            : base(messageBus, logger)
        {}

        /// <summary>
        /// Perform an action (typically the sending of 1 or more messages) and then await a response.
        /// </summary>
        /// <param name="initAction"></param>
        /// <returns></returns>
        public InlineResponseHandler PerformAction(Action initAction)
        {
            _initAction = initAction;
            return this;
        }

        /// <summary>
        /// Await a single correlated response before continuing.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="correlationId"></param>
        /// <param name="responseCallback"></param>
        /// <returns></returns>
        public InlineResponseHandler AndAwait<TResponse>(CorrelationId correlationId, Action<TResponse> responseCallback)
            where TResponse : Message, ICorrelatedMessage<CorrelationId>
        {
            _responseActions[correlationId] = new ResponseAction<TResponse>(responseCallback);
            Logger?.Invoke($"Awaiting {typeof(TResponse).Name} Id:{correlationId}");

            return this;
        }


        /// <summary>
        /// Await a collection of responses (typically from a fan-out operation) before continuing. The collelationIds are always new "child" correlations.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="correlationIds"></param>
        /// <param name="responseCallback"></param>
        /// <returns></returns>
        public InlineResponseHandler AndAwaitAll<TResponse>(CorrelationId[] correlationIds, Action<TResponse> responseCallback)
            where TResponse : Message, ICorrelatedMessage<CorrelationId>
        {
            foreach (var correlationId in correlationIds)
            {
                _responseActions[correlationId] = new ResponseAction<TResponse>(responseCallback);
            }

            var ids = from id in correlationIds select $"{id}";
            Logger?.Invoke($"Awaiting All {typeof(TResponse).Name}\n\tId:{string.Join("\n\tId:", ids)}");

            return this;
        }

        /// <summary>
        /// Define a function to be called after the successful response invocation.
        /// </summary>
        /// <param name="continuation"></param>
        /// <returns></returns>
        public InlineResponseHandler ContinueWith(Action continuation)
        {
            _continuation = continuation;
            return this;
        }

        /// <summary>
        /// Define a function to be called during a timeout or a successful operation at the close of the handler.
        /// </summary>
        /// <param name="finalAction"></param>
        /// <returns></returns>
        public InlineResponseHandler AndFinally(Action finalAction)
        {
            _finalAction = finalAction;
            return this;
        }

        /// <summary>
        /// Run the initial action to kick off the request/reply sequence. A correlated AwaitMessage is sent to the message bus
        /// so remotely posted AckMessages may extend a standard operation timeout.
        /// </summary>
        /// <param name="parentCorrelation"></param>
        public void Run(CorrelationId parentCorrelation)
        {
            _parentCorrelation = parentCorrelation;
            MessageBus.Publish(new AwaitMessage(parentCorrelation));

            MessageBus.Subscribe(this);

            _initAction?.Invoke();
        }


        /// <summary>
        /// Run the initial action to kick off the request/reply sequence. A correlated AwaitMessage is sent to the message bus
        /// so remotely posted AckMessages may extend a user defined timeout.
        /// </summary>
        /// <param name="parentCorrelation"></param>
        public void Run(CorrelationId parentCorrelation, int timeoutMs)
        {
            _parentCorrelation = parentCorrelation;
            MessageBus.Publish(new AwaitWithTimeoutMessage(parentCorrelation) { Timeout = timeoutMs });

            MessageBus.Subscribe(this);

            _initAction?.Invoke();
        }

        private static readonly Type[] IgnoreTypes = 
            {
                typeof (CancelOperation),
                typeof (OperationComplete),
            };

        public void Handle(Message message)
        {
            if (!IgnoreTypes.Contains(message.GetType()))
            {
                var correlatedMessage = message as ICorrelatedMessage<CorrelationId>;
                if (correlatedMessage != null)
                {
                    IResponseAction response;
                    if (_responseActions.TryGetValue(correlatedMessage.CorrelationId, out response) && response.TryExecuteAction(message))
                    {
                        _responseActions.Remove(correlatedMessage.CorrelationId);
                        if (_responseActions.Count == 0)
                        {
                            _continuation?.Invoke();
                            MessageBus.Publish(new OperationComplete(_parentCorrelation));
                        }
                    }
                }
            }
        }

        public void Handle(CancelOperation message)
        {
            if (message.CorrelationId.Equals(_parentCorrelation))
            {
                MessageBus.Unsubscribe(this);

                var operationIds = from opIds in _responseActions.Keys select $"{opIds}";
                Logger?.Invoke($"Cancel Operations\n\tId:{string.Join("\n\tId:", operationIds)}");

                _finalAction?.Invoke();
            }
        }

        public void Handle(OperationComplete message)
        {
            if (message.CorrelationId.Equals(_parentCorrelation))
            {
                MessageBus.Unsubscribe(this);

                Logger?.Invoke($"Operation Complete Id:{message.CorrelationId}");

                _finalAction?.Invoke();
            }
        }
    }
}