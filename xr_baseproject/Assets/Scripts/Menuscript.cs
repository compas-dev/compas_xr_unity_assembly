using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menuscript : MonoBehaviour
{

    //private variables
    public GameObject Reload_Button;
    public GameObject Editor_Button;
    public GameObject Info_Button;
    public GameObject Background;
    public GameObject Communication;


    // Start is called before the first frame update
    void Start()
    {
        //For each button, define OnClick Action and prefab
        Button btn = GetComponent<Button>();
        btn.onClick.AddListener(Menu_Toggle);
        Background.SetActive(false);


    }

     //Toggle ON and OFF the dropdown submenu options
    private void Menu_Toggle()
    {
        //deactivate the buttons if they are on
        if (Reload_Button.activeSelf == true)
        {
            Reload_Button.SetActive(false);
            Editor_Button.SetActive(false);
            Info_Button.SetActive(false);
            Background.SetActive(false);
            Communication.SetActive(false);
        }
        else 
        {
            Reload_Button.SetActive(true);
            Editor_Button.SetActive(true);
            Info_Button.SetActive(true);
            Background.SetActive(true);
            Communication.SetActive(true);
        }
    }


}
