    // Create a class structure that matches the JSON data
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JSON
{   
    
   /////////////Classes for Assembly Desearialization./////////////// 
    [System.Serializable]
    public class Node
    {
        public Part part { get; set; }
        public string type_data { get; set; }
        public string type_id { get; set; }
        public Attributes attributes { get; set; }
    }

    [System.Serializable]
    public class Part
    {
        public Frame frame { get; set; }
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
        
        //TODO: GET RID OF?
        public string type { get; set; }
    } 

    [System.Serializable]
    public class Frame
    {
        public float[] point { get; set; }
        public float[] xaxis { get; set; }
        public float[] yaxis { get; set; }
    }

    /////////////// Classes For Step Desearialization///////////////////
    
    [System.Serializable]
    public class Step
    {
        public Data data { get; set; }
        public string dtype { get; set; }
        public string guid { get; set; }
    }

    [System.Serializable]
    public class Data
    {
        public string[] element_ids { get; set; }
        public string actor { get; set; }
        public Frame location { get; set; }
        public string geometry { get; set; }
        public string[] instructions { get; set; }
        public bool is_built { get; set; }
        public bool is_planned { get; set; }
        public int[] elements_held { get; set; }
        public System.Int64 priority { get; set; }
    }
    
}