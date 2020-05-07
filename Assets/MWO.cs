using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Timers;


public class MWO : MonoBehaviour
{
    //for timer
    //private static Timer timer;
    // for mouse wheel letters
    int jmwo = 0;
    int imwo = 0;
    int timerCount =  0;
    char letter = 'A';
    char beforeLetter = '_';
    char afterLetter = 'B';
    char timerLetter = '_';
    char tLetter = '_';
    string endWord = "";


    
    public Text mouseWheelValue;
    public Text aboveWheelValue;
    public Text belowWheelValue;
    public Text inputFieldValue;
    public Text endWordValue;

    float timer = 0;
    double pauseLength = 1.5; //set your pause length to count the letter here
    double stopper = 1.505; // stops from adding in multiple of the same letter
    double endWordLength = 4.0; //set time in which to end your word
   
   

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime; //Starts timer
        if (timer > pauseLength) // if timer is > pause length
        {
            if(timer < stopper)// and timer is < stopper
            {
                inputFieldValue.text += letter.ToString(); // update our input field with that letter
                endWord += letter; //and we will also add it to our end word
                
                
            }
           
        }
        if(timer > endWordLength)// if timer is > end word length then we will display the final word
        {
            endWordValue.text = "Your Final Word: " + endWord;
        }






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
            
            if(timer > endWordLength) // if you're building a new word
            {
                inputFieldValue.text = "";
            }
            jmwo += 1; //increment the count

            if (jmwo > 25) //if are wheel position is greater or equal to 25
            {
                jmwo = 25;
                //jmwo += 1; //increment the count
            }
            ////////
            if (jmwo < 25)// if jmwo is not our last letter position
            {
                afterLetter = alpha[jmwo + 1];// then our afterletter position is 1 greater than our letter position
            }
            else if (jmwo == 25)// if it does equal 25 then our position is  - for our after letter
            {
                afterLetter = '-';
            }
            else //checks for an error
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

            /* aTimer.Stop();
             aTimer.Start();*/
            timer = 0;

           
            
            



        }
        else if (w < 0f)// if scrollwheel has moved down
        {
            
            
            if (timer > endWordLength) // if you're building a new word
            {
                inputFieldValue.text = "";
            }
            if (jmwo < 0) // if jmwo is lessthan 0 set it back to 0
            {
                jmwo = 0;
                beforeLetter = '-';
            }
            else if (jmwo < 26 & jmwo > 0 & jmwo != 25)// if jmwo is less than 25 or after letter will be position jmwo +1
            {
                afterLetter = alpha[jmwo + 1];
                beforeLetter = alpha[jmwo - 1]; // before letter will be equal to the letter in the aplhabet at the position of jmwo -1
            }
            else if (jmwo == 25) // if jmwo is = 25 (z) then our next letter will show '-'
            {
                afterLetter = '-';
                beforeLetter = alpha[jmwo - 1]; // before letter will be equal to the letter in the aplhabet at the position of jmwo -1
            }

            else if (jmwo == 0) // if jmwo is = 1 then our before letter will just show a '_'
            {
                beforeLetter = '_';
                afterLetter = alpha[jmwo + 1];
            }
            else // checks for er
            {
                beforeLetter = '$';
                Debug.Log("ERRRRRRRRRRRRRRRRRRRRRRRRRRROOOOORRR" + jmwo);
            }
            
            

            letter = alpha[jmwo]; // letter is going to equal to the letter in the alphabet at that position of jmwo
            jmwo -= 1; //decount jmwo by 1
            //timerLetter = letter;
            /*aTimer.Stop();
            aTimer.Start();*/
            timer = 0;



        }
        else {
            if (jmwo < 0) {
                jmwo = 0;
            } 
            letter = letter; }
        //Debug.Log("The count is: " + jmwo);
        //Debug.Log("The before letter is: " + beforeLetter + "Value: " + (jmwo -1));
        //Debug.Log("The letter is: " + letter);
        //Debug.Log("The after letter is: " + afterLetter + "Value: "+ (jmwo +1) );
        mouseWheelValue.text ="Your letter is: " + letter.ToString();
        aboveWheelValue.text = beforeLetter.ToString();
        belowWheelValue.text = afterLetter.ToString();




        /*if(aTimer >= 2000)
         {

         }
         Debug.Log("tletter is : " + tLetter);*/




        /*Debug.Log("jmwo is : " + jmwo);
        Debug.Log("beforeLetter is: " + beforeLetter);
        Debug.Log("Letter is: " + letter);
        Debug.Log("afterLetter is: " + afterLetter);*/
    }


    
}
