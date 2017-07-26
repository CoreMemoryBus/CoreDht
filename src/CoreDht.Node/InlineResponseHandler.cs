using System;
using System.Collections.Generic;
using System.Linq;
using CoreDht.Node.Messages.Internal;
using CoreDht.Utils;
using CoreMemoryBus;
using CoreMemoryBus.Messages;

namespace CoreDht.Node
{
    public class InlineResponseHandler
        : AwaitHandler
        , IHandle<Message>
        , IHandle<RetryAction>
        , IHandle<CancelOperation>
        , IHandle<OperationComplete>
    {
        private Action _initAction;
        private readonly int _retryCount;
        private RetryOperation _retryFunction;
        private Func<CorrelationId, RetryOperation> _retryFactory = c => null;
        private Action _continuation;
        private CorrelationId _parentCorrelation;
        private readonly Dictionary<CorrelationId, IResponseAction> _responseActions = new Dictionary<CorrelationId, IResponseAction>();

        public InlineResponseHandler(IMessageBus messageBus, Action<string> logger, int retryCount = RetryCount.None)
            : base(messageBus, logger)
        {
            _retryCount = retryCount;
        }

        public InlineResponseHandler PerformAction(Action initAction)
        {
            _initAction = initAction;
            return this;
        }

        public InlineResponseHandler PerformRetryAction(Action retryAction)
        {
            _retryFactory = c => new RetryOperation(MessageBus, c, retryAction, _retryCount);
            return this;
        }

        public InlineResponseHandler AndAwait<TResponse>(CorrelationId correlationId, Action<TResponse> responseCallback)
            where TResponse : Message, ICorrelatedMessage<CorrelationId>
        {
            _responseActions[correlationId] = new ResponseAction<TResponse>(responseCallback);
            Logger?.Invoke($"Awaiting {typeof(TResponse).Name} Id:{correlationId}");

            return this;
        }

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

        public InlineResponseHandler ContinueWith(Action continuation)
        {
            _continuation = continuation;
            return this;
        }

        private RetryOperation RetryFunction => _retryFunction ?? (_retryFunction = _retryFactory.Invoke(_parentCorrelation));

        public void Run(CorrelationId parentCorrelation)
        {
            _parentCorrelation = parentCorrelation;
            MessageBus.Publish(new AwaitMessage(parentCorrelation));

            MessageBus.Subscribe(this);

            _initAction?.Invoke();
            RetryFunction?.Invoke();
        }

        public void Run(CorrelationId parentCorrelation, int timeoutMs)
        {
            _parentCorrelation = parentCorrelation;
            MessageBus.Publish(new AwaitWithTimeoutMessage(parentCorrelation) { Timeout = timeoutMs });

            MessageBus.Subscribe(this);

            _initAction?.Invoke();
            RetryFunction?.Invoke();
        }

        private static readonly Type[] IgnoreTypes = new Type[3]
            {
                typeof (CancelOperation),
                typeof (OperationComplete),
                typeof (RetryAction)
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

        public void Handle(RetryAction message)
        {
           RetryFunction?.Invoke();
        }

        public void Handle(CancelOperation message)
        {
            if (message.CorrelationId.Equals(_parentCorrelation))
            {
                MessageBus.Unsubscribe(this);

                var operationIds = from opIds in _responseActions.Keys select $"{opIds}";
                Logger?.Invoke($"Cancel Operations\n\tId:{string.Join("\n\tId:", operationIds)}");
            }
        }

        public void Handle(OperationComplete message)
        {
            if (message.CorrelationId.Equals(_parentCorrelation))
            {
                MessageBus.Unsubscribe(this);

                Logger?.Invoke($"Operation Complete Id:{message.CorrelationId}");
            }
        }
    }
}