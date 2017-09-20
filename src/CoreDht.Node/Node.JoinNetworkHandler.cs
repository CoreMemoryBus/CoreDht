using CoreDht.Node.Messages.Internal;
using CoreDht.Node.Messages.NetworkMaintenance;
using CoreDht.Utils;
using CoreMemoryBus;
using CoreMemoryBus.Handlers;

namespace CoreDht.Node
{
    partial class Node
    {
        public class JoinNetworkHandler
            : IHandle<BeginJoinNetwork>
            , IHandle<EndJoinNetwork>
            , IHandle<JoinNetwork>
        {
            private readonly NodeHandlerContext _cxt;
            private readonly ICommunicationManager _commMgr;

            public JoinNetworkHandler(NodeHandlerContext cxt)
            {
                _cxt = cxt;
                _commMgr = cxt.CommunicationManager;
            }

            public void Handle(BeginJoinNetwork message)
            {
                JoinToSeedNode(message.SeedNodeIdentity);
            }

            // Another node may beat us to join to the seed node, so we may need to join at another recommended location.
            private void JoinToSeedNode(NodeInfo seedNode)
            {
                var joinOpId = _cxt.CorrelationIdFactory.GetNextCorrelation();
                var joinReplyHandler = new InlineResponseHandler(_cxt.MessageBus, _cxt.Logger);
                joinReplyHandler
                    .PerformAction(() =>
                    {
                        RetryJoinToSeedNode(seedNode, joinOpId);
                    })
                    .AndAwait(joinOpId, (JoinNetworkReply reply) =>
                    {
                        // Copy the supplied successor table to this node
                    })
                    //.ContinueWith(() =>
                    //{
                    //    _commMgr.SendInternal(new InitStabilize());
                    //})
                    .Run(joinOpId);
            }

            private void RetryJoinToSeedNode(NodeInfo seedNode, CorrelationId retryCorrelation)
            {
                var config = _cxt.Configuration;
                // A Seed node may be busy accepting another node to join, so we may need to retry
                var retryOp = new RetryOperationHandler(_cxt.MessageBus, retryCorrelation, () =>
                {
                    // make sure the retry interval is also not self-similar
                    var expiryTime = _cxt.ExpiryTimeCalculator.CalcRandomExpiry(config.JoinWaitMin, config.JoinWaitVariation);
                    _cxt.Scheduler.ScheduleAction(expiryTime, null, state =>
                    {
                        var joinCorrelation = _cxt.CorrelationIdFactory.GetNextCorrelation();
                        var joinMsg = new JoinNetwork(_cxt.Identity, seedNode, joinCorrelation);
                        _commMgr.Send(joinMsg);
                    });
                }, RetryCount.Infinite);

                _cxt.MessageBus.Subscribe(retryOp);

                // Finally, try the first invocation
                retryOp.Invoke();
            }

            public void Handle(EndJoinNetwork message)
            {
                // Set to appropriate state. If node is in ideal state, accept messages else reject
            }

            public void Handle(JoinNetwork message)
            {
                // TODO: If the node is busy accepting another node in a join operation we should reply with a NackMessage.

            }
        }
    }
}
