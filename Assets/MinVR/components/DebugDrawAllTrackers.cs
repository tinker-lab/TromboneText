using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MinVR {

    public class DebugDrawAllTrackers : MonoBehaviour {

        [Tooltip("Prefab of axes geometry to use as a cursor.  If the GameObject has a TextMesh component, the text will be set to the tracker name.")]
        public GameObject cursorPrefab;

        [Tooltip("It is useful to at least ignore the Head tracker so a cursor doesn't show up right in front of the eyes.")]
        public List<string> trackersToIgnore = new List<string>(new string[] { "Head" });

        // map tracker names to game objects
        private Dictionary<string, GameObject> cursors = new Dictionary<string, GameObject>();


        // Start is called before the first frame update
        void Start() {
            VRMain.Instance.AddOnVREventCallback(OnVREvent);
        }

        // Update is called once per frame
        void Update() {

        }

        void OnVREvent(VREvent e) {

            // only respond to tracker move events
            if ((e.DataIndex.ContainsKey("EventType")) &&
                (e.DataIndex.GetValueAsString("EventType") == "TrackerMove"))
			{
                string trackerName = e.Name.Remove(e.Name.IndexOf("_Move"), 5);
                if (!trackersToIgnore.Contains(trackerName)) {
                    float[] data = e.DataIndex.GetValueAsFloatArray("Transform");
                    Matrix4x4 m = VRConvert.ToMatrix4x4(data);
                    Vector3 pos = m.GetTranslation();
                    Quaternion rot = m.GetRotation();

                    if (!cursors.ContainsKey(trackerName)) {
                        GameObject newCursorObj = Instantiate(cursorPrefab);
                        TextMesh label = newCursorObj.GetComponentInChildren<TextMesh>();
                        if (label != null) {
                            label.text = trackerName;
                        }
                        cursors[trackerName] = newCursorObj;
                    }
                    GameObject cursorObj = cursors[trackerName];
                    cursorObj.transform.position = pos;
                    cursorObj.transform.rotation = rot;
                }
            }
        }
    }
}