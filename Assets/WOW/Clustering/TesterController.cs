using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WOW.Threading;

public class TesterController : MonoBehaviour
{
    [SerializeField]
    private TesterSerialize test = null;

    private int count = 0;

    private void Start()
    {
        TestLoop().Forget(Debug.LogException);
    }

    private async Task TestLoop()
    {
        Debug.Log("Start TestLoop");
        while (true)
        {
            try
            {
                await MiniTask.SwitchToThreadPool();
                await Task.Delay(1);
                var response = await test.GetResponseAsync();
                await MiniTask.SwitchToMainThread();
                Debug.Log(response.Message);
            }
            catch(System.Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    private void Update()
    {
        try
        {
            //Debug.Log("TryEnqueue: " + count);
            test.EnqueueRequest(new TestMessage()
            {
                Number = count,
                DoubleNumber = count,
                IsBool = true,
                Message = $"aaaaaaaaaaaaaaaaaaaaaaa{count}"
            });
            count++;
        }
        catch(System.Exception e)
        {
            Debug.LogWarning(e.Message);
            //Debug.LogException(e);
        }
    }
}
