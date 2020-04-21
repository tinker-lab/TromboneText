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
    char letter = 'A';
    char beforeLetter = '_';
    char afterLetter = 'B';

    
    public Text mouseWheelValue;
    public Text aboveWheelValue;
    public Text belowWheelValue;

    
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
        //Debug.Log(w);
        //char[] alpha = new char[26]; 

        char[] alpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
       
        /*for (char letter = 'A'; letter <= 'Z'; letter++)
        {
            alpha[i] = letter;
            i = i + 1;
        }*/
        if (w > 0f) // if scrollwheel has moved up
        {
            jmwo += 1; //increment the count
            if (jmwo > 25) //if are wheel position is greater or equal to 25
            {
                jmwo = 0;
                //jmwo += 1; //increment the count
            }
            ////////
            if (jmwo < 25)// if jmwo is not our last letter position
            {
                afterLetter = alpha[jmwo + 1];// then our afterletter position is 1 greater than our letter position
            }
            else if (jmwo == 25)// if it does equal 25 then our position is reset back to 0 for our after letter
            {
                afterLetter = alpha[0];
            }
            else
            {
                afterLetter = '$';
                Debug.Log("ERRRRRRRRRRRRRRRRRRRRRRRRRRROOOOORRR" + jmwo);
            }
            if (jmwo == 0)// if we are in the 'a' position 
            {
                beforeLetter = '_';// set before letter to _
            }
            else
            {
                beforeLetter = alpha[jmwo - 1]; // sets beforeLetter to the letter in the alphabet before the letter 
            }



            letter = alpha[jmwo]; //sets letter to the letter in the alphabet at that position
           

           
            
            



        }
        else if (w < 0f)// if scrollwheel has moved down
        {
            if (jmwo < 0) // if jmwo is lessthan 0 set it back to 0
            {
                jmwo = 0;
            }
            if (jmwo < 26)// if jmwo is less than 25 or after letter will be position jmwo +1
            {
                afterLetter = alpha[jmwo + 1];
            }
            else if (jmwo == 25) // if jmwo is = 25 (z) then our next letter will show 'a'
            {
                afterLetter = alpha[0];
            }
            else
            {
                beforeLetter = '$';
                Debug.Log("ERRRRRRRRRRRRRRRRRRRRRRRRRRROOOOORRR" + jmwo);
            }
            
            
            jmwo -= 1; //decount jmwo by 1
            letter = alpha[jmwo]; // letter is going to equal to the letter in the alphabet at that position of jmwo
            beforeLetter = alpha[jmwo-1]; // before letter will be equal to the letter in the aplhabet at the position of jmwo -1
           
            if (jmwo == 0) // if jmwo is = 1 then our before letter will just show a '_'
            {
                beforeLetter = '_';
            }
            

            
        }
        else {
            if (jmwo < 0) {
                jmwo = 0;
            } 
            letter = letter; }
        Debug.Log("The count is: " + jmwo);
        Debug.Log("The before letter is: " + beforeLetter + "Value: " + (jmwo -1));
        Debug.Log("The letter is: " + letter);
        Debug.Log("The after letter is: " + afterLetter + "Value: "+ (jmwo +1) );
        mouseWheelValue.text ="Your letter is: " + letter.ToString();
        aboveWheelValue.text = beforeLetter.ToString();
        belowWheelValue.text = afterLetter.ToString();
        
    }



}
