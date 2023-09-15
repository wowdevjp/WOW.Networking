using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WOW.Threading
{
    public struct AwaitableMainThread
    {
        private readonly CancellationToken cancellationToken;

        public AwaitableMainThread(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
        }

        public Awaiter GetAwaiter()
        {
            return new Awaiter();
        }

        public struct Awaiter : INotifyCompletion
        {
            private readonly CancellationToken cancellationToken;

            public Awaiter(CancellationToken cancellationToken)
            {
                this.cancellationToken = cancellationToken;
            }

            public bool IsCompleted
            {
                get
                {
                    return MainThreadRunner.IsMainThread;
                }
            }
            public void OnCompleted(Action continuation)
            {
                MainThreadRunner.AddAction(continuation);
            }
            public void GetResult()
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}