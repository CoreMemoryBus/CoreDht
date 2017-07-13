using System;
using System.Collections.Generic;
using CoreDht.Utils;

namespace CoreDht.Node
{
    public class SocketCache : ObjectCache<string, OutgoingSocket>, ISocketCache, IDisposable
    {
        private readonly IUtcClock _clock;

        public SocketCache(INodeSocketFactory socketFactory, IUtcClock clock)
        {
            _clock = clock;
            Factory = key => new OutgoingSocket(key, socketFactory.CreateForwardingSocket(key), _clock);
        }

        public override void Remove(string key)
        {
            OutgoingSocket socket;
            if (Cache.TryGetValue(key, out socket))
            {
                socket.Dispose();
                base.Remove(key);
            }
        }

        public IEnumerable<string> PurgeFaultySockets()
        {
            var result = new List<string>();
            foreach (var wrapper in Cache)
            {
                if (wrapper.Value.Error)
                {
                    result.Add(wrapper.Key);
                }
            }
            result.ForEach(Remove);
            return result;
        }

        /// <summary>
        /// This function is added so local calls can be treated the same as foreign calls
        /// </summary>
        /// <param name="hostAndPort">local identity host and port</param>
        /// <param name="actor">The actor socket instance for the host</param>
        /// 
        public void AddActor(string hostAndPort, INodeActor actor)
        {
            this.Cache.Add(hostAndPort, new OutgoingSocket(actor, _clock));
        }

        #region IDisposable Support

        private bool _isDisposed = false;

        public void Dispose()
        {
            if (!_isDisposed)
            {
                foreach (var socket in Cache.Values)
                {
                    socket.Dispose();
                }

                _isDisposed = true;
            }
        }

        #endregion
    }
}