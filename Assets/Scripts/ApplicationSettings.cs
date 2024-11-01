    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    
    namespace CompasXR.AppSettings
    {
        [System.Serializable]
        public class ApplicationSettings
        {
            public string project_name {get; set;}
            public string storage_folder {get; set;}
            public bool z_to_y_remap {get; set;}
        }
    }