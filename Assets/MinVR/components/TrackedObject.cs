using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MinVR {

    public class TrackedObject : MonoBehaviour {

        [Tooltip("Name of the VREvent to listen for for tracking updates (e.g., Head_Move, LHand_Move, RHand_Move, ...)")]
        public string trackingEvent = "LHand_Move";

        [Tooltip("Offset (if any) to add to the transform (e.g., to align a virtual cursor with a tracked physical prop).")]
        public Vector3 translationalOffset;

        [Tooltip("Offset (if any) to add to the transform (e.g., to align a virtual cursor with a tracked physical prop).")]
        public Vector3 rotationalOffset;

        [Tooltip("If empty, tracking updates are applied to the GameObject this script is attached to.  " +
            "To track other GameObjects instead, add them to this list.")]
        public List<GameObject> trackedObjects;


        void Start() {
            VRMain.Instance.AddOnVRTrackerMoveCallback(trackingEvent, OnTrackerMove);
        }


        public void OnTrackerMove(Vector3 pos, Quaternion rot) {
            Vector3 newPos = translationalOffset + pos;
            Quaternion newRot  = Quaternion.Euler(rotationalOffset) * rot;

            if (trackedObjects.Count == 0) {
                transform.position = newPos;
                transform.rotation = newRot;
            }
            else {
                for (int i=0; i<trackedObjects.Count; i++) {
                    trackedObjects[i].transform.position = newPos;
                    trackedObjects[i].transform.rotation = newRot;
                }
            }
        }

    }

}
