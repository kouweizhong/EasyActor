﻿using System;
using System.Diagnostics;
using System.Threading;

//Inspired by and adapted from http://www.1024cores.net/
//Ref: http://www.1024cores.net/home/lock-free-algorithms/eventcounts

namespace Concurrent.Signals 
{
    internal class SimplifiedEventCount: IDisposable
    {
        private readonly Semaphore _Semaphore = new Semaphore(0, 2);
        private readonly object _Lock = new object();
        private volatile uint _Epoch;
        private uint _EpochStarted;
        private bool _Spurious;
        private bool _InWait;

        public void PrepareWait() 
        {
            Thread.MemoryBarrier();
            // this is good place to pump previous spurious wakeup
            if (_Spurious) 
            {
                _Spurious = false;
                var res = _Semaphore.WaitOne(0);
                Debug.Assert(res == true);
            }
            _InWait = true;
            _EpochStarted = _Epoch;
            Thread.MemoryBarrier();
        }

        public bool Wait(CancellationToken cancellationToken) 
        {
            if (_EpochStarted == _Epoch)
                return _Semaphore.WaitOne(cancellationToken);
            
            RetireWait();
            return true;
        }

        public void RetireWait() 
        {
            _Spurious = true;
            // try to remove node from waitset
            if (!_InWait)
                return;

            lock (_Lock)
            {
                if (_InWait) 
                {
                    // successfully removed from waitset,
                    // so there will be no spurious wakeup
                    _InWait = false;
                    _Spurious = false;
                }
            }
        }

        public void NotifyOne() 
        {
            Thread.MemoryBarrier();
            if (!_InWait)
            {
                Thread.MemoryBarrier();
                return;
            }

            lock (_Lock) 
            {
                _Epoch += 1;
                _InWait = false;
            }

            _Semaphore.Release();
            Thread.MemoryBarrier();
        }

        public void Dispose()
        {
            _Semaphore.Dispose();
        }
    }
}
