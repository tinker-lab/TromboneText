using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MinVR {

    public class VRTrackedDesktopCamera : MonoBehaviour {
        [Tooltip("Name of the VREvent to listen for for tracking updates (e.g., Head_Move)")]
        public string trackingEvent = "Head_Move";

        [Tooltip("The camera to apply the tracking updates to.  Defaults to Main Camera.")]
        public Camera cam;


        void Start() {
            if (cam == null) {
                cam = Camera.main;
            }

            VRMain.Instance.AddOnVRTrackerMoveCallback(trackingEvent, OnTrackerMove);
        }


        public void OnTrackerMove(Vector3 pos, Quaternion rot) {
            cam.transform.position = pos;
            cam.transform.rotation = rot;
        }

    }
}
