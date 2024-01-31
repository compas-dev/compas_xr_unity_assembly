using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using RosSharp.RosBridgeClient.MessageTypes.Rosapi;

namespace MQTTDataCompasXR
{
    /*
        WARNING: These classes define standard message formats for Compas XR MQTT communication.

        DISCLAIMER: The structure of these classes is tightly coupled with the Compas XR MQTT communication protocol.
        Any modifications to these classes must be synchronized with corresponding changes in the compas_xr/mqtt/messages.py file.
        GitHub Repository: https://github.com/gramaziokohler/compas_xr/blob/messages_module/src/compas_xr/mqtt/messages.py

        USAGE GUIDELINES:
        - These classes set the format for messages exchanged in Compas XR MQTT communication.
        - Avoid making changes to these classes without coordinating updates in the associated messages.py file.
        - Refer to the GitHub repository for the latest information and updates related to the Compas XR MQTT protocol.

        IMPORTANT: Modifications without proper coordination may lead to communication issues between components using this protocol.

        PLEASE NOTE: The information provided here serves as a guide to maintain compatibility and consistency with the Compas XR system.
        For detailed protocol specifications and discussions, refer to the GitHub repository mentioned above.

        CONTRIBUTORS: Ensure that any changes made to these classes are well-documented and discussed within the development team.
    */
   
    /////////////////////////////////////////// Classes for Topic Publishers and Subscribers ///////////////////////////////////////////
    
    //Topics Class for storing specific topics to publish on and subscribe to
    [System.Serializable]
    public class CompasXRTopics
    {
        public Publishers publishers { get; set; }
        public Subscribers subscribers { get; set; }

        //Constructer for topics that takes an input project name
        public CompasXRTopics(string projectName)
        {
            publishers = new Publishers(projectName);
            subscribers = new Subscribers(projectName);
        }
    }

    //Publishers class for storing specific topics to publish on
    [System.Serializable]
    public class Publishers
    {
        //Properties for publishers topics
        public string getTrajectoryRequestTopic { get; set; }
        public string approveTrajectoryTopic { get; set; }
        public string sendTrajectoryTopic { get; set; }

        //Constructer for publishers that takes an input project name
        public Publishers(string projectName)
        {
            getTrajectoryRequestTopic = $"compas_xr/get_trajectory_request/{projectName}";
            approveTrajectoryTopic = $"compas_xr/approve_trajectory/{projectName}";
            sendTrajectoryTopic = $"compas_xr/send_trajectory/{projectName}";
        }

    }

    //Subscriber class for storing specific topics to subscribe to
    [System.Serializable]
    public class Subscribers
    {
        //Properties for subscriber topics
        public string getTrajectoryResultTopic { get; set; }
        public string approveTrajectoryTopic { get; set; }

        //Constructer for subscribers that takes an input project name
        public Subscribers(string projectName)
        {
            getTrajectoryResultTopic = $"compas_xr/get_trajectory_result/{projectName}";
            approveTrajectoryTopic = $"compas_xr/approve_trajectory/{projectName}";
        }
    }

    /////////////////////////////////////////// Classes for Compas XR Custom Messages //////////////////////////////////////////////////
    
    
    [System.Serializable]
    public class SequenceCounter
    {
        private static readonly int ROLLOVER_THRESHOLD = 1000000;
        private int _value;
        private readonly object _lock = new object();

        public SequenceCounter(int start = 0)
        {
            _value = start;
        }

        public int Increment(int num = 1)
        {
            lock (_lock)
            {
                _value += num;
                if (_value > ROLLOVER_THRESHOLD)
                {
                    _value = 1;
                }
                return _value;
            }
        }
    }

    //Class for setting specific response ID's for each message
    [System.Serializable]
    public class ResponseID
    {
        //Response Attributes.
        private static readonly int ROLLOVER_THRESHOLD = 1000000;
        private int _value;
        private readonly object _lock = new object();

        public ResponseID(int start = 0)
        {
            _value = start;
        }

        public int Increment(int num = 1)
        {
            lock (_lock)
            {
                _value += num;
                if (_value > ROLLOVER_THRESHOLD)
                {
                    _value = 1;
                }
                return _value;
            }
        }

        public int Update(int value)
        {
            lock (_lock)
            {
                if (value == _value)
                {
                    Increment();
                }
                else
                {
                    _value = value;
                }
                return _value;
            }
        }
    }


    // Header Compas XR Message specific class : Expected message header for all messages

    [System.Serializable]
    public class Header
    {
        // Private Properties for each header to build upon itself
        private static SequenceCounter _sharedSequenceCounter;
        private static ResponseID _sharedResponseIDCounter;
        private static int _lastResponseID;
        
        // Accessible properties
        public int SequenceID { get; private set; }
        public int ResponseID { get; private set; }
        public string DeviceID { get; private set; }
        public string TimeStamp { get; private set; }

        //Constructor for creating a new Header Message instance
        public Header()
        {
            SequenceID = EnsureSequenceID();
            ResponseID = EnsureResponseID();
            DeviceID = GetDeviceID();
            TimeStamp = GetTimeStamp();
        }

        // Method to retrieve header data as a dictionary
        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "sequence_id", SequenceID },
                { "response_id", ResponseID },
                { "device_id", DeviceID },
                { "time_stamp", TimeStamp }
            };
        }

        // Method to increment sequenceID for each message
        private static int EnsureSequenceID()
        {
            if (_sharedSequenceCounter == null)
            {
                _sharedSequenceCounter = new SequenceCounter();
                return _sharedSequenceCounter.Increment();
            }
            else
            {
                return _sharedSequenceCounter.Increment();
            }
        }

        // Method to increment responseID when required for each message
        private static int EnsureResponseID()
        {
            if (_sharedResponseIDCounter == null)
            {
                _sharedResponseIDCounter = new ResponseID();
                _lastResponseID = _sharedResponseIDCounter.Increment();
                return _lastResponseID;
            }
            else
            {
                if (_lastResponseID != _sharedResponseIDCounter.Increment())
                {
                    _sharedResponseIDCounter.Increment();
                    _lastResponseID = _sharedResponseIDCounter.Increment();
                }
                else
                {
                    _lastResponseID = _sharedResponseIDCounter.Increment();
                }
                return _lastResponseID;
            }
        }

        //method to get device ID
        private static string GetDeviceID()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        //method to get time stamp
        private static string GetTimeStamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }

    // Get Trajectory Request Compas XR Message specific class : Expected message for trajectory request sent to planner
    [System.Serializable]
    public class GetTrajectoryRequest
    {
        // Accessible properties
        public Header Header { get; private set; }
        public string ElementID { get; private set; }
        public string TrajectoryID { get; private set; }

        // Constructor for creating a new GetTrajectoryRequest Message instance
        public GetTrajectoryRequest(string elementID)
        {
            Header = new Header();
            ElementID = elementID;
            TrajectoryID = $"trajectory_id_{elementID}";
        }

       // Method to retrieve request data as a dictionary
        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "element_id", ElementID },
                { "trajectory_id", TrajectoryID }
            };
        }
    }

    // Get Trajectory Result Compas XR Message specific class: Expected message trajectory reply from planner
    [System.Serializable]
    public class GetTrajectoryResult
    {
        // Accessible properties
        public Header Header { get; private set; }
        public string ElementID { get; private set; }
        public string TrajectoryID { get; private set; }
        public List<List<System.Double>> Trajectory { get; private set; }

        // Constructor for creating a new GetTrajectoryResult Message instance
        public GetTrajectoryResult(string elementID, List<List<System.Double>> trajectory)
        {
            Header = new Header();
            ElementID = elementID;
            TrajectoryID = $"trajectory_id_{elementID}";
            Trajectory = trajectory;
        }

       // Method to retrieve result data as a dictionary
        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "element_id", ElementID },
                { "trajectory_id", TrajectoryID },
                { "trajectory", Trajectory }
            };
        }
    }


    // Approve Trajectory Compas XR Message specific class: Expected message from devices for trajectory approval
    [System.Serializable]
    public class ApproveTrajectory
    {
        // Accessible properties
        public Header Header { get; private set; }
        public string ElementID { get; private set; }
        public string TrajectoryID { get; private set; }
        public object Trajectory { get; private set; }
        public object ApprovalStatus { get; private set; }

        // Constructor for creating a new ApproveTrajectory Message instance
        public ApproveTrajectory(string elementID, object trajectory, object approvalStatus)
        {
            Header = new Header();
            ElementID = elementID;
            TrajectoryID = $"trajectory_id_{elementID}";
            Trajectory = trajectory;
            ApprovalStatus = approvalStatus;
        }

       // Method to retrieve approval data as a dictionary
        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "element_id", ElementID },
                { "trajectory_id", TrajectoryID },
                { "trajectory", Trajectory },
                { "approval_status", ApprovalStatus }
            };
        }
    }

    [System.Serializable]
    public class SendTrajectory
    {
        // Accessible properties
        public Header Header { get; private set; }
        public string ElementID { get; private set; }
        public string TrajectoryID { get; private set; }
        public List<List<System.Double>> Trajectory { get; private set; }

        // Constructor for creating a new SendTrajectory Message instance
        public SendTrajectory(string elementID, List<List<System.Double>> trajectory)
        {
            Header = new Header();
            ElementID = elementID;
            TrajectoryID = $"trajectory_id_{elementID}";
            Trajectory = trajectory;
        }

       // Method to retrieve approval data as a dictionary
        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "element_id", ElementID },
                { "trajectory_id", TrajectoryID },
                { "trajectory", Trajectory }
            };
        }
    }
}
