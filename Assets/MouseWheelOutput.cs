using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseWheelOutput : MonoBehaviour
{
   /* //Prints output of mouse wheel
    int travel;
    int scrollSpeed = 3;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var d = Input.GetAxis("Mouse ScrollWheel");
        Debug.Log(d);
        float w = Input.GetAxis("Mouse ScrollWheel");
        //char[] alpha = new char[26]; 
        int j = 0;
        int i = 0;
        char[] alpha = Enumerable.Range('A', 'Z' - 'A' + 1).Select(i => (Char)i).ToArray();
        *//*for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            alpha[i] = letter;
            i = i + 1;
        }*//*
        if (w > 0f) // if scrollwheel has moved up
        {
            if (j >= 26)
            {
                j = 0;
            }
            j += 1;
            Debug.Log(alpha[j]);

        }
        else if (w < 0f)// if scrollwheel has moved down
        {
            if (j <0)
            {
                j = 0;
            }
            j -= 1;
            Debug.Log(alpha[j]);
        }
        *//*if (d > 0f && travel > -30)
        {
            travel = travel - scrollSpeed;
            Camera.main.transform.Translate(0, 0, 1 * scrollSpeed, Space.Self);
        }
        else if(d < 0f && travel < 100)
        {
            travel = travel + scrollSpeed;
            Camera.main.transform.Translate(0, 0, -1 * scrollSpeed, Space.Self);
        }*//*
    }
    
*/

}
