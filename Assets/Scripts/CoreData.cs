using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CompasXR.Core.Data
{   
   ///////////// Class for Handeling Data conversion Inconsistencies /////////////// 

    [System.Serializable]
    public static class DataConverters
    {
        public static float[] ConvertDatatoFloatArray(object data)
        {
            if (data is List<object>)
            {
                List<object> dataList = data as List<object>;
                return dataList.Select(Convert.ToSingle).ToArray();
            }
            else if (data is float[])
            {
                return (float[])data;
            }
            else if (data is List<System.Double>)
            {
                List<System.Double> doubleList = data as List<System.Double>;
                return doubleList.Select(Convert.ToSingle).ToArray();
            }
            else if (data is System.Single[])
            {
                return new float[] { (float)data };
            }
            else if (data is List<System.Single>)
            {
                List<System.Single> singleList = data as List<System.Single>;
                return singleList.Select(Convert.ToSingle).ToArray();
            }
            else if (data is System.Double[])
            {
                System.Double[] doubleArray = data as System.Double[];
                return doubleArray.Select(Convert.ToSingle).ToArray();
            }
            else if (data is JArray)
            {
                JArray dataArray = data as JArray;
                return dataArray.Select(token => (float)token).ToArray();
            }
            else
            {
                Debug.LogError("DataParser: Data is not a List<Object>, List<System.Double>, System.Double Array, System.Single Array, List<System.Single>,  float Array, or JArray.");
                return null;
            }
        }
    } 
    

   /////////////Classes for Assembly Desearialization./////////////// 
    [System.Serializable]
    public class Node
    {
        public Part part { get; set; }
        public string type_data { get; set; }
        public string type_id { get; set; }
        public Attributes attributes { get; set; }
        public static Node Parse(string key, object jsondata)
        {
            //Generic Dictionary for deserialization     
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;

            // Access nested values 
            Dictionary<string, object> partDict = jsonDataDict["part"] as Dictionary<string, object>;
            Dictionary<string, object> dataDict = partDict["data"] as Dictionary<string, object>;
            Dictionary<string, object> frameDataDict = dataDict["frame"] as Dictionary<string, object>;

            //Create class instances of node elements
            Node node = new Node();
            node.part = new Part();
            node.attributes = new Attributes();

            //Set node type_id
            node.type_id = key;

            //Get dtype from partDict
            string dtype = (string)partDict["dtype"];
            
            //Check dtype, and determine how they should be deserilized.
            if (dtype != "compas.datastructures/Part")
            {
                DtypeGeometryDesctiptionSelector(node, dtype, dataDict);
            }
            else
            {
                PartDesctiptionSelector(node, dataDict);
            }

            //Parse frame from class method
            node.part.frame = Frame.FromData(frameDataDict);

            Debug.Log("Node Deserilized");

            return node;
        }
        private static void DtypeGeometryDesctiptionSelector(Node node, string dtype, Dictionary<string, object> jsonDataDict) //TODO: Adjust Static Class Parsing
        {
            //Set node part dtype
            node.part.dtype = dtype;

            switch (dtype)
            {
                case "compas.geometry/Cylinder":
                    
                    //Set node type_data
                    node.type_data = "0.Cylinder";
                    
                    // Accessing different parts of json data to make common attributes dictionary
                    float height = Convert.ToSingle(jsonDataDict["height"]);
                    float radius = Convert.ToSingle(jsonDataDict["radius"]);

                    //Add Items to the attributes dictionary remapping name to length, width, height
                    node.attributes.length = radius;
                    node.attributes.width = radius;
                    node.attributes.height = height;
                    break;

                case "compas.geometry/Box":
                    
                    //Set node type_data
                    node.type_data = "1.Box";

                    // Accessing different parts of json data to make common attributes dictionary
                    float xsize = Convert.ToSingle(jsonDataDict["xsize"]);
                    float ysize = Convert.ToSingle(jsonDataDict["ysize"]);
                    float zsize = Convert.ToSingle(jsonDataDict["zsize"]);

                    //Add Items to the attributes dictionary remapping name to length, width, height
                    node.attributes.length = xsize;
                    node.attributes.width = ysize;
                    node.attributes.height = zsize;
                    break;
                
                case "compas.datastructures/Mesh":

                    //Set node type_data
                    node.type_data = "3.Mesh"; //TODO: SET LWH to 0 (Doesn't solve, but also prevents errors for objectLengthButton.)

                    // Set Node Length width height to 0 because it does not contain definitions for this information.
                    node.attributes.length = 0.00f;
                    node.attributes.width = 0.00f;
                    node.attributes.height = 0.00f;

                    Debug.Log("This is a Mesh assembly");
                    break;

                case "compas.geometry/Frame":
                    
                    //Set node type_data //TODO: SET LWH to 0 (Doesn't solve, but also prevents errors for objectLengthButton.)
                    node.type_data = "4.Frame";

                    // Set Node Length width height to 0 because it does not contain definitions for this information.
                    node.attributes.length = 0.00f;
                    node.attributes.width = 0.00f;
                    node.attributes.height = 0.00f;

                    Debug.Log("This is a frame assembly");
                    break;

                case "compas_timber.parts/Beam":

                    //Set node type_data
                    node.type_data = "2.ObjFile";

                    // Accessing different parts of json data to make common attributes dictionary
                    float objLength = Convert.ToSingle(jsonDataDict["length"]);
                    float objWidth = Convert.ToSingle(jsonDataDict["width"]);
                    float objHeight = Convert.ToSingle(jsonDataDict["height"]);

                    //Add Items to the attributes dictionary remapping name to length, width, height
                    node.attributes.length = objLength;
                    node.attributes.width = objWidth;
                    node.attributes.height = objHeight;

                    break;

                case string connectionType when connectionType.StartsWith("compas_timber.connections"):
            
                    //Set node type_data
                    node.type_data = "5.Joint";

                    // Do not update attributes because this is not interactable.

                    Debug.Log("This is a timbers connection");
                    break;


                default:
                    Debug.Log("Default");
                    break;
            }
        }
        private static void PartDesctiptionSelector(Node node, Dictionary<string, object> jsonDataDict) //TODO: Adjust Static Class Parsing
        {
            //Access nested Part information.
            Dictionary<string, object> attributesDict = jsonDataDict["attributes"] as Dictionary<string, object>;
            Dictionary<string, object> nameDict = attributesDict["name"] as Dictionary<string, object>;
            Dictionary<string, object> partdataDict = nameDict["data"] as Dictionary<string, object>;

            //Get dtype from name dictionary
            string dtype = (string)nameDict["dtype"];

            //Call dtype description selector.
            DtypeGeometryDesctiptionSelector(node, dtype, partdataDict);

        }
        public bool IsValidNode()
        {   
            // Basic validation: Check if the required properties are present or have valid values
            if (!string.IsNullOrEmpty(type_id) &&
                !string.IsNullOrEmpty(type_data) &&
                part != null &&
                part.frame != null)
            {
                if (type_data == "5.Joint")
                {
                    Debug.Log("This is a timbers Joint and should be ignored");
                    return false;
                }
                else if (type_data != "4.Frame" || type_data != "3.Mesh")
                {
                    // Check if the required properties are present or have valid values
                    if (attributes != null &&
                        attributes?.length != null &&
                        attributes?.width != null &&
                        attributes?.height != null)
                    {
                        // Set default values for properties that may be null
                        return true;
                    }
                    else
                    {
                        // If it is not a frame assembly and does not have geometric description.
                        return false;
                    }
                }
                else
                {
                    // Set default values for properties that may be null
                    return true;
                }
            }
            Debug.Log($"node.type_id is: '{type_id}'");
            return false;
        }

    }

    [System.Serializable]
    public class Part
    {
        public Frame frame { get; set; }
        public string dtype { get; set; }

    }

    [System.Serializable]
    public class Attributes
    {
        public bool is_built { get; set;}
        public bool is_planned { get; set;}
        public string placed_by { get; set; }
        public float length { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        public string type { get; set; }
    } 

    [System.Serializable]
    public class Frame
    {
        public float[] point { get; set; }
        public float[] xaxis { get; set; }
        public float[] yaxis { get; set; }

        // Method to parse an instance of the class from a json string
        public static Frame Parse(object jsondata)
        {
            // Parse the json string into a dictionary
            Dictionary<string, object> frameDataDict = jsondata as Dictionary<string, object>;;
            return FromData(frameDataDict);
        }
        public static Frame FromData(Dictionary<string, object> frameDataDict)
        {            
            Frame frame = new Frame();
            float[] point = DataConverters.ConvertDatatoFloatArray(frameDataDict["point"]);
            float[] xaxis = DataConverters.ConvertDatatoFloatArray(frameDataDict["xaxis"]);
            float[] yaxis = DataConverters.ConvertDatatoFloatArray(frameDataDict["yaxis"]);

            if (point == null || xaxis == null || yaxis == null)
            {
                Debug.LogError("FrameParse: One or more arrays is null.");
            }
            else
            {
                frame.point = point;
                frame.xaxis = xaxis;
                frame.yaxis = yaxis;
            }

            return frame;
        }
        public Dictionary<string, object> GetData()
        {
            return new Dictionary<string, object>
            {
                { "point", point },
                { "xaxis", xaxis },
                { "yaxis", yaxis }
            };
        }

    }

    /////////////// Classes For Building Plan Desearialization///////////////////
    
    [System.Serializable]
    public class BuildingPlanData
    {
        public string LastBuiltIndex { get; set; }
        public Dictionary<string, Step> steps { get; set; }
        public static (BuildingPlanData, Dictionary<string, List<string>>) Parse(object jsondata, Dictionary<string, List<string>> PriorityTreeDictionary)
        {
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            (BuildingPlanData buildingPlanData, Dictionary<string, List<string>> priorityTreeDictionary) = BuildingPlanData.FromData(jsonDataDict, PriorityTreeDictionary);
            return (buildingPlanData, priorityTreeDictionary);
        }
        public static (BuildingPlanData, Dictionary<string, List<string>>) FromData(Dictionary<string, object> jsonDataDict, Dictionary<string, List<string>> PriorityTreeDictionary)
        {
            //Create new building plan instance
            BuildingPlanData buidingPlanData = new BuildingPlanData();
            buidingPlanData.steps = new Dictionary<string, Step>();
            
            //Attempt to get last built index and if it doesn't exist set it to null
            if (jsonDataDict.TryGetValue("LastBuiltIndex", out object last_built_index))
            {
                Debug.Log($"Last Built Index Fetched From database: {last_built_index.ToString()}");
                buidingPlanData.LastBuiltIndex = last_built_index.ToString();
            }
            else
            {
                buidingPlanData.LastBuiltIndex = null;
            }

            //Try to access steps as dictionary... might need to be a list
            List<object> stepsList = jsonDataDict["steps"] as List<object>;

            //Loop through steps desearialize and check if they are valid
            for(int i =0 ; i < stepsList.Count; i++)
            {
                string key = i.ToString();
                var json_data = stepsList[i];

                //Create step instance from the information
                Step step_data = Step.Parse(json_data);
                
                //Check if step is valid and add it to building plan dictionary
                if (step_data.IsValidStep())
                {
                    //Add step to building plan dictionary
                    buidingPlanData.steps[key] = step_data;
                    Debug.Log($"Step {key} successfully added to the building plan dictionary");

                    //Add step to priority tree dictionary
                    if (PriorityTreeDictionary.ContainsKey(step_data.data.priority.ToString()))
                    {
                        //If the priority already exists add the key to the list
                        PriorityTreeDictionary[step_data.data.priority.ToString()].Add(key);
                        Debug.Log($"Step {key} successfully added to the priority tree dictionary item {step_data.data.priority.ToString()}");
                    }
                    else
                    {
                        //If not create a new list and add the key to the list
                        PriorityTreeDictionary[step_data.data.priority.ToString()] = new List<string>();
                        PriorityTreeDictionary[step_data.data.priority.ToString()].Add(key);
                        Debug.Log($"Step {key} added a new priority {step_data.data.priority.ToString()} to the priority tree dictionary");
                    }
                }
                else
                {
                    Debug.LogWarning($"Invalid Step structure for key '{key}'. Not added to the dictionary.");
                }
            }
            return (buidingPlanData, PriorityTreeDictionary);
        }

    }
    
    [System.Serializable]
    public class Step
    {
        public Data data { get; set; }
        public string dtype { get; set; }
        public string guid { get; set; }

        public static Step Parse(object jsondata)
        {
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(jsonDataDict);
        }
        public static Step FromData(Dictionary<string, object> jsonDataDict)
        {
            //Create class instances of node elements
            Step step = new Step();

            //Set values for base node class to keep data structure consistent
            step.dtype = (string)jsonDataDict["dtype"];
            step.guid = (string)jsonDataDict["guid"];

            //Access nested information
            Dictionary<string, object> dataDict = jsonDataDict["data"] as Dictionary<string, object>;

            //Parse the frame information from the location dictionary
            step.data = Data.FromData(dataDict);

            return step;
        }
        public static bool AreEqualSteps(Step step ,Step NewStep)
        {
            // Basic validation: Check if two steps are equal
            if (step != null &&
                NewStep != null &&
                step.data.device_id == NewStep.data.device_id &&
                step.data.element_ids == step.data.element_ids &&
                step.data.actor == NewStep.data.actor &&
                step.data.location.point.SequenceEqual(NewStep.data.location.point) &&
                step.data.location.xaxis.SequenceEqual(NewStep.data.location.xaxis) &&
                step.data.location.yaxis.SequenceEqual(NewStep.data.location.yaxis) &&
                step.data.geometry == NewStep.data.geometry &&
                step.data.instructions.SequenceEqual(NewStep.data.instructions) &&
                step.data.is_built == NewStep.data.is_built &&
                step.data.is_planned == NewStep.data.is_planned &&
                step.data.elements_held.SequenceEqual(NewStep.data.elements_held) &&
                step.data.priority == NewStep.data.priority)
            {
                // Set default values for properties that may be null
                return true;
            }
            Debug.Log($"Steps with elementID : {step.data.element_ids[0]} and {NewStep.data.element_ids[0]} are not equal");
            return false;
        }
        public bool IsValidStep()
        {
            // Basic validation: Check if the required properties are present or have valid values
            if (data != null &&
                data.element_ids != null &&
                !string.IsNullOrEmpty(data.actor) &&
                data.location != null &&
                data.geometry != null &&
                data.instructions != null &&
                data.is_built != null &&
                data.is_planned != null &&
                data.elements_held != null &&
                data.priority != null)
            {
                return true;
            }
            return false;
        }

    }

    [System.Serializable]
    public class Data
    {
        public string device_id { get; set; }
        public string[] element_ids { get; set; }
        public string actor { get; set; }
        public Frame location { get; set; }
        public string geometry { get; set; }
        public string[] instructions { get; set; }
        public bool is_built { get; set; }
        public bool is_planned { get; set; }
        public int[] elements_held { get; set; }
        public int priority { get; set; }

        public static Data Parse(object jsondata)
        {
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(jsonDataDict);
        }
        public static Data FromData(Dictionary<string, object> dataDict)
        {
            //Create class instances of data class
            Data data = new Data();

            //Parse the frame as from the nested location dictionary
            Dictionary<string, object> locationDataDict = dataDict["location"] as Dictionary<string, object>;
            data.location = Frame.FromData(locationDataDict);

            //Try to get device_id for the step if it does not exist set it to null.
            if (dataDict.TryGetValue("device_id", out object device_id))
            {
                data.device_id = device_id.ToString();
            }
            else
            {
                data.device_id = null;
            }

            //Set values for step
            data.actor = (string)dataDict["actor"];
            data.geometry = (string)dataDict["geometry"];
            data.is_built = (bool)dataDict["is_built"];
            data.is_planned = (bool)dataDict["is_planned"];
            data.priority = (int)(long)dataDict["priority"];

            //Parse list informatoin from the items
            List<object> element_ids = dataDict["element_ids"] as List<object>;
            List<object> instructions = dataDict["instructions"] as List<object>;
            List<object> elements_held = dataDict["elements_held"] as List<object>;
            
            if (element_ids != null &&
                instructions != null &&
                elements_held != null)
            {
                data.elements_held = elements_held.Select(Convert.ToInt32).ToArray();
                data.element_ids = element_ids.Select(x => x.ToString()).ToArray();
                data.instructions = instructions.Select(x => x.ToString()).ToArray();
            }
            else
            {
                Debug.Log("FromData (Data): One of the lists is null or improperly casted.");
            }

            return data;
        }

    }

    ////////////////Classes for User Current Informatoin/////////////////////
    
    [System.Serializable]
    public class UserCurrentInfo
    {
        public string currentStep { get; set; }
        public string timeStamp { get; set; }
        public static UserCurrentInfo Parse(object jsondata)
        {
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            return FromData(jsonDataDict);
        }
        public static UserCurrentInfo FromData(Dictionary<string, object> jsonDataDict)
        {
            //Create class instances of node elements
            UserCurrentInfo userCurrentInfo = new UserCurrentInfo();
            userCurrentInfo.currentStep = (string)jsonDataDict["currentStep"];
            userCurrentInfo.timeStamp = (string)jsonDataDict["timeStamp"];
            return userCurrentInfo;
        }

    }
}