using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MWO : MonoBehaviour
{
    //Prints output of mouse wheel
    int travel;
    int scrollSpeed = 3;
    int jmwo = 0;
    int imwo = 0;

    [SerializeField]
    private Text mouseWheelValue;

    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var d = Input.GetAxis("Mouse ScrollWheel");
        //Debug.Log(d);
        float w = Input.GetAxis("Mouse ScrollWheel");
        //char[] alpha = new char[26]; 

        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        char letter = '\0';
        /*for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            alpha[i] = letter;
            i = i + 1;
        }*/
        if (w > 0f) // if scrollwheel has moved up
        {
            if (jmwo >= 26)
            {
                jmwo = 0;
            }
            jmwo += 1;
            letter = alpha[jmwo];
            Debug.Log(letter);

        }
        else if (w < 0f)// if scrollwheel has moved down
        {
            if (jmwo < 0)
            {
                jmwo = 0;
            }
            jmwo -= 1;
            letter = alpha[jmwo];
            Debug.Log(letter);
        }
        else { letter = letter; }
        mouseWheelValue.text ="Your letter is: " + letter.ToString();
        /*if (d > 0f && travel > -30)
        {
            travel = travel - scrollSpeed;
            Camera.main.transform.Translate(0, 0, 1 * scrollSpeed, Space.Self);
        }
        else if(d < 0f && travel < 100)
        {
            travel = travel + scrollSpeed;
            Camera.main.transform.Translate(0, 0, -1 * scrollSpeed, Space.Self);
        }*/
    }



}
