using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WOW.Clustering
{
    public class MessageRequest<T> : IMessage, IDisposable where T : IMessage
    {
        public string Id { get => this.id; }
        public T Body { get => this.body; }

        private string id = string.Empty;
        private T body = default(T);

        public MessageRequest()
        {

        }

        public MessageRequest(T body)
        {
            id = Guid.NewGuid().ToString();
            this.body = body;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Serialize(ref id);
            body.Serialize(serializer);
        }

        public virtual void Dispose()
        {

        }
    }

    public enum MessageResult
    {
        None = 0,
        Processing = 1,
        Success = 2,
        Failed = 3,
    }

    public class MessageResponse<T> : IMessage, IDisposable where T : IMessage
    {
        public string Id { get => this.id; }
        public T Body { get => this.body; }
        public MessageResult Result { get => this.result; }

        private string id = string.Empty;
        private MessageResult result = MessageResult.None;
        private T body = default(T);

        public MessageResponse()
        {

        }

        public MessageResponse(T body)
        {
            this.id = Guid.NewGuid().ToString();
            this.body = body;
        }

        public void Serialize(ISerializer serializer)
        {
            serializer.Serialize(ref id);
            serializer.Serialize(ref result);
            body.Serialize(serializer);
        }

        public virtual void Dispose()
        {

        }

        public static MessageResponse<T> CreateResponse(string id, T body, MessageResult result)
        {
            return new MessageResponse<T>()
            {
                id = id,
                body = body,
                result = result
            };
        }
    }
}