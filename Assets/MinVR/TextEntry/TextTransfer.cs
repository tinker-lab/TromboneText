using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextTransfer : MonoBehaviour
{
    // Start is called before the first frame update
    public string theString;
    public GameObject inputField;
    public GameObject stringDisplay;


    public void StoreName()
    {
        theString = inputField.GetComponent<Text>().text;
        stringDisplay.GetComponent<Text>().text = "This is your text " + theString;
    }
}
