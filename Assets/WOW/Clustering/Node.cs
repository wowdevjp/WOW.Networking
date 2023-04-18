using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Video;
using WOW.Threading;

namespace WOW.Clustering
{
    public abstract partial class Node<RequestT, ResponseT> : MonoBehaviour
        where RequestT : IMessage
        where ResponseT : IMessage
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

        [SerializeField]
        private int maxRequestQueue = 9;

        private TcpListener tcpListener = null;
        private TcpClient tcpClient = null;

        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        // 送信結果待ち
        private Queue<ProcessStateMachine> waitQueue = new Queue<ProcessStateMachine>();
        private HashSet<TcpClient> clients = new HashSet<TcpClient>();

        /// <summary>
        /// Read bytes from TCPClient.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        protected static async Task<byte[]> ReadBytesAsync(TcpClient client, int bufferSize)
        {
            byte[] tempBuffer = new byte[bufferSize];

            var stream = client.GetStream();
            int receivedSize = await stream.ReadAsync(tempBuffer, 0, tempBuffer.Length);

            byte[] buffer = new byte[receivedSize];
            Array.Copy(tempBuffer, buffer, receivedSize);
            
            // release
            tempBuffer = null;

            return buffer;
        }

        protected static byte[] SerializeMessageRequest(MessageRequest<RequestT> messageRequest)
        {
            var serializer = new ValueToBinarySerializer();
            messageRequest.Serialize(serializer);
            return serializer.GetBuffer();
        }

        protected static byte[] SerializeMessageResponse(MessageResponse<ResponseT> messageResponse)
        {
            var serializer = new ValueToBinarySerializer();
            messageResponse.Serialize(serializer);
            return serializer.GetBuffer();
        }

        protected static MessageResponse<ResponseT> DeserializeMessageResponse(byte[] bytes)
        {
            var deserializer = new BinaryToValueSerializer();
            deserializer.SetBuffer(bytes);
            var message = new MessageResponse<ResponseT>();
            message.Serialize(deserializer);
            return message;
        }

        protected static MessageRequest<RequestT> DeserializeMessageRequest(byte[] bytes)
        {
            var deserializer = new BinaryToValueSerializer();
            deserializer.SetBuffer(bytes);
            var message = new MessageRequest<RequestT>();
            message.Serialize(deserializer);
            return message;
        }

        /// <summary>
        /// クライアントの待ち受け
        /// </summary>
        /// <returns></returns>
        protected async Task LoopServer()
        {
            tcpListener = new TcpListener(System.Net.IPAddress.Parse(ipAddress), portNumber);
            tcpListener.Start();

            await MiniTask.SwitchToThreadPool();

            while (true)
            {
                try
                {
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    var tcpClient = await tcpListener.AcceptTcpClientAsync().ToCancellableTask(cancellationTokenSource.Token);

                    clients.Add(tcpClient);
                    ReceiveResponse(tcpClient).Forget(Debug.LogException);
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

        /// <summary>
        /// クライアントからの処理結果を受け取る
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected virtual async Task ReceiveResponse(TcpClient client)
        {
            Debug.Log("Connected!!!");

            while(client.Connected)
            {
                cancellationTokenSource.Token.ThrowIfCancellationRequested();
                var bytes = await ReadBytesAsync(client, bufferSize);
                
                if(bytes.Length < 1)
                {
#if LOG_VERBOSE_NETWORKING
                    Debug.Log($"[Node] Client diconnected.");
#endif
                    break;
                }

                try
                {
                    var responseMessage = DeserializeMessageResponse(bytes);
                    var stateMachine = waitQueue.Where(q => q.RequestId == responseMessage.Id).FirstOrDefault();

                    if(stateMachine == null)
                    {
                        throw new Exception("[Node] StateMachine does not exist.");
                    }

                    switch (responseMessage.Result)
                    {
                        case MessageResult.Success:
                            Debug.Log("Success!!!!");
                            stateMachine.Complete(responseMessage);
                            break;
                        case MessageResult.Failed:
                            stateMachine.Fail();
                            var newClient = GetVacantClient();
                            var requestMessage = new MessageRequest<RequestT>(stateMachine.Request);
                            var messageBytes = SerializeMessageRequest(requestMessage);
                            SendRequest(newClient, messageBytes);
                            break;
                    }

                    
                }
                catch(Exception e)
                {
                    Debug.LogException(e);
                }
            }

            clients.Remove(client);
            client.Dispose();
        }

        protected virtual void SendRequest(TcpClient client, byte[] data)
        {
            Debug.Log("Send Request");
            client.GetStream().Write(data);
        }

        private TcpClient GetVacantClient()
        {
            var processings = waitQueue.Select(q => q.Client).ToList();
            var vacants = clients.Where(c => processings.Contains(c) == false).ToList();
            
            if(vacants.Count > 0)
            {
                return vacants.First();
            }
            else
            {
                throw new Exception("[Node] Vacant Client does not exist.");
            }
        }

        /// <summary>
        /// リクエストをキューする
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="Exception"></exception>
        public void EnqueueRequest(RequestT request)
        {
            if(maxRequestQueue > waitQueue.Count)
            {
                var requestMessage = new MessageRequest<RequestT>(request);
                var bytes = SerializeMessageRequest(requestMessage);


                var targetClient = GetVacantClient();
                SendRequest(targetClient, bytes);
                var stateMachine = ProcessStateMachine.Create(targetClient, requestMessage);
                waitQueue.Enqueue(stateMachine);
            }
            else
            {
                throw new Exception($"[Node] Queue Capacity <= {maxRequestQueue}");
            }
        }

        public async Task<ResponseT> GetResponseAsync()
        {
            Debug.Log("GetResponseAsync");

            await MiniTask.WaitUntil(() => {
                Debug.Log(waitQueue.Count);
                return waitQueue.Count > 0;
            });
            var peek = waitQueue.Peek();

            await MiniTask.WaitUntil(() =>
            {
                Debug.Log($"{peek.State}, {peek.RequestId}");
                return peek.State == ProcessStateMachine.ProcessState.Complete;
            });

            var value = waitQueue.Dequeue();
            Debug.Log(waitQueue.Count);
            return value.Response;
        }

        protected virtual async Task ReceiveRequest(TcpClient client)
        {
            await MiniTask.SwitchToThreadPool();

            while (client.Connected)
            {
                Debug.Log("wait!!");

                cancellationTokenSource.Token.ThrowIfCancellationRequested();
                var bytes = await ReadBytesAsync(client, bufferSize);

                if (bytes.Length < 1)
                {
#if LOG_VERBOSE_NETWORKING
                    Debug.Log($"[Node] Client diconnected.");
#endif
                    break;
                }

                Debug.Log("Request Received");

                try
                {
                    var requestMessage = DeserializeMessageRequest(bytes);
                    var response = await ProcessRequest(requestMessage.Body);

                    var responseMessage = MessageResponse<ResponseT>.CreateResponse(requestMessage.Id, response, MessageResult.Success);

                    var responseBytes = SerializeMessageResponse(responseMessage);

                    client.GetStream().Write(responseBytes);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            clients.Remove(client);
            client.Dispose();
        }

        protected abstract Task<ResponseT> ProcessRequest(RequestT request);

        protected void Awake()
        {
            switch (nodeType)
            {
                case NodeType.Primary:
                    LoopServer().Forget(Debug.LogException);
                    break;
                case NodeType.Cluster:
                    tcpClient = new TcpClient(ipAddress, portNumber);
                    ReceiveRequest(tcpClient).Forget(Debug.LogException);
                    break;
            }
        }

        private void Update()
        {

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