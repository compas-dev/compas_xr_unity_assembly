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
            Dictionary<string, object> jsonDataDict = jsondata as Dictionary<string, object>;
            Node node = FromData(jsonDataDict, key);
            Debug.Log("Node Deserilized");
            return node;
        }
        public static Node FromData(Dictionary<string, object> jsonDataDict, string key)
        {
            Node node = new Node();
            node.part = new Part();
            node.attributes = new Attributes();
            node.type_id = key;
            DtypeGeometryDesctiptionSelector(node, jsonDataDict);
            return node;
        }
        private static void DtypeGeometryDesctiptionSelector(Node node, Dictionary<string, object> jsonDataDict)
        {
            //Set node part dtype
            Dictionary<string, object> partDict = jsonDataDict["part"] as Dictionary<string, object>;
            Dictionary<string, object> dataDict = partDict["data"] as Dictionary<string, object>;
            string dtype = (string)partDict["dtype"];

            switch (dtype)
            {
                case "compas.geometry/Cylinder":

                    node.part.frame = Frame.Parse(dataDict["frame"]);
                    node.part.dtype = dtype;

                    float height = Convert.ToSingle(dataDict["height"]);
                    float radius = Convert.ToSingle(dataDict["radius"]);
                    node.attributes.length = radius;
                    node.attributes.width = radius;
                    node.attributes.height = height;

                    break;

                case "compas.geometry/Box":
                    
                    node.part.dtype = dtype;
                    node.part.frame = Frame.Parse(dataDict["frame"]);

                    float xsize = Convert.ToSingle(dataDict["xsize"]);
                    float ysize = Convert.ToSingle(dataDict["ysize"]);
                    float zsize = Convert.ToSingle(dataDict["zsize"]);
                    node.attributes.length = xsize;
                    node.attributes.width = ysize;
                    node.attributes.height = zsize;

                    break;

                case "compas.geometry/Frame":
                    
                    node.part.dtype = dtype;
                    node.part.frame = Frame.FromData(dataDict);

                    //TODO: SET LWH to 0 (Doesn't solve, but also prevents errors for objectLengthButton.)
                    node.attributes.length = 0.00f;
                    node.attributes.width = 0.00f;
                    node.attributes.height = 0.00f;

                    break;

                case "compas.datastructures/Mesh":

                    node.part.dtype = dtype;

                    Dictionary<string, object> frameDict;
                    if (jsonDataDict.TryGetValue("frame", out object frameObject))
                    {
                        frameDict = jsonDataDict["frame"] as Dictionary<string, object>;
                        Dictionary<string, object> frameDataDict = frameDict["data"] as Dictionary<string, object>;
                        node.part.frame = Frame.FromData(frameDataDict);
                    }
                    else
                    {
                        node.part.frame = Frame.RhinoWorldXY();
                    }

                    node.attributes.length = 0.00f;
                    node.attributes.width = 0.00f;
                    node.attributes.height = 0.00f;

                    break;

                case "compas_timber.parts/Beam":

                    node.part.dtype = dtype;
                    node.part.frame = Frame.Parse(dataDict["frame"]);

                    float objLength = Convert.ToSingle(dataDict["length"]);
                    float objWidth = Convert.ToSingle(dataDict["width"]);
                    float objHeight = Convert.ToSingle(dataDict["height"]);
                    node.attributes.length = objLength;
                    node.attributes.width = objWidth;
                    node.attributes.height = objHeight;

                    break;

                case string connectionType when connectionType.StartsWith("compas_timber.connections"):
                    //TODO: Set dtype to only compas_timber.connections so It can be checked in valid node without .StartsWith
                    node.part.dtype = "compas_timber.connections";
                    break;

                case "compas.datastructures/Part":
                    PartDesctiptionSelector(node, jsonDataDict);
                    break;

                default:
                    Debug.LogError($"DtypeGeometryDesctiptionSelector: No Deserilization type for dtype {dtype}.");
                    break;
            }
        }
        private static void PartDesctiptionSelector(Node node, Dictionary<string, object> jsonDataDict)
        {
            Dictionary<string, object> partDict = jsonDataDict["part"] as Dictionary<string, object>;
            Dictionary<string, object> dataDict = partDict["data"] as Dictionary<string, object>;
            Dictionary<string, object> attributesDict = dataDict["attributes"] as Dictionary<string, object>;
            Dictionary<string, object> shapeDict = attributesDict["shape"] as Dictionary<string, object>;
            Dictionary<string, object> shapeDataDict = shapeDict["data"] as Dictionary<string, object>;
            string dtype = (string)shapeDict["dtype"];

            switch (dtype)
            {
                case "compas.geometry/Cylinder":

                    node.part.frame = Frame.Parse(shapeDataDict["frame"]);
                    node.part.dtype = dtype;

                    float height = Convert.ToSingle(shapeDataDict["height"]);
                    float radius = Convert.ToSingle(shapeDataDict["radius"]);
                    node.attributes.length = radius;
                    node.attributes.width = radius;
                    node.attributes.height = height;

                    break;

                case "compas.geometry/Box":
                    
                    node.part.dtype = dtype;
                    node.part.frame = Frame.Parse(shapeDataDict["frame"]);

                    float xsize = Convert.ToSingle(shapeDataDict["xsize"]);
                    float ysize = Convert.ToSingle(shapeDataDict["ysize"]);
                    float zsize = Convert.ToSingle(shapeDataDict["zsize"]);
                    node.attributes.length = xsize;
                    node.attributes.width = ysize;
                    node.attributes.height = zsize;

                    break;
                
                case "compas.datastructures/Mesh":

                    node.part.dtype = dtype;

                    Dictionary<string, object> frameDict;
                    if (jsonDataDict.TryGetValue("frame", out object frameObject))
                    {
                        frameDict = jsonDataDict["frame"] as Dictionary<string, object>;
                        Dictionary<string, object> frameDataDict = frameDict["data"] as Dictionary<string, object>;
                        node.part.frame = Frame.FromData(frameDataDict);
                    }
                    else
                    {
                        node.part.frame = Frame.RhinoWorldXY();
                    }

                    node.attributes.length = 0.00f;
                    node.attributes.width = 0.00f;
                    node.attributes.height = 0.00f;

                    break;

                case "compas.geometry/Frame":
                    
                    node.part.dtype = dtype;
                    node.part.frame = Frame.FromData(shapeDataDict);

                    //TODO: SET LWH to 0 (Doesn't solve, but also prevents errors for objectLengthButton.)
                    node.attributes.length = 0.00f;
                    node.attributes.width = 0.00f;
                    node.attributes.height = 0.00f;

                    if (attributesDict.TryGetValue("name", out object name))
                    {
                        string nameString = name.ToString();
                        if(nameString.StartsWith("QR_"))
                        {
                            node.part.dtype = "compas_xr/QRCode";
                        }
                    }

                    break;

                default:
                    Debug.LogError($"PartDesctiptionSelector: No Part Deserilization type for dtype {dtype}.");
                    break;
                
            }
        }
        public bool IsValidNode()
        {   
            if (!string.IsNullOrEmpty(type_id) &&
                !string.IsNullOrEmpty(part.dtype) &&
                part != null &&
                part.frame != null)
            {
                if (part.dtype == "compas_timber.connections")
                {
                    Debug.Log("This is a timbers Joint and should be ignored");
                    return false;
                }
                else if (part.dtype != "compas.geometry/Frame" || 
                        part.dtype != "compas.datastructures/Mesh" ||
                        part.dtype != "compas_xr/QRCode")
                {
                    if (attributes != null &&
                        attributes?.length != null &&
                        attributes?.width != null &&
                        attributes?.height != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
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
        public static Frame RhinoWorldXY()
        {
            /*
            Returns a frame that represents the world XY plane in Rhino coordinates.
            */
            Frame frame = new Frame();
            frame.point = new float[] { 0.0f, 0.0f, 0.0f };
            frame.xaxis = new float[] { 1.0f, 0.0f, 0.0f };
            frame.yaxis = new float[] { 0.0f, 1.0f, 0.0f };
            return frame;
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