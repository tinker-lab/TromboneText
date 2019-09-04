using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MinVR
{
    /**
	 * Genertates VREvents for inputs coming from VRPN (like the events from Optitrack or the buttons on the stylus).
     * 
     * Note: A game object that this is attached to will be deactivated automatically by VRMain unless a VRDevice is active and running as a NetServer
	 */
    public class VRPNInput : MonoBehaviour, VREventGenerator
    {
        [Tooltip("If empty, VRPNAnalog objects in the scene will be automatically found at runtime")]
        public List<VRPNAnalog> analogInputs;

        [Tooltip("If empty, VRPNButton objects in the scene will be automatically found at runtime")]
        public List<VRPNButton> buttonInputs;

        [Tooltip("If empty, VRPNTracker objects in the scene will be automatically found at runtime")]
        public List<VRPNTracker> trackerInputs;


        void Start()
        {
            VRMain.Instance.AddEventGenerator(this);

            if (analogInputs.Count == 0)
            {
                analogInputs.AddRange(Resources.FindObjectsOfTypeAll(typeof(VRPNAnalog)) as VRPNAnalog[]);
            }

            if (buttonInputs.Count == 0)
            {
                buttonInputs.AddRange(Resources.FindObjectsOfTypeAll(typeof(VRPNButton)) as VRPNButton[]);
            }

            if (trackerInputs.Count == 0)
            {
                trackerInputs.AddRange(Resources.FindObjectsOfTypeAll(typeof(VRPNTracker)) as VRPNTracker[]);
            }
        }

        public void AddEventsSinceLastFrame(ref List<VREvent> eventList)
        {
            foreach (VRPNAnalog input in analogInputs)
            {
                input.AddEventsSinceLastFrame(ref eventList);
            }

            foreach (VRPNButton input in buttonInputs)
            {
                input.AddEventsSinceLastFrame(ref eventList);
            }

            foreach (VRPNTracker input in trackerInputs)
            {
                input.AddEventsSinceLastFrame(ref eventList);
            }
        }
    }
}
