using UnityEngine;

namespace ApplicationModeControler
{
    //Control Visulization & Touch Modes
    public class ModeControler
    {
        public VisulizationMode VisulizationMode { get; set; }
        public TouchMode TouchMode { get; set; }

        public ModeControler()
        {
            VisulizationMode = VisulizationMode.BuiltUnbuilt;
            TouchMode = TouchMode.None;
        }
    }

    //Enum to addapt to various coloring modes
    public enum VisulizationMode
    {
        BuiltUnbuilt = 0,
        ActorView = 1,

        //TODO: MAS: 7. Add new enum for sequence color
        SequenceColor = 2,
    }
    
    //Enum to addapt to various touch modes            
    public enum TouchMode
    {
        None = 0,
        ElementEditSelection = 1,

    }


}

