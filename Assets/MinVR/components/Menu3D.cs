using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu3D : MonoBehaviour
{

    public string title = "Main Menu";
    public List<string> items = new List<string>(new string[] { "Item 1", "Item 2" });
    public List<string> eventsToGenerate = new List<string>(new string[] { "MainMenu1", "MainMenu2" });

    public string trackerEvent = "Hand_Move";
    public string buttonDownEvent = "Kbdm_Down";
    public string buttonUpEvent = "Kbdm_Up";


    public Color titleColor = new Color(1.0f, 1.0f, 1.0f);
    public Color titleBGColor = new Color(85.0f / 255.0f, 83.0f / 255.0f, 83.0f / 255.0f);
    public Color titleHighColor = new Color(19.0f / 255.0f, 0.0f / 255.0f, 239.0f / 255.0f);

    public Color itemColor = new Color(0.0f, 0.0f, 0.0f);
    public Color itemBGColor = new Color(230.0f / 255.0f, 230.0f / 255.0f, 221.0f / 221.0f);
    public Color itemHighColor = new Color(255.0f / 255.0f, 195.0f / 255.0f, 0.0f / 255.0f);

    public Color pressColor = new Color(226.0f / 255.0f, 0.0f / 255.0f, 23.0f / 255.0f);


    public float textSizeInWorldUnits = 0.2f;
    public float itemSep = 0.08f;
    public Vector2 padding = new Vector2(0.05f, 0.05f);
    public float depth = 0.1f;
    public float zEpsilon = 0.001f;


    private Font font;
    private Material fontMaterial;

    private List<TextMesh> labelMeshes = new List<TextMesh>();
    private List<GameObject> labelBoxes = new List<GameObject>();
    private GameObject titleBoxObj;
    private GameObject bgBox;
    private Vector3 lastTrackerPos;
    private Quaternion lastTrackerRot;

    // -1 = nothing, 0 = titlebar, 1..items.Count = menu items
    private int selected = -1;
    private bool buttonPressed = false;

    Vector2 TextExtents(TextMesh textMesh) {
        // https://forum.unity.com/threads/computing-exact-size-of-text-line-with-textmesh.485767/
        Vector2 extents = new Vector2();
        foreach (char symbol in textMesh.text) {
            CharacterInfo info;
            if (textMesh.font.GetCharacterInfo(symbol, out info, textMesh.fontSize, textMesh.fontStyle)) {
                extents[0] += info.advance;
                if (info.glyphHeight > extents[1]) {
                    extents[1] = info.glyphHeight;
                }
            }
        }
        extents[0] = extents[0] * textMesh.characterSize * 0.1f;
        extents[1] = extents[1] * textMesh.characterSize * 0.1f;
        return extents;
    }

    // true if point p lies inside a Cube primitive that has been translated,
    // rotated, and scaled to create a rectangular box, like a 3D button
    bool InsideTransformedCube(Vector3 p, GameObject boxObj) {
        Vector3 pBoxSpace = boxObj.transform.InverseTransformPoint(p);
        return (Mathf.Abs(pBoxSpace[0]) <= 0.5) &&
            (Mathf.Abs(pBoxSpace[1]) <= 0.5) &&
            (Mathf.Abs(pBoxSpace[2]) <= 0.5);
    }


    // Start is called before the first frame update
    void Start()
    {
        font = Resources.Load<Font>("Fonts/Futura_Medium_BT");
        fontMaterial = Resources.Load<Material>("Material/Futura_Medium_BT_WithOcclusion");

        MinVR.VRMain.Instance.AddOnVRTrackerMoveCallback(trackerEvent, OnTrackerMove);
        MinVR.VRMain.Instance.AddOnVRButtonDownCallback(buttonDownEvent, OnButtonDown);
        MinVR.VRMain.Instance.AddOnVRButtonUpCallback(buttonUpEvent, OnButtonUp);

       
        // Create a title box and label
        GameObject titleObj = new GameObject(title);
        titleObj.transform.parent = this.transform;

        GameObject titleTextObj = new GameObject(title + " Label");
        titleTextObj.transform.SetParent(titleObj.transform);
        TextMesh titleTextMesh = titleTextObj.AddComponent<TextMesh>();
        titleTextMesh.font = font;
        titleTextMesh.GetComponent<MeshRenderer>().material = fontMaterial;
        titleTextMesh.text = title.ToUpper();
        titleTextMesh.color = titleColor;
        titleTextMesh.anchor = TextAnchor.MiddleLeft;
        titleTextMesh.fontSize = 100;
        titleTextMesh.characterSize = textSizeInWorldUnits * 10.0f / titleTextMesh.fontSize;


        titleBoxObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        titleBoxObj.name = title + " Box";
        titleBoxObj.transform.SetParent(titleObj.transform);
        titleBoxObj.GetComponent<Renderer>().material.color = titleBGColor;


        // Create a box and label for each item
        for (int i=0; i<items.Count; i++) {
            GameObject itemObj = new GameObject(items[i]);
            itemObj.transform.parent = this.transform;
            
            GameObject textObj = new GameObject(items[i] + " Label");
            textObj.transform.SetParent(itemObj.transform);
            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.font = font;
            textMesh.GetComponent<MeshRenderer>().material = fontMaterial;
            textMesh.text = items[i];
            textMesh.color = itemColor;
            textMesh.anchor = TextAnchor.MiddleLeft;
            textMesh.fontSize = 100;
            textMesh.characterSize = textSizeInWorldUnits * 10.0f / textMesh.fontSize;


            GameObject boxObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boxObj.name = items[i] + " Box";
            boxObj.transform.SetParent(itemObj.transform);
            boxObj.GetComponent<Renderer>().material.color = itemBGColor;

            labelMeshes.Add(textMesh);
            labelBoxes.Add(boxObj);
        }


        // Calculate the max extents
        Vector2 max_text_extents = new Vector2();
        for (int i = 0; i < labelMeshes.Count; i++) {
            Vector2 text_extents = TextExtents(labelMeshes[i]);
            max_text_extents[0] = Mathf.Max(text_extents[0], max_text_extents[0]);
            max_text_extents[1] = Mathf.Max(text_extents[1], max_text_extents[1]);
        }

        // size of activatable box
        Vector3 menu_box_dims = new Vector3(max_text_extents[0] + 2.0f * padding[0],
                                            max_text_extents[1] + 2.0f * padding[1],
                                            depth);

        float height = items.Count * menu_box_dims[1] +
                       (items.Count - 1) * itemSep;

        // special case: title bar taller than items
        Vector2 title_extents = TextExtents(titleTextMesh);
        if (height < title_extents[0]) {
            height = title_extents[0] + 2.0f * padding[0];
        }

        // set transforms to use for drawing boxes and labels

        titleBoxObj.transform.position = new Vector3(-0.5f * menu_box_dims[0] - 0.5f * menu_box_dims[1], 0f, 0f);
        titleBoxObj.transform.localScale = new Vector3(menu_box_dims[1], height, depth);

        titleTextMesh.transform.position = new Vector3(-0.5f * menu_box_dims[0] - 0.5f * menu_box_dims[1],
                                                       -0.5f * height + padding[0],
                                                       -0.5f * depth - zEpsilon);
        titleTextMesh.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90.0f));

        float y = 0.5f * height - 0.5f * menu_box_dims[1];
        for (int i = 0; i < items.Count; i++) {
            labelBoxes[i].transform.position = new Vector3(0.0f, y, 0.0f);
            labelBoxes[i].transform.localScale = menu_box_dims;
            labelMeshes[i].transform.position = new Vector3(-0.5f * menu_box_dims[0] + padding[0], y, -0.5f * depth - zEpsilon);
            y -= menu_box_dims[1] + itemSep;
        }

        bgBox = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bgBox.name = "Background Box";
        bgBox.transform.SetParent(this.transform);
        bgBox.GetComponent<Renderer>().material.color = itemBGColor;
        bgBox.transform.position = new Vector3(zEpsilon, 0f, 0.5f * itemSep + zEpsilon);
        bgBox.transform.localScale = new Vector3(menu_box_dims[0] - zEpsilon, height - 2.0f * zEpsilon, menu_box_dims[2] - itemSep);
    }


    // Update is called once per frame
    void Update() {

    }


    void OnTrackerMove(Vector3 pos, Quaternion rot) {
        Vector3 posMenuSpace = transform.InverseTransformPoint(pos);

        if ((buttonPressed) && (selected == 0)) {
            // In dragging state, move the menu
            Vector3 deltaPos = pos - lastTrackerPos;
            Quaternion deltaRot = rot * Quaternion.Inverse(lastTrackerRot);
            float deltaAngle;
            Vector3 deltaAxis;
            deltaRot.ToAngleAxis(out deltaAngle, out deltaAxis);

            transform.RotateAround(pos, deltaAxis, deltaAngle);

            //transform.rotation = deltaRot * transform.rotation;
            transform.position = transform.position + deltaPos;
        }
        else if (!buttonPressed) {
            // Clear selection and highlighting
            selected = -1;
            titleBoxObj.GetComponent<Renderer>().material.color = titleBGColor;
            for (int i = 0; i < labelBoxes.Count; i++) {
                labelBoxes[i].GetComponent<Renderer>().material.color = itemBGColor;
            }

            // Update selection
            if (InsideTransformedCube(pos, titleBoxObj)) {
                selected = 0;
                titleBoxObj.GetComponent<Renderer>().material.color = titleHighColor;
            }
            else {
                for (int i=0; i<labelBoxes.Count; i++) {
                    if (InsideTransformedCube(pos, labelBoxes[i])) {
                        selected = i + 1;
                        labelBoxes[i].GetComponent<Renderer>().material.color = itemHighColor;
                    }
                }
            }
        }

        lastTrackerPos = pos;
        lastTrackerRot = rot;
    }

    void OnButtonDown() {
        buttonPressed = true;
        if (selected == 0) {
            titleBoxObj.GetComponent<Renderer>().material.color = pressColor;
        }
        else if (selected > 0) {
            labelBoxes[selected - 1].GetComponent<Renderer>().material.color = pressColor;

            string eName = eventsToGenerate[selected - 1];
            MinVR.VRDataIndex data = new MinVR.VRDataIndex(eName);
            data.AddData("EventType", "ButtonDown");
            MinVR.VRMain.Instance.QueueEvent(new MinVR.VREvent(eName, data));
        }
    }

    void OnButtonUp() {
        buttonPressed = false;
    }

}
