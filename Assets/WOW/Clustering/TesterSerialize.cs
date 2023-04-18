using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using WOW.Clustering;
using WOW.Threading;

public struct TestMessage : IMessage
{
    public int Number;
    public double DoubleNumber;
    public bool IsBool;
    public string Message;

    public void Serialize(ISerializer serializer)
    {
        serializer.Serialize(ref Number);
        serializer.Serialize(ref DoubleNumber);
        serializer.Serialize(ref IsBool);
        serializer.Serialize(ref Message);
    }
}


public class TesterSerialize : Node<TestMessage, TestMessage>
{
    protected override async Task<TestMessage> ProcessRequest(TestMessage request)
    {
        await MiniTask.SwitchToMainThread();
        int randomNumber = Random.Range(100, 500);
        Debug.Log("Randoom: " + randomNumber);
        await MiniTask.SwitchToThreadPool();
        await Task.Delay(randomNumber);
        var newMessage = request;
        newMessage.Message = request.Message.Replace("a", "!");
        return newMessage;
    }
}

//public class TesterSerialize : MonoBehaviour
//{
//    public struct ContentMessage : IMessage
//    {
//        public string Message;
//        public byte[] Data;

//        public void Serialize(ISerializer serializer)
//        {
//            serializer.Serialize(ref Message);
//            serializer.Serialize(ref Data);
//        }
//    }

//    public struct TestMessage : IMessage
//    {
//        public int Number;
//        public double DoubleNumber;
//        public bool IsBool;
//        public string Message;
//        public byte[] Binary;
//        public ContentMessage Content;

//        public void Serialize(ISerializer serializer)
//        {
//            serializer.Serialize(ref Number);
//            serializer.Serialize(ref DoubleNumber);
//            serializer.Serialize(ref IsBool);
//            serializer.Serialize(ref Message);
//            serializer.Serialize(ref Binary);
//            Content.Serialize(serializer);
//        }
//    }

//    private void Start()
//    {
//        var testMessage = new TestMessage()
//        {
//            Number = 1,
//            DoubleNumber = 2,
//            IsBool = true,
//            Message = "Hello!!!!!!",
//            Binary = new byte[] { 1, 2, 3, 4, 5, 7, 8, 9, 10, 11 },
//            Content = new ContentMessage() { Message = "ahdkashdkajshdkajshdkajhds", Data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 6, 4, 1, 1, 5, 5, 7, 8, 9, 9, 65, 56, 1, 2, 2, 5, 4, 7, 8, 9, 8, 7, 5, 4, 2, 1, 2, 4, 5, 8, 7, 8, } }
//        };

//        var binarySerializer = new ValueToBinarySerializer();
//        testMessage.Serialize(binarySerializer);
//        var buffer = binarySerializer.GetBuffer();

//        var valueSerializer = Serializer.ValueSerializer.SetBuffer(buffer);
//        var newMessage = new TestMessage() { };
//        newMessage.Serialize(valueSerializer);

//        Debug.Assert(testMessage.Number == newMessage.Number);
//        Debug.Assert(testMessage.DoubleNumber == newMessage.DoubleNumber);
//        Debug.Assert(testMessage.IsBool == newMessage.IsBool);
//        Debug.Assert(testMessage.Message == newMessage.Message);
//        Debug.Assert(testMessage.Content.Message == newMessage.Content.Message);
//        Debug.Log(testMessage.Content.Message);
//        Debug.Log(newMessage.Content.Message);
//        Debug.Log(string.Join(", ", testMessage.Binary));
//        Debug.Log(string.Join(", ", testMessage.Content.Data));
//        Debug.Log(string.Join(", ", newMessage.Content.Data));
//        Debug.Log(string.Join(", ", newMessage.Binary));
//    }
//}
