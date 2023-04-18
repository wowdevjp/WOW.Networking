using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using UnityEngine;

public class TcpSenderTest : MonoBehaviour
{
    //private TcpClient tcp = null;
    //private NetworkStream stream = null;    
    //// Start is called before the first frame update
    //async void Start()
    //{
    //    tcp = new TcpClient();
    //    tcp.Connect("192.168.110.55", 3000);
    //    stream = tcp.GetStream();

    //    await stream.ReadAsync();
    //}

    //// Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetKeyUp(KeyCode.S))
    //    {
    //        //using (tcp = new TcpClient("192.168.110.55", 3000))
    //        {
    //            Debug.Log("Send!!");
                
    //            stream.Write(System.Text.Encoding.UTF8.GetBytes("hogehoge"));
    //        }
    //    }

    //    if (Input.GetKeyDown(KeyCode.D))
    //    {
    //        Debug.Log("Dispose");
    //        tcp.Client.Shutdown(SocketShutdown.Both);
    //        tcp?.Close();
    //        tcp?.Dispose();
    //        stream?.Close();
    //        stream?.Dispose();
    //    }
    //}

    //private void OnDestroy()
    //{
    //    tcp.Dispose();
    //}
}
