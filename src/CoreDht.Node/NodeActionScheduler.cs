using System;
using CoreDht.Node.Messages.Internal;
using CoreDht.Utils;
using CoreMemoryBus;
using NetMQ;

namespace CoreDht.Node
{
    /// <summary>
    /// NodeActionScheduler ensures that timer events created by the ActionScheduler are enqueued with all other events via the actor
    /// so that even timer events occur in a single thread. As a consequence there is no contention on the scheduled action heap, so no locking 
    /// is required. The TimerFiredHandler is added straight to the MessageBus as part of the setup. This completes the firing and action loop
    /// to create single-threaded concurrency.
    /// </summary>
    public class NodeActionScheduler : ActionScheduler, IDisposable
    {
        private readonly ICommunicationManager _commMgr;

        public NodeActionScheduler(IUtcClock clock, IActionTimer timer, ICommunicationManager commMgr)
            : base(clock, timer)
        {
            _commMgr = commMgr;
        }

        protected override void OnTimerFired()
        {
            _commMgr.SendInternal(new TimerFired());
        }

        public IHandle<TimerFired> CreateTimerHandler()
        {
            return new TimerFiredHandler(this);
        }

        public class TimerFiredHandler 
            : IHandle<TimerFired>
        {
            private readonly IActionScheduler _scheduler;

            public TimerFiredHandler(IActionScheduler scheduler)
            {
                _scheduler = scheduler;
            }

            public void Handle(TimerFired message)
            {
                _scheduler.DoTimerFired();
            }
        }

        #region Locking strategy

        protected override ILockingStrategy NewLock()
        {
            return new NoLockingStrategy();
        }

        protected struct NoLockingStrategy : ILockingStrategy
        {
            public void Dispose()
            {
            }

            public void Lock()
            {
            }

            public void Unlock()
            {
            }
        }

        #endregion
    }
}