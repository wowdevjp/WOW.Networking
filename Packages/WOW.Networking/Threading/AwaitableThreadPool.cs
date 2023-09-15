using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace WOW.Threading
{
    /// <summary>
    /// Switch continuation action to thread pool.
    /// </summary>
    public struct AwaitableThreadPool
    {
        public Awaiter GetAwaiter()
        {
            return new Awaiter();
        }

        public struct Awaiter : INotifyCompletion
        {
            private static WaitCallback callbackSwitch = new WaitCallback((state) =>
            {
                // Invoke continuation action on thread pool.
                var continuation = (Action)state;
                continuation();
            });

            public bool IsCompleted { get => false; }
            public void OnCompleted(Action continuation)
            {
                ThreadPool.QueueUserWorkItem(callbackSwitch, continuation);
            }

            public void GetResult()
            {

            }
        }
    }
}