using System;
using NetMQ;
using NetMQ.Sockets;

namespace CoreDht.Node
{
    public class Node : IDisposable
    {
        protected Action<string> Logger { get; }
        protected DealerSocket ListeningSocket { get; }
        protected NetMQPoller Poller { get; set; }
        private PairSocket Shim { get; set; }

        protected Node(string binding, Action<string> logger)
        {
            Logger = logger;
            ListeningSocket = new DealerSocket(binding);
            Poller = new NetMQPoller();
            Actor = CreateActor();
        }

        public NetMQActor Actor { get; }

        private NetMQActor CreateActor()
        {
            return NetMQActor.Create(shim =>
            {
                Shim = shim;
                Shim.ReceiveReady += ShimOnReceiveReady;
                Shim.SignalOK();

                Poller.Add(Shim);
                Poller.Add(ListeningSocket);
                Poller.Run();

                Logger?.Invoke("Node closed");
            });
        }


        private void ShimOnReceiveReady(object sender, NetMQSocketEventArgs args)
        {
            var mqMsg = args.Socket.ReceiveMultipartMessage();
            var typeCode = mqMsg[0].ConvertToString();
            switch (typeCode)
            {
                case NetMQActor.EndShimMessage:
                    Poller.Stop();
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
                    if (Poller.IsRunning)
                    {
                        Poller.Stop();
                    }
                    Poller.Dispose();
                    Actor.Dispose();
                    ListeningSocket.Dispose();
                    // this wont scale!!
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