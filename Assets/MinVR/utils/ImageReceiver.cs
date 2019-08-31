using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageReceiver : MonoBehaviour
{
    private Texture2D tex;

    // Start is called before the first frame update
    void Start()
    {
        tex = new Texture2D(100, 100, TextureFormat.RGBAFloat, false);
        MinVR.VRMain.Instance.AddOnVREventCallback("ImageUpdate", this.OnImageUpdate);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Called whenever an ImageUpdate event is received via MinVR
    void OnImageUpdate(MinVR.VREvent e)
    {
        // access the image data buffer from the event
        float[] buffer = e.GetFloatArrayData("Buffer");

        // copy data into an array of colors
        int w = 100;
        int h = 100;
        Color[] colors = new Color[w * h];
        int bindex = 0;
        int cindex = 0;
        for (int y=0; y<h; y++) {
            for (int x=0; x<w; x++) {
                colors[cindex] = new Color(buffer[bindex + 0], buffer[bindex + 1], buffer[bindex + 2], 1.0f);
                cindex++;
                bindex += 3;
            }
        }

        tex.SetPixels(colors);
        tex.Apply();

        // connect texture to material of GameObject this script is attached to
        GetComponent<Renderer>().material.mainTexture = tex;
    }
}
