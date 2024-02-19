using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Std;
using UnityEngine;

public class RosMessageEvent : UnitySubscriber<RosSharp.RosBridgeClient.MessageTypes.Std.String>
{
    public GameObject greenPanel;
    private bool messageReceived = false; // Flag to indicate message receipt

    protected override void Start()
    {
        base.Start();
    }

    protected override void ReceiveMessage(RosSharp.RosBridgeClient.MessageTypes.Std.String message)
    {
        // Log the received message data
        Debug.Log(message.data);
        // Turn on the panel
        messageReceived = true; // Set the flag when a message is received
        
    }

    private void Update()
    {
        if (messageReceived)
        {
            greenPanel.SetActive(true);
        }
    }
}
