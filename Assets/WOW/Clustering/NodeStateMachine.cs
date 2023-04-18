using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

namespace WOW.Clustering
{
    public partial class Node<RequestT, ResponseT>: MonoBehaviour
    {
        private class ProcessStateMachine
        {
            public enum ProcessState
            {
                None = 0,
                Processing = 1,
                Complete = 2,
            }

            public string RequestId { get => request.Id; }
            public TcpClient Client { get => this.client; }
            public RequestT Request { get => request.Body; }
            public ResponseT Response { get => response.Body; }
            public ProcessState State { get => state; }

            private TcpClient client = null;
            private MessageRequest<RequestT> request = null;
            private MessageResponse<ResponseT> response = null;
            private ProcessState state = ProcessState.None;
        
            public static ProcessStateMachine Create(TcpClient client, MessageRequest<RequestT> request)
            {
                return new ProcessStateMachine()
                {
                    client = client,
                    request = request,
                    response = null,
                    state = ProcessState.Processing
                };
            }

            public void Complete(MessageResponse<ResponseT> response)
            {
                this.response = response;
                this.state = ProcessState.Complete;
            }

            public void Fail()
            {
                this.response = null;
                this.state = ProcessState.None;
                this.client = null;
            }

            public void Retry(TcpClient client)
            {
                this.client = client;
            }
        }
    }
}