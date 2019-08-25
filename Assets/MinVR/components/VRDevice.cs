using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MinVR {

    public class VRDevice : MonoBehaviour {


        // NETWORKING SETTINGS:

        public enum VRNodeType { StandAlone, NetClient /*, NetServer */ };  // TODO: Add NetServer option

        [Tooltip("StandAlone does not use a network connection.  " +
            "All NetClient(s) must connect to a single MinVR server upon startup." +
            /* "NetServer acts as the single server for one or more NetClients." + */
            "A command line option of the form '-vrnodetype NetClient' will override this value.")]
        public VRNodeType vrNodeType = VRNodeType.StandAlone;


        [Tooltip("If vrNodeType==NetClient, then this is used as the ip address of the server to connect to.  " +
            "A command line option of the form '-serverip 127.0.0.1' will override this value.")]
        public string serverIPAddress = "127.0.0.1";

        [Tooltip("If vrNodeType==NetClient, then this is used as the server's port number.  " +
            "A command line option of the form '-serverport 3490' will override this value.")]
        public int serverPort = 3490;



        // GRAPHICS WINDOW SETTINGS:

        [Tooltip("X position of the graphics window on screen when not running inside the editor.  " +
            "A command line option of the form '-xpos 100' will override this value.")]
        public int windowXPos = 0;
        [Tooltip("Y position of the graphics window on screen when not running inside the editor.  " +
            "A command line option of the form '-ypos 100' will override this value.")]
        public int windowYPos = 0;
        [Tooltip("Width of the graphics window on screen when not running inside the editor.  " +
            "A command line option of the form '-width 1800' will override this value.")]
        public int windowWidth = 1024;
        [Tooltip("Height of the graphics window on screen when not running inside the editor.  " +
            "A command line option of the form '-height 1200' will override this value.")]
        public int windowHeight = 768;



        // INPUT EVENT SETTINGS:

        [Tooltip("If true, converts mouse up/down events from unity to VREvent ButtonUp/Down events.")]
        public bool unityMouseBtnsToVREvents = false;

        [Tooltip("If true, converts mouse movement events from unity to VREvent CursorMove events.")]
        public bool unityMouseMovesToVREvents = false;

        [Tooltip("Other unity inputs, like key presses can be converted to VREvents so that they can be handled with " +
            "MinVR's event system, just like tracker buttons.  This is the list of Unity buttons accessible via " +
            "Input.GetKeyDown() that MinVR should listen for and convert to button up/down VREvents.")]
        public List<string> unityKeysToVREvents = new List<string>();

        [Tooltip("Other unity inputs, like joystick values can be converted to VREvents so that they can be handled with " +
            "MinVR's event system, just like analog sensors.  This is the list of Unity axes accessible via " +
            "Input.GetAxis() that MinVR should listen for and convert to analog update VREvents.")]
        public List<string> unityAxesToVREvents = new List<string>();

        // TODO: add more Unity to VREvent conversions as needed over time...


        // Aliases could be stored in a dictionary, but if we use a List of these little classes,
        // then it's possible to view them in the Inspector.
        [System.Serializable]
        public class EventAlias {
            public string aliasName;
            public string rawEventName;

            public EventAlias(string aliasName, string rawEventName) {
                this.aliasName = aliasName;
                this.rawEventName = rawEventName;
            }
        }

        [Tooltip("Create an alias for an event.  Useful when writing apps that run across devices.  For example, " +
                 "write scripts that listen for the alias 'BrushBtn_Down' and then map this to different raw input " +
                 "events based on the vrdevice you are running on.  For example: " +
                 "Alias=BrushBtn_Down, RawEventName=Kbdb_Down (when running in desktop mode) or " +
                 "Alias=BrushBtn_Down, RawEventName=WandTrigger_Down (when running in cave mode).")]
        public List<EventAlias> eventAliases = new List<EventAlias>();


        public void AddEventAlias(string aliasName, string rawEventName) {
            eventAliases.Add(new EventAlias(aliasName, rawEventName));
        }

        public bool FindAliasesForEvent(string eventName, ref List<string> aliases) {
            for (int i = 0; i < eventAliases.Count; i++) {
                if (eventAliases[i].rawEventName == eventName) {
                    aliases.Add(eventAliases[i].aliasName);
                }
            }
            return (aliases.Count > 0);
        }


    }

}
