using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using RosSharp.RosBridgeClient.MessageTypes.Rosapi;
using Unity.VisualScripting.AssemblyQualifiedNameParser;
using Newtonsoft.Json;
using MQTTDataCompasXR;

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
        public string approvalCounterRequestTopic { get; set; }
        public string sendTrajectoryTopic { get; set; }
        public string approvalCounterResultTopic { get; set; }

        //Constructer for publishers that takes an input project name
        public Publishers(string projectName)
        {
            getTrajectoryRequestTopic = $"compas_xr/get_trajectory_request/{projectName}";
            approvalCounterRequestTopic = $"compas_xr/approval_counter_request/{projectName}"; //THIS is published to once the request trajectory button is pressed.
            approvalCounterResultTopic = $"compas_xr/approval_counter_result/{projectName}"; //This is published by everyone once a request message is received.
            approveTrajectoryTopic = $"compas_xr/approve_trajectory/{projectName}";
            sendTrajectoryTopic = $"compas_xr/send_trajectory/{projectName}";
        }

    }

    //Subscriber class for storing specific topics to subscribe to
    [System.Serializable]
    public class Subscribers
    {
        //Properties for subscriber topics
        public string getTrajectoryRequestTopic { get; set; } 
        public string getTrajectoryResultTopic { get; set; }
        public string approveTrajectoryTopic { get; set; }
        public string approvalCounterRequestTopic { get; set; }
        public string approvalCounterResultTopic { get; set; }

        //Constructer for subscribers that takes an input project name
        public Subscribers(string projectName)
        {
            getTrajectoryRequestTopic = $"compas_xr/get_trajectory_request/{projectName}"; //LISTENS TO GET APPLY REQUEST TRANSACTION LOCK WHEN THE REQUEST DOES NOT COME FROM ME.
            getTrajectoryResultTopic = $"compas_xr/get_trajectory_result/{projectName}";
            approveTrajectoryTopic = $"compas_xr/approve_trajectory/{projectName}";
            approvalCounterRequestTopic = $"compas_xr/approval_counter_request/{projectName}";
            approvalCounterResultTopic = $"compas_xr/approval_counter_result/{projectName}";
        }
    }

    /////////////////////////////////////////// Classes for Compas XR Services Management ///////////////////////////////////////////////
    
    //Service Manager Class for managing primary user and service control conditions.
    [System.Serializable]
    public class ServiceManager
    {
        //Counter to add to for the approval counter result message.
        public SimpleCounter UserCount { get; set; }

        //Counter to increment when approval result of ApproveTrajectoryMessage is true.
        public SimpleCounter ApprovalCount { get; set; }
        
        //Bool to control if I am the primary user in this transaction.
        public bool PrimaryUser { get; set; }

        //List to store current trajectory under review.
        public List<List<float>> CurrentTrajectory { get; set; } //TODO: CHECK TYPE OF TRAJECTORY INFORMATION FROM PLANNER

        //Current Service ennum
        public CurrentService currentService { get; set; }

        //Get Trajectory Request Transaction Lock
        public bool TrajectoryRequestTransactionLock { get; set; }

        //TODO: FIGURE OUT A WAY TO IGNORE UNEXPECTED REQUEST FROM PLANNER... THROW OUT MESSAGES THAT I AM NOT EXPECTING.
        //TODO: THIS SHOULD BE DONE THROUGH AN ISDIRTY BOOL... BASICALLY THE TIMEOUT INCLUDES FLIPPING THE IS DIRTY BOOL TO TRUE, AND THEN I IGNORE ANY RESPONSES WITH THAT SAME RESPONSEID.

        //TODO: ADD LAST REQUEST MESSAGE TO THIS CLASS TO CHECK IF THE MESSAGE I RECEIVED IS THE SAME AS THE ONE I SENT.
        
        //Constructer for ServiceManager
        public ServiceManager()
        {
            UserCount = new SimpleCounter();
            ApprovalCount = new SimpleCounter();
            PrimaryUser = false;
            CurrentTrajectory = null; //TODO: Maybe should be empty list? Would prevent weird scenario where it is null and I request .Count
            currentService = CurrentService.None;
        }

        //Enum for current service
        public enum CurrentService
        {
            None = 0,
            GetTrajectory = 1,
            ApproveTrajectory = 2,
            ExacuteTrajectory = 3,
        }
    }

    // Class for simple counter that can be used in the user counter and approval counter instances of the parent ServiceManager class
    [System.Serializable]
    public class SimpleCounter
    {
        private int _value;
        private readonly object _lock = new object();

        public SimpleCounter(int start = 0)
        {
            _value = start;
        }

        public int Value
        {
            get
            {
                lock (_lock)
                {
                    return _value;
                }
            }
        }

        public int Increment(int num = 1)
        {
            lock (_lock)
            {
                _value += num;
                return _value;
            }
        }

        public int Reset()
        {
            lock (_lock)
            {
                _value = 0;
                return _value;
            }
        }
    }
    /////////////////////////////////////////// Classes for Compas XR Custom Messages //////////////////////////////////////////////////
    
    //TODO: MAJOR TODO: CHECK IF THE HEADER PARSING PRODUCES INCORRECT HEADER INFORMATION ON SENDING OF NEW MESSAGES.
    //TODO: CHECK OVERALL STRUCTURE OF PASSING INFORMATION BACK AND FORTH BETWEEN GH AND UNITY. 
    //TODO: THERE IS A LOT OF CONDITIONAL INPUTS THAT ARE NOT IN THE PYTHON FILE, AND I AM NOT SURE IF THIS IS BECAUSE THE SPECIFICITY OF C# AND PARSING OR IF IT IS BECAUSE IMPORT WAS INCORRECT IN PYTHON FILE. 
    //TODO: ELEMENT ID SHOULD ACTUALLY BE STEP ID EVERYWHERE.
    //TODO: IMPLEMENT EXCEPTIONS FOR FETCHING DATA.
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

        public int UpdateFromMsg(int responseValue)
        {
            lock (_lock)
            {
                //If my response value is greater then my current value update my value
                if(responseValue > _value)
                {
                    _value = responseValue;
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
        
        public int Value
        {
            get
            {
                lock (_lock)
                {
                    return _value;
                }
            }
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

        public int UpdateFromMsg(int responseValue)
        {
            lock (_lock)
            {
                //If my response value is greater then my current value update my value
                if(responseValue > _value)
                {
                    _value = responseValue;
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
        
        // Accessible properties
        public int SequenceID { get; private set; }
        public int ResponseID { get; private set; }
        public string DeviceID { get; private set; }
        public string TimeStamp { get; private set; }

        //Constructer for header with optional inputs that are used for parsing information from a received message so it does not mess up internal logic of counters.
        public Header(bool incrementResponseID = false, int? sequenceID=null, int? responseID=null, string deviceID=null, string timeStamp=null)
        {   
            if(sequenceID.HasValue && responseID.HasValue && deviceID != null && timeStamp != null)
            {    
                SequenceID = sequenceID.Value;
                ResponseID = responseID.Value;
                DeviceID = deviceID;
                TimeStamp = timeStamp;
            }
            else
            {
                SequenceID = EnsureSequenceID();
                ResponseID = EnsureResponseID(incrementResponseID);
                DeviceID = GetDeviceID();
                TimeStamp = GetTimeStamp();
            }
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
        private static int EnsureResponseID(bool incrementResponseID = false)
        {
            if (_sharedResponseIDCounter == null)
            {
                _sharedResponseIDCounter = new ResponseID();
                int responseIDValue = _sharedResponseIDCounter.Value;
                return responseIDValue;
            }
            else
            {
                //Only increment if input says to... more specifically only when I am sending a GetTrajectoryRequest.
                if (incrementResponseID)
                {
                    _sharedResponseIDCounter.Increment();
                }
                
                int responseIDValue = _sharedResponseIDCounter.Value;
                
                return responseIDValue;
            }
        }

        //Method to update my SequenceCounterValue based on parsed information from a received message
        private static void _updateSharedSequenceIDCounter(int sequenceID)
        {
            //TODO: IF SEQUENCEID IS NULL THROW EXCEPTION.
            if (_sharedSequenceCounter != null)
            {
                _sharedSequenceCounter.UpdateFromMsg(sequenceID);
            }
            else
            {
                _sharedSequenceCounter = new SequenceCounter(sequenceID);
            }
        }

        //Method to update my ResponseIDValue based on parsed information from a received message
        private static void _updateSharedResponseIDCounter(int responseID)
        {
            //TODO: IF RESPONSEID IS NULL THROW EXCEPTION.
            if (_sharedResponseIDCounter != null)
            {
                _sharedResponseIDCounter.UpdateFromMsg(responseID);
            }
            else
            {
                _sharedResponseIDCounter = new ResponseID(responseID);
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



        // Method to parse an instance of the class from a json string
        public static Header Parse(string jsonString)
        {
            //Deserilize string into a dictionary string object
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);

            // Extract header data from the JSON object and cast to new required types.
            var sequenceID = Convert.ToInt32(jsonObject["sequence_id"]);
            var responseID = Convert.ToInt32(jsonObject["response_id"]);
            var deviceID = jsonObject["device_id"].ToString();
            var timeStamp = jsonObject["time_stamp"].ToString();

            //Update Header SequenceCounter based on received information
            _updateSharedSequenceIDCounter(sequenceID);

            //Update Header ResponseID Based on received information
            _updateSharedResponseIDCounter(responseID);

            // Create and return a new Header instance with inputs so the values are the same and do not mess up internal logic of counters.
            return new Header(false, sequenceID, responseID, deviceID, timeStamp);
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
        public GetTrajectoryRequest(string elementID, Header header=null)
        {
            //Only moment to increment the responseID is when I send a new trajectory request.
            Header = header ?? new Header(true);
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

        // Method to parse an instance of the class from a json string
        public static GetTrajectoryRequest Parse(string jsonString)
        {
            //Deserilize string into a dictionary string object
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            
            // Extract header data from the JSON object and parse into a new Header instance
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            // Extract additional data from the JSON object and cast to new required types.
            var elementID = jsonObject["element_id"].ToString();
            
            // Create and return a new GetTrajectoryResult instance
            return new GetTrajectoryRequest(elementID, header);
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
        public List<List<float>> Trajectory { get; private set; } //TODO: CHECK TYPE OF TRAJECTORY INFORMATION FROM PLANNER

        //TODO: ADD ROBOT LOCATION (BASEFRAME) TO THIS MESSAGE.

        // Constructor for creating a new GetTrajectoryResult Message instance
        public GetTrajectoryResult(string elementID, List<List<float>> trajectory, Header header=null) //TODO: CHECK TYPE OF TRAJECTORY INFORMATION FROM PLANNER
        {
            Header = header ?? new Header();
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

        // Method to parse an instance of the class from a json string
        public static GetTrajectoryResult Parse(string jsonString)
        {
            //Deserilize string into a dictionary string object
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            
            // Extract header data from the JSON object and parse into a new Header instance
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            // Extract additional data from the JSON object and cast to new required types.
            var elementID = jsonObject["element_id"].ToString();
            
            //throw exception if trajectory is null //TODO: THIS IS WHERE YOU INCLUDED THE EXCEPTION. HOWEVER MAYBE IT SHOULD BE IN THE TRAJECTORY MANAGER CLASS METHOD?
            if (jsonObject["trajectory"] == null)
            {
                throw new Exception("Trajectory is null");
            }

            var trajectory = JsonConvert.DeserializeObject<List<List<float>>>(jsonObject["trajectory"].ToString());

            // Create and return a new GetTrajectoryResult instance
            return new GetTrajectoryResult(elementID, trajectory, header);
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
        public List<List<float>> Trajectory { get; private set; }
        public int ApprovalStatus { get; private set; }

        // Constructor for creating a new ApproveTrajectory Message instance
        public ApproveTrajectory(string elementID, List<List<float>> trajectory, int approvalStatus, Header header=null)
        {
            Header = header ?? new Header();
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

        // Method to parse an instance of the class from a json string
        public static ApproveTrajectory Parse(string jsonString)
        {
            //Deserilize string into a dictionary string object
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            
            // Extract header data from the JSON object and parse into a new Header instance
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            // Extract additional data from the JSON object and cast to new required types.
            var elementID = jsonObject["element_id"].ToString();
            var approvalStatus = Convert.ToInt16(jsonObject["approval_status"]);
            var trajectory = JsonConvert.DeserializeObject<List<List<float>>>(jsonObject["trajectory"].ToString());
            
            // Create and return a new GetTrajectoryResult instance
            return new ApproveTrajectory(elementID, trajectory, approvalStatus, header);
        }
    }

    // Approval Counter Request Compas XR Message specific class: Expected message from devices to request a reply from all active devices to control the amount of approvals needed to proceed
    [System.Serializable]
    public class ApprovalCounterRequest
    {
        // Accessible properties
        public Header Header { get; private set; }
        public string ElementID { get; private set; }
        public string TrajectoryID { get; private set; }

        // Constructor for creating a new ApproveTrajectory Message instance
        public ApprovalCounterRequest(string elementID, Header header=null)
        {
            Header = header ?? new Header();
            ElementID = elementID;
            TrajectoryID = $"trajectory_id_{elementID}";
        }

        // Method to retrieve approval data as a dictionary
        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "element_id", ElementID },
                { "trajectory_id", TrajectoryID }
            };
        }
            
        // Method to parse an instance of the class from a json string
        public static ApprovalCounterRequest Parse(string jsonString)
        {
            //Deserilize string into a dictionary string object
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            
            // Extract header data from the JSON object and parse into a new Header instance
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            // Extract additional data from the JSON object and cast to new required types.
            var elementID = jsonObject["element_id"].ToString();
            
            // Create and return a new GetTrajectoryResult instance
            return new ApprovalCounterRequest(elementID, header);
        }
    }

    // Approval Counter Result Compas XR Message specific class: Expected message from devices to control the amount of approvals needed to proceed
    [System.Serializable]
    public class ApprovalCounterResult
    {
        // Accessible properties
        public Header Header { get; private set; }
        public string ElementID { get; private set; }
        public string TrajectoryID { get; private set; }

        // Constructor for creating a new ApprovalCounterResult Message instance
        public ApprovalCounterResult(string elementID, Header header=null)
        {
            Header = header ?? new Header();
            ElementID = elementID;
            TrajectoryID = $"trajectory_id_{elementID}";
        }

        // Method to retrieve counter response data as a dictionary
        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "header", Header.GetData() },
                { "element_id", ElementID },
                { "trajectory_id", TrajectoryID }
            };
        }

        // Method to parse an instance of the class from a json string
        public static ApprovalCounterResult Parse(string jsonString)
        {
            //Deserilize string into a dictionary string object
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            
            // Extract header data from the JSON object and parse into a new Header instance
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            // Extract additional data from the JSON object and cast to new required types.
            var elementID = jsonObject["element_id"].ToString();
            
            // Create and return a new GetTrajectoryResult instance
            return new ApprovalCounterResult(elementID, header);
        }
    }
    [System.Serializable]
    public class SendTrajectory
    {
        // Accessible properties
        public Header Header { get; private set; }
        public string ElementID { get; private set; }
        public string TrajectoryID { get; private set; }
        public List<List<float>> Trajectory { get; private set; } //TODO: CHECK TYPE OF TRAJECTORY INFORMATION FROM PLANNER

        // Constructor for creating a new SendTrajectory Message instance
        public SendTrajectory(string elementID, List<List<float>> trajectory, Header header=null) //TODO: CHECK TYPE OF TRAJECTORY INFORMATION FROM PLANNER
        {
            Header = header ?? new Header();
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

        // Method to parse an instance of the class from a json string
        public static SendTrajectory Parse(string jsonString)
        {
            //Deserilize string into a dictionary string object
            var jsonObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
            
            // Extract header data from the JSON object and parse into a new Header instance
            var headerInfo = JsonConvert.SerializeObject(jsonObject["header"]);
            Header header = Header.Parse(headerInfo);

            // Extract additional data from the JSON object and cast to new required types.
            var elementID = jsonObject["element_id"].ToString();
            var trajectory = JsonConvert.DeserializeObject<List<List<float>>>(jsonObject["trajectory"].ToString());

            // Create and return a new GetTrajectoryResult instance
            return new SendTrajectory(elementID, trajectory, header);
        }

    }
}


