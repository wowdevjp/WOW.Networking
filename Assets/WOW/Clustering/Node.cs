using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using WOW.Threading;

namespace WOW.Clustering
{
    public class Node<T> : MonoBehaviour where T : IMessage
    {
        [SerializeField]
        private NodeType nodeType = NodeType.None;
        [SerializeField]
        private int bufferSize = 1024 * 1024 * 1024;

        [Header("Network")]
        [SerializeField]
        private string ipAddress = null;
        [SerializeField]
        private int portNumber = 3000;

        private TcpListener tcpListener = null;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private HashSet<TcpClient> clients = new HashSet<TcpClient>();

        private static async Task<byte[]> ReadBytesAsync(TcpClient client, int bufferSize)
        {
            byte[] tempBuffer = new byte[bufferSize];

            var stream = client.GetStream();
            int receivedSize = await stream.ReadAsync(tempBuffer, 0, tempBuffer.Length);

            byte[] buffer = new byte[receivedSize];
            Array.Copy(tempBuffer, buffer, receivedSize);

            return buffer;
        }

        protected virtual async Task LoopServeTcpClient()
        {
            tcpListener = new TcpListener(System.Net.IPAddress.Parse(ipAddress), portNumber);
            tcpListener.Start();
            Debug.Log("Start");

            await MiniTask.SwitchToThreadPool();

            while (true)
            {
                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    var tcpClient = await tcpListener.AcceptTcpClientAsync().ToCancellableTask(cancellationTokenSource.Token);

                    clients.Add(tcpClient);
                    LoopClientCommunication(tcpClient).Forget(Debug.LogException);
                }
                catch(TaskCanceledException)
                {
                    Debug.Log("[Node] LoopServeTcpClient is canceled.");
                    break;
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }

            tcpListener.Stop();
            tcpListener = null;
        }

        protected virtual async Task LoopClientCommunication(TcpClient client)
        {
            while(client.Connected)
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();

                var bytes = await ReadBytesAsync(client, bufferSize);
                Debug.Log(bytes.Length);
            }

            clients.Remove(client);
            client.Dispose();
        }

        protected void Awake()
        {
            LoopServeTcpClient().Forget(Debug.LogException);
        }

        private void OnDestroy()
        {
            if(tcpListener != null)
            {
                cancellationTokenSource.Cancel();
                tcpListener?.Stop();
            }

            foreach(var client in clients)
            {
                client?.Dispose();
            }
        }
    }
}