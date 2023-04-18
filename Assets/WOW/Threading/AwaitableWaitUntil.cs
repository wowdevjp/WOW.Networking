using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

namespace WOW.Threading
{
    public struct AwaitableWaitUntil
    {
        private Awaiter awaiter;

        public AwaitableWaitUntil(Func<bool> predictor, CancellationToken cancellationToken)
        {
            this.awaiter = new Awaiter(predictor, cancellationToken);
        }

        public Awaiter GetAwaiter()
        {
            return awaiter;
        }

        public class Awaiter : INotifyCompletion
        {
            public bool IsCompleted
            {
                get {
                    return isCompleted;
                }
            }

            private bool isCompleted;
            private Func<bool> predictor;
            private readonly CancellationToken cancellationToken;
            private Action continuation;

            public Awaiter(Func<bool> predictor, CancellationToken cancellationToken)
            {
                this.predictor = predictor;
                this.cancellationToken = cancellationToken;
                this.isCompleted = false;
            }

            private void LoopOnThread()
            {
                while(true)
                {
                    try
                    {
                        isCompleted = predictor();
                        if (isCompleted)
                        {
                            break;
                        }
                    }
                    catch(Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                continuation?.Invoke();
            }

            private void LoopOnMainThread()
            {
                isCompleted = predictor();
                if(isCompleted)
                {
                    MainThreadRunner.RemoveLoopAction(LoopOnMainThread);
                    continuation?.Invoke();
                }
            }

            public void OnCompleted(Action continuation)
            {
                this.continuation = continuation;
                if(MainThreadRunner.IsMainThread)
                {
                    MainThreadRunner.AddLoopAction(LoopOnMainThread);
                }
                else
                {
                    LoopOnThread();
                }
            }

            public void GetResult()
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}