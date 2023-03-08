using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace WOW.Threading
{
    public static class FireAndForget
    {
        public static void Forget(this Task task, Action<System.Exception> exception = null)
        {
            try
            {
                var _ = task;
            }
            catch(Exception e)
            {
                exception?.Invoke(e);
            }
        }
    }
}