using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CompasXR.Core;


namespace CompasXR.Testing
{
    public class nameSpaceTestingMono : MonoBehaviour //TODO: Able to access single instance of the class from a namespace, and all of its methods but can have troubles.
    {
        //Static instance to be initilized on class loading
        private static nameSpaceTestingMono instance = new nameSpaceTestingMono();

        //public variable used to access the variable
        public static nameSpaceTestingMono Instance
        {
            get {return instance;}
        }

        //Private constructor preventing instantiation outside of this class
        private nameSpaceTestingMono() {}

        public int RandomMonoInt = 1;

        public static int RandomStaticMonoInt = 2;

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        //Testing accessing From namespace
        public void methodAccessTesting()
        {
            Debug.Log("Printing random int from my in calss method " + RandomMonoInt);
        }

        public void methodAccessStaticMonoTesting()
        {
            Debug.Log("Printing random static int from my method " + RandomStaticMonoInt);
        }
    }
}

// namespace CompasXR.StaticTesting
// {
//     public static class nameSpaceTestingStatic //TODO: Accessable without creating an instance of the class, but good for accessing via namespace... Good for maintaining state, but can cause problems w/ testing & inheritance. 
//     {
//         public static int RandomStaticInt = 3;

//         // public int RandomInt = 4;

//         public static void methodAccessTestingStatic()
//         {
//             Debug.Log("Printing random int from my method " + RandomStaticInt);
//         }
//     }

//     public class nameSpaceTesting //TODO: only accessible once a class instance is created and can be accessed through the instance...
//     {
//         public int RandomClassInt = 4;

//         public void methodAccessTesting()
//         {
//             Debug.Log("Printing random int from standard class method " + RandomClassInt);
//         }
//     }
// }