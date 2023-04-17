using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace WOW.Threading
{
    public class MiniTask
    {
        public static AwaitableThreadPool SwitchToThreadPool()
        {
            return new AwaitableThreadPool();
        }

        public static AwaitableMainThread SwitchToMainThread()
        {
            return new AwaitableMainThread();
        }

        public static AwaitableMainThread SwitchToMainThread(CancellationToken cancellationToken)
        {
            return new AwaitableMainThread(cancellationToken);
        }

        public static AwaitableWaitUntil WaitUntil(Func<bool> predictor, CancellationToken cancellationToken = default)
        {
            return new AwaitableWaitUntil(predictor, cancellationToken);
        }
    }
}