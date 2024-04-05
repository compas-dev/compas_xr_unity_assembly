using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JSON
{   
   ///////////// Class for Handeling Data conversion Inconsistencies /////////////// 

    //General Class to handle conversions of different data types.
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

    //Overall Class for storing the node data structure


    //Class for storing the part distintion of the node


    //Class for storing the attributes of the node

    //Class for storing Frame Data //TODO: DECIDE IF WE DO THIS TOGETHER OR NOT.
    [System.Serializable]
    public class Frame
    {
        public float[] point { get; set; }
        public float[] xaxis { get; set; }
        public float[] yaxis { get; set; }

        // Method to parse an instance of the class from a json string
        public static Frame Parse(Dictionary<string, object> frameDataDict)
        {            
            //Create a new instance of the class
            Frame frame = new Frame();

            //Method for converting data values to fload arrays.
            float[] point = DataConverters.ConvertDatatoFloatArray(frameDataDict["point"]);
            float[] xaxis = DataConverters.ConvertDatatoFloatArray(frameDataDict["xaxis"]);
            float[] yaxis = DataConverters.ConvertDatatoFloatArray(frameDataDict["yaxis"]);

            if (point == null || xaxis == null || yaxis == null)
            {
                // At least one of the arrays is null
                Debug.LogError("FrameParse: One or more arrays is null.");
            }
            else
            {
                // All arrays are not null, proceed with assignment
                frame.point = point;
                frame.xaxis = xaxis;
                frame.yaxis = yaxis;
            }

            return frame;
        }

        // Method to convert an instance of the class to a dictionary
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
}