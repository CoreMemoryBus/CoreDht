﻿using System;
using CoreDht.Utils;
using NetMQ;
using NetMQ.Sockets;

namespace CoreDht.Node
{
    /// <summary>
    /// Encapsulate all the actor boilerplate into something re-usable.
    /// </summary>
    public class NodeActor : INodeActor, IDisposable
    {
        private readonly NetMQSocket _listeningSocket;
        private readonly Action<NetMQMessage> _mqMessageHandler;
        private readonly Action<NetMQSocket, Exception> _exceptionHandler;
        private PairSocket _shim;
        private NetMQPoller _poller;
        private NetMQActor _actor;
        private readonly int _socketMsgBatchSize;

        public const int DefaultMsgBatchSize = 1000;

        public NodeActor(NetMQSocket listeningSocket, Action<NetMQMessage> mqMessageHandler, Action<NetMQSocket, Exception> exceptionHandler = null, int socketMsgBatchSize = DefaultMsgBatchSize)
        {
            _listeningSocket = listeningSocket;
            _mqMessageHandler = mqMessageHandler;
            _socketMsgBatchSize = socketMsgBatchSize;
            _exceptionHandler = exceptionHandler;
        }

        public void Start()
        {
            _actor = NetMQActor.Create(shim =>
            {
                _shim = shim;
                _shim.ReceiveReady += ShimOnReceiveReady;
                using (new DisposableAction(
                    () => { _listeningSocket.ReceiveReady += ListeningSocketOnReceiveReady; },
                    () => { _listeningSocket.ReceiveReady -= ListeningSocketOnReceiveReady; }))
                {
                    _poller = new NetMQPoller { _listeningSocket, _shim }; // TODO: Share the poller across multiple nodes...

                    _shim.SignalOK();
                    _poller.Run(); // TODO: How do we do this when poller is shared
                }
            });
        }

        public virtual void Stop() { _poller.Stop(); }

        public bool IsRunning => _poller.IsRunning;

        private void ShimOnReceiveReady(object sender, NetMQSocketEventArgs e)
        {
            _mqMessageHandler(e.Socket.ReceiveMultipartMessage());
        }

        private void ListeningSocketOnReceiveReady(object sender, NetMQSocketEventArgs args)
        {
            try
            {
                var socket = args.Socket;
                for (int i = 0; i < _socketMsgBatchSize; ++i)
                {
                    NetMQMessage mqMsg = null;
                    if (socket.TryReceiveMultipartMessage(ref mqMsg))
                    {
                        _actor.SendMultipartMessage(mqMsg);
                    }
                }
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

        public bool TrySend(ref Msg msg, TimeSpan timeout, bool more)
        {
            return _actor.TrySend(ref msg, timeout, more);
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
                    DisposePoller();
                }

                _isDisposed = true;
            }
        }

        protected virtual void DisposePoller()
        {
            _poller?.Dispose();
        }

        public void Dispose()
        {
            if (IsRunning)
            {
                Stop();
            }
            Dispose(true);
        }

        #endregion
    }
}