using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace WOW.Threading
{
    /// <summary>
    /// Manage queues to switch to the main thread.
    /// </summary>
    public class MainThreadRunner : MonoBehaviour
    {
        /// <summary>
        /// Is running on the main thread?
        /// </summary>
        public static bool IsMainThread { get => mainThreadId == Thread.CurrentThread.ManagedThreadId; }

        private static MainThreadRunner instance = null;
        private static int mainThreadId = 0;
        private static Queue<Action> actionQueue = new Queue<Action>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeOnLoad()
        {
            if (instance == null)
            {
                mainThreadId = Thread.CurrentThread.ManagedThreadId;
                GameObject obj = new GameObject("MainThreadRunner");
                instance = obj.AddComponent<MainThreadRunner>();
            }
        }

        /// <summary>
        /// Add main thread action.
        /// </summary>
        /// <param name="action"></param>
        public static void AddAction(Action action)
        {
            actionQueue.Enqueue(action);
        }

        private void Awake()
        {
            if(instance != null)
            {
                Destroy(this.gameObject);
                return;
            }
        }

        private void Update()
        {
            while(actionQueue.Count > 0)
            {
                Action action = actionQueue.Dequeue();
                action?.Invoke();
            }
        }
    }
}