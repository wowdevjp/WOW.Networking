using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;

using Task = System.Threading.Tasks.Task;

namespace WOW.Threading
{
    public static class Cancellable
    {
        public static Task<T> ToCancellableTask<T>(this Task<T> task, CancellationToken token)
        {
            if (!token.CanBeCanceled)
            {
                return task;
            }

            var taskCompletionSource = new TaskCompletionSource<T>();
            token.Register(() => taskCompletionSource.TrySetCanceled(token), false);

            task.ContinueWith(t =>
            {
                if (task.IsCanceled)
                {
                    taskCompletionSource.TrySetCanceled();
                }
                else if (task.IsFaulted)
                {
                    taskCompletionSource.TrySetException(t.Exception);
                }
                else
                {
                    taskCompletionSource.TrySetResult(t.Result);
                }
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);


            return taskCompletionSource.Task;
        }
    }
}