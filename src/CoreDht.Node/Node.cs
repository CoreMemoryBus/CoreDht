using System;
using NetMQ;
using NetMQ.Sockets;

namespace CoreDht.Node
{
    public class Node : IDisposable
    {
        protected Action<string> Logger { get; }
        protected DealerSocket ListeningSocket { get; }
        protected NodeActor Actor { get; }

        protected Node(string binding, Action<string> logger)
        {
            Logger = logger;
            ListeningSocket = new DealerSocket(binding);
            Actor = new NodeActor(ListeningSocket, OnReceiveMsg, (socket, ex) => {});
        }

        void OnReceiveMsg(NetMQMessage msg)
        {
            var typeCode = msg[0].ConvertToString();
            switch (typeCode)
            {
                case NetMQActor.EndShimMessage:
                    Actor.Stop();
                    Logger?.Invoke($"Node terminating.");
                    break;
            }
        }

        #region IDisposable Support

        private bool _isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (Actor.IsRunning)
                    {
                        Actor.Stop();
                    }
                    Actor.Dispose();
                    ListeningSocket.Dispose();
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}