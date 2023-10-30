using UnityEngine;
using TMPro;

public class SaveInputs : MonoBehaviour
{
    public TMP_InputField[] inputFields;
    private const string InputPrefix = "FirebaseInput_";

    void Start()
    {
        LoadInputs();
    }

    public void SaveInputFields()
    {
        for (int i = 0; i < inputFields.Length; i++)
        {
            string inputText = inputFields[i].text;
            PlayerPrefs.SetString(InputPrefix + i, inputText);
            Debug.Log("Input Saved: " + inputText);
        }
        PlayerPrefs.Save();
    }

    private void LoadInputs()
    {
        for (int i = 0; i < inputFields.Length; i++)
        {
            string key = InputPrefix + i;
            if (PlayerPrefs.HasKey(key))
            {
                string savedInput = PlayerPrefs.GetString(key);
                inputFields[i].text = savedInput;
                Debug.Log("Input Loaded: " + savedInput);
            }
        }
    }
}
