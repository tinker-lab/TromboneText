using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPS : MonoBehaviour
{
    public Color textColor = Color.white;

    [Tooltip("Prefab of textmesh to use to display fps.")]
    public GameObject labelPrefab;

    [Tooltip("Label Position in world space")]
    public Vector3 labelPosition;

    private TextMesh label;

    void Start()
    {
        GameObject newObj = Instantiate(labelPrefab);
        label = newObj.GetComponent<TextMesh>();
        newObj.transform.position = labelPosition;
        label.color = textColor;
    }

    void Update()
    {
        if (label != null)
        {
            float fps = 1.0f / Time.smoothDeltaTime;
            string text = string.Format("{0:0.} fps", fps);
            label.text = text;
        }
    }
   

    void OnGUI() {
        int w = Screen.width;
        int h = Screen.height;
        GUIStyle style = new GUIStyle();
        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = textColor;
        float fps = 1.0f / Time.smoothDeltaTime;
        string text = string.Format("{0:0.} fps", fps);
        GUI.Label(rect, text, style);
    }
}
