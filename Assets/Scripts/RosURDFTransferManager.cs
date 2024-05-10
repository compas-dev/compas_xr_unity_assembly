// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.Threading;
// using System;
// using System.IO;
// using RosSharp.RosBridgeClient.UrdfTransfer;
// using RosSharp.RosBridgeClient;
// using RosSharp.RosBridgeClient.Protocols;
// using UnityEditor;

// // namespace RosSharp.RosBridgeClient
// // {
// public class URDFTransferManager : MonoBehaviour
// {
//     public class RuntimeTransferFromRosHandler
//     {
//         private string robotName;
//         private string localDirectory;
//         private int timeout;
//         private string assetPath;
//         private string urdfParameter;

//         private RosSocket rosSocket;

//         public Dictionary<string, ManualResetEvent> StatusEvents;

//         public RuntimeTransferFromRosHandler()
//         {
//             StatusEvents = new Dictionary<string, ManualResetEvent>{
//                 { "connected", new ManualResetEvent(false) },
//                 { "robotNameReceived",new ManualResetEvent(false) },
//                 { "robotDescriptionReceived", new ManualResetEvent(false) },
//                 { "resourceFilesReceived", new ManualResetEvent(false) },
//                 { "disconnected", new ManualResetEvent(false) },
//                 { "importComplete", new ManualResetEvent(false) }
//                 };
//         }

//         public void TransferUrdf(Protocol protocolType, string serverUrl, int timeout, string assetPath, string urdfParameter, RosSocket.SerializerEnum serializer)
//         {
//             this.timeout = timeout;
//             this.assetPath = assetPath;
//             this.urdfParameter = urdfParameter;

//             // initialize
//             ResetStatusEvents();

//             rosSocket = RosConnector.ConnectToRos(protocolType, serverUrl, OnConnected, OnClosed, serializer);

//             if (!StatusEvents["connected"].WaitOne(timeout * 1000))
//             {
//                 Debug.LogWarning("Failed to connect to ROS before timeout");
//                 return;
//             }

//             ImportAssets();
//         }

//         private void ImportAssets()
//         {
//             // setup Urdf Transfer
//             UrdfTransferFromRos urdfTransfer = new UrdfTransferFromRos(rosSocket, assetPath, urdfParameter);
//             StatusEvents["robotNameReceived"] = urdfTransfer.Status["robotNameReceived"];
//             StatusEvents["robotDescriptionReceived"] = urdfTransfer.Status["robotDescriptionReceived"];
//             StatusEvents["resourceFilesReceived"] = urdfTransfer.Status["resourceFilesReceived"];

//             urdfTransfer.Transfer();

//             if (StatusEvents["robotNameReceived"].WaitOne(timeout * 1000))
//             {
//                 robotName = urdfTransfer.RobotName;
//                 localDirectory = urdfTransfer.LocalUrdfDirectory;
//             }

//             // import URDF assets:
//             if (StatusEvents["resourceFilesReceived"].WaitOne(timeout * 1000))
//                 Debug.Log("Imported urdf resources to " + localDirectory);
//             else
//                 Debug.LogWarning("Not all resource files have been received before timeout.");

//             rosSocket.Close();
//         }

//         public void GenerateModelIfReady()
//         {
//             if (!StatusEvents["resourceFilesReceived"].WaitOne(0) || StatusEvents["importComplete"].WaitOne(0))
//                 return;

//             AssetDatabase.Refresh();

//             if (EditorUtility.DisplayDialog(
//                 "Urdf Assets imported.",
//                 "Do you want to generate a " + robotName + " GameObject now?",
//                 "Yes", "No"))
//             {
//                 UrdfRobotExtensions.Create(Path.Combine(
//                     localDirectory,
//                     Path.GetFileNameWithoutExtension(urdfParameter) + ".urdf"));
//             }

//             StatusEvents["importComplete"].Set();
//         }

//         private void OnClosed(object sender, EventArgs e)
//         {
//             StatusEvents["disconnected"].Set();
//         }

//         private void OnConnected(object sender, EventArgs e)
//         {
//             StatusEvents["connected"].Set();
//         }

//         private void ResetStatusEvents()
//         {
//             foreach (var manualResetEvent in StatusEvents.Values)
//                 manualResetEvent.Reset();
//         }
//     }
// }
// // }
