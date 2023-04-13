using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace WOW.Threading
{
    public static class FireAndForget
    {
        public static async void ForgetWithExceptionAsync(this Task task, Action<System.Exception> exception = null)
        {
            try
            {
                await task;
            }
            catch(Exception e)
            {
                exception?.Invoke(e);
            }
        }

        public static void Forget(this Task task, Action<System.Exception> exception = null)
        {
            task.ForgetWithExceptionAsync(exception);
        }
    }
}