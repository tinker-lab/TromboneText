using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MinVR
{
    public class StylusTextInput : MonoBehaviour
    {

        public GameObject textObject;

        // Start is called before the first frame update
        void Start()
        {
            VRMain.Instance.AddOnVRAnalogUpdateCallback("BlueStylusAnalog", UpdateText);
        }


        void UpdateText(float value)
        {
            
            float remapped = Map(value, 0, 63, 0, 25);
            char letter = (char)('a' + Mathf.RoundToInt(remapped));
            print("Stylus update: " + value+" "+letter);


           textObject.GetComponent<Text>().text += letter;
        }

        float Map(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            float oldRange = oldMax - oldMin;
            float newRange = newMax - newMin;

            return ((value - oldMin) * newRange) / oldRange + newMin; 
        }
}
}
