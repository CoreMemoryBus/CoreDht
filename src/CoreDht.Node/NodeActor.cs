using System;
using CoreDht.Utils;
using NetMQ;
using NetMQ.Sockets;

namespace CoreDht.Node
{
    /// <summary>
    /// Encapsulate all the actor boilerplate into something re-usable.
    /// </summary>
    public class NodeActor : IDisposable, IReceivingSocket
    {
        private readonly NetMQSocket _listeningSocket;
        private readonly Action<NetMQMessage> _mqMessageHandler;
        private readonly Action<NetMQSocket, Exception> _exceptionHandler;
        private PairSocket _shim;
        private NetMQPoller _poller;
        private readonly NetMQActor _actor;

        public NodeActor(NetMQSocket listeningSocket, Action<NetMQMessage> mqMessageHandler, Action<NetMQSocket, Exception> exceptionHandler = null)
        {
            _listeningSocket = listeningSocket;
            _mqMessageHandler = mqMessageHandler;
            _exceptionHandler = exceptionHandler;
            _actor = NetMQActor.Create(shim =>
            {
                _shim = shim;
                _shim.ReceiveReady += ShimOnReceiveReady;
                using (new DisposableAction(
                    () => { _listeningSocket.ReceiveReady += ListeningSocketOnReceiveReady; }, 
                    () => { _listeningSocket.ReceiveReady -= ListeningSocketOnReceiveReady; }))
                {
                    _poller = new NetMQPoller { _listeningSocket, _shim };

                    _shim.SignalOK();
                    _poller.Run();
                }
            });
        }

        public bool IsRunning => _poller.IsRunning;

        public void Stop() {  _poller.Stop(); }

        private void ShimOnReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            _mqMessageHandler(e.Socket.ReceiveMultipartMessage());
        }

        private void ListeningSocketOnReceiveReady(object sender, NetMQSocketEventArgs args)
        {
            try
            {
                _actor.SendMultipartMessage(args.Socket.ReceiveMultipartMessage());
            }
            catch (Exception e)
            {
                _exceptionHandler?.Invoke(args.Socket, e);
                if (_exceptionHandler == null)
                {
                    throw;
                }
            }
        }

        public bool TryReceive(ref Msg msg, TimeSpan timeout)
        {
            return _actor.TryReceive(ref msg, timeout);
        }

        #region IDisposable Support

        private bool _isDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _actor.Dispose();
                    _poller?.Dispose();
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