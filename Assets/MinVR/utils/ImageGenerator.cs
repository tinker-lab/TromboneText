using System.Collections;
using System.Collections.Generic;
using MinVR;
using UnityEngine;

public class ImageGenerator : MonoBehaviour, MinVR.VREventGenerator
{
    // Start is called before the first frame update
    void Start()
    {
        // Treat this class as a virtual input device so MinVR will poll it
        // once each frame to ask for any events generated since the last frame
        MinVR.VRMain.Instance.AddEventGenerator(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Add a single new event called "ImageUpdate" that contains a buffer of floats
    // for the image data.
    public void AddEventsSinceLastFrame(ref List<VREvent> eventList)
    {
        int w = 100;
        int h = 100;
        float[] imgBuffer = new float[w * h];
        int index = 0;
        for (int y=0; y<h; y++) {
            for (int x=0; x<w; x++) {
                if (index % 2 == 0) {
                    imgBuffer[index] = 1.0f;
                }
                else {
                    imgBuffer[index] = 0.0f;
                }
                index++;
            }
        }

        VREvent e = new VREvent("ImageUpdate");
        e.AddData("Buffer", imgBuffer);
        eventList.Add(e);
    }

}
