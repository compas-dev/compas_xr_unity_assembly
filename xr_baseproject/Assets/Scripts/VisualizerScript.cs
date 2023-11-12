using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualizerScript : MonoBehaviour
{

    //private variables
    public GameObject Preview_Builder;
    public GameObject ID_Button;
    public GameObject ObjectLength_Button;
    public GameObject Robot_Button;
    public GameObject Background;



    // Start is called before the first frame update
    void Start()
    {
        //For each button, define OnClick Action and prefab
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(Visualizer_Toggle);
        Background.SetActive(false);


    }

     //Toggle ON and OFF the dropdown submenu options
    private void Visualizer_Toggle()
    {
        //deactivate the buttons if they are on
        if (Robot_Button.activeSelf == true)
        {
            Preview_Builder.SetActive(false);
            ID_Button.SetActive(false);
            ObjectLength_Button.SetActive(false);
            Robot_Button.SetActive(false);
            Background.SetActive(false);
        }
        else 
        {
            Preview_Builder.SetActive(true);
            ID_Button.SetActive(true);
            ObjectLength_Button.SetActive(true);
            Robot_Button.SetActive(true);
            Background.SetActive(true);
        }
    }


}
