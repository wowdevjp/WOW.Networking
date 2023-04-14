using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class TcpSenderTest : MonoBehaviour
{
    private TcpClient tcp = null;
    private NetworkStream stream = null;    
    // Start is called before the first frame update
    void Start()
    {
        tcp = new TcpClient();
        tcp.Connect("192.168.110.55", 3000);
        stream = tcp.GetStream();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.S))
        {
            //using (tcp = new TcpClient("192.168.110.55", 3000))
            {
                Debug.Log("Send!!");
                
                stream.Write(System.Text.Encoding.UTF8.GetBytes("hogehoge"));
            }
        }   
    }

    private void OnDestroy()
    {
        tcp.Dispose();
    }
}
