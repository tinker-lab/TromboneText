using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MinVR {

    /** VRMain must be the first script in Unity's execution order, something that must be set manually in the Unity Editor:
	 *  1. Go to Edit -> Project Settings -> Script Execution Order.
	 *  2. Click on the "+" button and select the MinVR.VRMain script.
	 *  3. Drag it above the bar labeled "Default Time".  This will set its order to "-100", which means its Update() method
	 *     will be called before the Update() method for any other script.
     *
     *
     *  This class is the brains of the MinVR system.  You must create one instance of this class in order to use MinVR,
     *  there can only be one instance per application, and the instance must persist across scene changes.  The class
     *  itself enforces this by using a Singleton pattern.  You can access VRMain from any other script using
     *  VRMain.Instance.
     *
     * 
     *  Using VRMain with VRDevices:
     *
     *  You should define one or more VRDevices in your application for VRMain to use by attaching a VRDevice
     *  component to one or more GameObjects.  When initialized, VRMain will activate exactly one of these
     *  VRDevices and deactivate all others.  This acts like a switch, so you can attach anything that should only
     *  run on one VRDevice to that device.  You can tell VRMain which VRDevice to activate with a command line
     *  argument, a critical feature for starting multiple instances of the same app for each wall of a Cave, for
     *  example.  You can also provide a default VRDevice by attaching a VRDevice component directly to VRMain or
     *  dragging a GameObject with a VRDevice attached to it to VRMain's "Default VR Device" field in the Inspector,
     *  but if "-vrdevice DeviceName" is specified on the command line, it will override both of these ways of
     *  specifying a default device.
     *
     *  A recommend strategy is to create a hierarchy of GameObjects that looks like something like this:
     *
     *  + VRDevices: (parent empty GameObject used only to keep things organized)
     *    + Desktop (attach a VRDevice component to all of these GameObjects)
     *    + 3DTV
     *    + Powerwall
     *    + CaveLeftWall
     *    + CaveRightWall
     *    + CaveFrontWall
     *    + CaveFloor
     *    ...
     *
     *  Attach a VRDevice component to each one and adjust the settings as needed.  Also, add additional scripts
     *  needed for each device, like FakeTrackerInput for testing with the Desktop mode, TrackedProjectionScreen
     *  for the various walls, etc.  When your app starts up, just one of the devices will be activated, like a
     *  switch.
     *
     *
     *  Registering event callbacks:
     *
     *  To respond to user input in your scripts, register event callbacks using the VRMain.AddOnVR*Callback()
     *  functions.  For example:
     *
     *  public class MyClass : MonoBehavior {
     *  
     *      public void Start() {
	 *          MinVR.VRMain.Instance.AddOnVRAnalogUpdateCallback("ArduinoAnalog01_Update", this.OnSliderUpdate);
     *      }
     *
     *      public void OnSliderUpdate(float val) {
	 *          Debug.Log("New slider value = " + val);
     *      }
     * 
     *  }
     *
     */
    public class VRMain : MonoBehaviour {

        // Use this to access to the singleton instance of VRMain that persists across scene loads/unloads
        public static VRMain Instance { get { return instance; } }
        private static VRMain instance;

        [Tooltip("To specify a default device to use if no command line '-vrdevice DeviceName' option is specified " +
            "you can either drag a default device here or add a VRDevice component directly to VRMain.  Note that " +
            "these defaults will be overwritten if '-vrdevice DeviceName' is specified on the command line.")]
        public VRDevice defaultVRDevice;

        [Tooltip("For debugging only -- Command line arguments to use when running in the Unity editor only.")]
        public string editorCmdLineArgs = "";

        [Tooltip("For debugging only -- Print all VREvents to Debug.Log -- slow!")]
        public bool debugLogEvents = false;



        // -- Delegates for VREvent callbacks --

        // Generic callback is a function that takes one VREvent parameter
        public delegate void OnVREventDelegate(VREvent e);

        // Analog event callback is a function that takes one float value
        public delegate void OnVRAnalogEventDelegate(float value);

        // Button down callback does not have any parameters
        public delegate void OnVRButtonDownEventDelegate();

        // Button up callback does not have any parameters
        public delegate void OnVRButtonUpEventDelegate();

        // Cursor move event callbacks take 2 parameters both Vector2.  The raw position is typically
        // reported in pixels.  The normalized position ranges from -1 to +1 in x and y.
        public delegate void OnVRCursorEventDelegate(Vector2 rawPos, Vector2 normalizedPos);

        // Tracker event callbacks take 2 parameters that report the position and rotation of the
        // tracker.
        public delegate void OnVRTrackerEventDelegate(Vector3 pos, Quaternion rot);






        // -- Routines for registering callbacks --

        // Register a callback using this method in order to receive a callback for every single
        // VR event that occurs.
        // Example:
        //  public void OnVREvent(VREvent e) {
        //    Debug.Log(e.ToString());
        //  }
        //
        //  public void Start() {
        //    MinVR.VRMain.Instance.AddOnVREventCallback(this.OnVREvent);
        //  }
        public void AddOnVREventCallback(OnVREventDelegate func) {
            genericCallbacksUnfiltered.Add(func);
        }


        // Register a callback using this method in order to receive a callback for all
        // VR events of the given name.
        // Example:
        //  public void OnFrameStart(VREvent e) {
        //    Debug.Log(e.ToString());
        //  }
        //
        //  public void Start() {
        //    MinVR.VRMain.Instance.AddOnVREventCallback("FrameStart", this.OnFrameStart);
        //  }
        public void AddOnVREventCallback(string eventName, OnVREventDelegate func) {
            if (!genericCallbacks.ContainsKey(eventName)) {
                genericCallbacks.Add(eventName, new List<OnVREventDelegate>());
            }
            genericCallbacks[eventName].Add(func);
        }


        // Register a callback using this method in order to receive a callback for analog
        // events of the specified name.
        // Example:
        //  public void OnSliderUpdate(float val) {
        //    Debug.Log("New slider value = " + val);
        //  }
        //
        //  public void Start() {
        //    MinVR.VRMain.Instance.AddOnVRAnalogUpdateCallback("ArduinoAnalog01_Update", this.OnSliderUpdate);
        //  }
        public void AddOnVRAnalogUpdateCallback(string eventName, OnVRAnalogEventDelegate func) {
            if (!analogCallbacks.ContainsKey(eventName)) {
                analogCallbacks.Add(eventName, new List<OnVRAnalogEventDelegate>());
            }
            analogCallbacks[eventName].Add(func);
        }


        // Register a callback using this method in order to receive a callback for button down
        // events of the specified name.
        // Example:
        //  public void OnWandBtnDown() {
        //    Debug.Log("Button Down!");
        //  }
        //
        //  public void Start() {
        //    MinVR.VRMain.Instance.AddOnVRButtonDownCallback("WandBtn1_Down", this.OnWandBtnDown);
        //  }
        public void AddOnVRButtonDownCallback(string eventName, OnVRButtonDownEventDelegate func) {
            if (!buttonDownCallbacks.ContainsKey(eventName)) {
                buttonDownCallbacks.Add(eventName, new List<OnVRButtonDownEventDelegate>());
            }
            buttonDownCallbacks[eventName].Add(func);
        }

        // Register a callback using this method in order to receive a callback for button up
        // events of the specified name.
        // Example:
        //  public void OnWandBtnUp() {
        //    Debug.Log("Button Down!");
        //  }
        //
        //  public void Start() {
        //    MinVR.VRMain.Instance.AddOnVRButtonUpCallback("WandBtn1_Up", this.OnWandBtnUp);
        //  }
        public void AddOnVRButtonUpCallback(string eventName, OnVRButtonUpEventDelegate func) {
            if (!buttonUpCallbacks.ContainsKey(eventName)) {
                buttonUpCallbacks.Add(eventName, new List<OnVRButtonUpEventDelegate>());
            }
            buttonUpCallbacks[eventName].Add(func);
        }

        // Register a callback using this method in order to receive a callback for cursor move
        // events of the specified name.
        // Example:
        //  public void OnJoystick(Vector2 pos, Vector2 normalizedPos) {
        //    Debug.Log("Joystick update: " + normalizedPos);
        //  }
        //
        //  public void Start() {
        //    MinVR.VRMain.Instance.AddOnVRCursorMoveCallback("Joystick01_Move", this.OnJoystick);
        //  }
        public void AddOnVRCursorMoveCallback(string eventName, OnVRCursorEventDelegate func) {
            if (!cursorCallbacks.ContainsKey(eventName)) {
                cursorCallbacks.Add(eventName, new List<OnVRCursorEventDelegate>());
            }
            cursorCallbacks[eventName].Add(func);
        }

        // Register a callback using this method in order to receive a callback for tracker move
        // events of the specified name.
        // Example:
        //  public void OnHandMove(Vector3 pos, Quaternion rot) {
        //    Debug.Log("Hand now located at: " + pos);
        //  }
        //
        //  public void Start() {
        //    MinVR.VRMain.Instance.AddOnVRTrackerMoveCallback("Hand_Move", this.OnHandMove);
        //  }
        public void AddOnVRTrackerMoveCallback(string eventName, OnVRTrackerEventDelegate func) {
            if (!trackerCallbacks.ContainsKey(eventName)) {
                trackerCallbacks.Add(eventName, new List<OnVRTrackerEventDelegate>());
            }
            trackerCallbacks[eventName].Add(func);
        }


        // Use this to register a class that implements the VREventGenerator interface.  VRMain will then
        // poll it once per frame to collect any new events generated since the past frame.  The events
        // are then synchronized across all nodes (if running on a network) and event callbacks are called
        // as usual.
        public void AddEventGenerator(VREventGenerator dev) {
            _inputDevices.Add(dev);
        }
    

        // As an alternative to making your class a virtual input device that gets polled each frame for
        // new events by implementing the "EventGenerator" interface, if you're class only occasionally
        // generates a new event, you might prefer to just queue the event directly with VRMain by calling
        // this function.  The event will be processed the following frame.
        public void QueueEvent(VREvent e) {
            _queuedEvents.Add(e);
        }

        /** Implementation Details Below this Point **/

        private VRDevice vrDevice;

		// Storage for event callbacks
		private List<OnVREventDelegate> genericCallbacksUnfiltered = new List<OnVREventDelegate>();
		private Dictionary<string, List<OnVREventDelegate>> genericCallbacks = new Dictionary<string, List<OnVREventDelegate>>();
		private Dictionary<string, List<OnVRAnalogEventDelegate>> analogCallbacks = new Dictionary<string, List<OnVRAnalogEventDelegate>>();
		private Dictionary<string, List<OnVRButtonDownEventDelegate>> buttonDownCallbacks = new Dictionary<string, List<OnVRButtonDownEventDelegate>>();
		private Dictionary<string, List<OnVRButtonUpEventDelegate>> buttonUpCallbacks = new Dictionary<string, List<OnVRButtonUpEventDelegate>>();
		private Dictionary<string, List<OnVRCursorEventDelegate>> cursorCallbacks = new Dictionary<string, List<OnVRCursorEventDelegate>>();
		private Dictionary<string, List<OnVRTrackerEventDelegate>> trackerCallbacks = new Dictionary<string, List<OnVRTrackerEventDelegate>>();

        private VRNetInterface _netInterface;
		//private VRNetClient _netClient;

		// When Unity starts up, Update seems to be called twice before we reach the EndOfFrame callback, so we maintain
		// a state variable here to make sure that we don't request events twice before requesting swapbuffers.
		private enum NetState { PreUpdateNext, PostRenderNext }
		private NetState _state = NetState.PreUpdateNext;


		private List<VREventGenerator> _inputDevices = new List<VREventGenerator>();

		private List<VREvent> _inputEvents = new List<VREvent>();
        private List<VREvent> _queuedEvents = new List<VREvent>();

		private bool _initialized = false;

		private Vector3 lastMousePos = new Vector3();

		private Dictionary<string, float> axesStates = new Dictionary<string, float>();




		private void Initialize() {

            // 1. FIGURE OUT WHICH VRDEVICE TO START

            // start with a null device
            vrDevice = null;

            // If a VRDevice to start is specified on the command line, then that is the one we will use.
            try {
                // process command line args
                string[] args = System.Environment.GetCommandLineArgs();
#if UNITY_EDITOR
                // if running in the editor add the editor command line args
                string[] sysArgs = args;
                string[] editorArgs = editorCmdLineArgs.Split(' ');
                args = new string[sysArgs.Length + editorArgs.Length];
                Array.Copy(sysArgs, args, sysArgs.Length);
                Array.Copy(editorArgs, 0, args, sysArgs.Length, editorArgs.Length);
#endif

                int i = 1;
                while (i < args.Length) {

                    // help command
                    if ((args[i] == "-h") || (args[i] == "-help") || (args[i] == "--help")) {
                        Debug.Log("Command Line Arguments:\n" +
                            "-help\n" +
                            "     Display this message.\n" +
                            "-vrdevice [name of a GameObject tagged with 'VRDevice']\n" +
                            "     Activates the specified GameObject and deactivates all others tagged as a VRDevice."
                        );
                        i++;
                    }

                    // vrdevice
                    else if (args[i] == "-vrdevice") {
                        if (args.Length <= i) {
                            throw new Exception("Missing command line parameter for -vrdevice.");
                        }
                        string vrDeviceName = args[i + 1];

                        // Find all GameObjects (even inactive ones) with a VRDevice component attached
                        // and see if there is one that matches the name provided on the command line
                        foreach (VRDevice dev in Resources.FindObjectsOfTypeAll(typeof(VRDevice)) as VRDevice[]) {
                            if (dev.name == vrDeviceName) {
                                vrDevice = dev;
                            }
                        }
                        if (vrDevice == null) {
                            throw new Exception("Got a '-vrdevice GameObject' command line argument, but cannot find a VRDevice named: " + vrDeviceName);
                        }
                        i += 2;
                    }

                    // ignore all other arguments
                    else {
                        //Debug.Log("Ignoring command line argument: " + args[i]);
                        i++;
                    }
                }
            }
            catch (Exception e) {
                Debug.LogException(e, this);
            }

            // If vrDevice is still null after parsing the command line, then see if a defaultDevice was
            // provided to VRMain in the inspector
            if (vrDevice == null) {
                vrDevice = defaultVRDevice;
            }

            // If vrDevice is still null, then see if there is a VRDevice component attached to VRMain
            if (vrDevice == null) {
                VRDevice dev = this.GetComponent(typeof(VRDevice)) as VRDevice;
                if (dev != null) {
                    vrDevice = dev;
                }
            }

            // If vrDevice is still null, then  proceed with default settings provided by the VRDevice constructor
            if (vrDevice == null) {
                Debug.Log("No VRDevice specified, adding a default one to VRMain.");
                vrDevice = gameObject.AddComponent(typeof(VRDevice)) as VRDevice;
            }



            // 2. ACTIVATE THE SPECIFIED VRDEVICE AND DEACTIVATE ALL OTHERS

			// Activate this vrdevice
			vrDevice.gameObject.SetActive(true);

            // and deactivate all others
            foreach (VRDevice dev in Resources.FindObjectsOfTypeAll(typeof(VRDevice)) as VRDevice[]) {
                if ((dev != vrDevice) && (dev != this)) {
                    dev.gameObject.SetActive(false);
                }
            }



            // 3. INITIALIZE VRMAIN WITH SETTINGS FROM THE VRDEVICE
            WindowUtils.RemoveBorder();
			WindowUtils.SetPositionAndSize(vrDevice.windowXPos, vrDevice.windowYPos, vrDevice.windowWidth, vrDevice.windowHeight);
            WindowUtils.SetWindowTitle("MinVRUnity " + vrDevice.name);

			if (vrDevice.vrNodeType == VRDevice.VRNodeType.NetClient) {
				_netInterface = new VRNetClient(vrDevice.serverIPAddress, vrDevice.serverPort);

                foreach (VRPNInput vrpn in Resources.FindObjectsOfTypeAll(typeof(VRPNInput)) as VRPNInput[])
                {
                   vrpn.gameObject.SetActive(false);
                }
            }
            else if (vrDevice.vrNodeType == VRDevice.VRNodeType.NetServer) {
                _netInterface = new VRNetServer(vrDevice.serverPort, vrDevice.numClients);

                foreach (VRPNInput vrpn in Resources.FindObjectsOfTypeAll(typeof(VRPNInput)) as VRPNInput[])
                {
                    vrpn.gameObject.SetActive(true);
                }
            }
            else if (vrDevice.vrNodeType == VRDevice.VRNodeType.StandAlone)
            {
                if (vrDevice.activateVRPNInputObjects)
                {
                    foreach (VRPNInput vrpn in Resources.FindObjectsOfTypeAll(typeof(VRPNInput)) as VRPNInput[])
                    {
                        vrpn.gameObject.SetActive(true);
                    }
                }
                else
                {
                    foreach (VRPNInput vrpn in Resources.FindObjectsOfTypeAll(typeof(VRPNInput)) as VRPNInput[])
                    {
                        vrpn.gameObject.SetActive(false);
                    }

                }
            }

            _initialized = true;
		}




		// AT THE START OF EACH FRAME: SYNCHRONIZE INPUT EVENTS AND CALL ONVREVENT CALLBACK FUNCTIONS
		private void PreUpdate() {


            // 1. COLLECT ANY NEW INPUT
            _inputEvents.Clear();

            // Add any user generated events queued since the last frame.
            for (int i=0; i<_queuedEvents.Count; i++) {
                _inputEvents.Add(_queuedEvents[i]);
            }
            _queuedEvents.Clear();

            // Add input events from Unity's input system, converting them to VREvents
            AddUnityInputEvents(ref this._inputEvents);

			// Add input events from this client's input devices (such as fake input events)
			for (int i = 0; i < this._inputDevices.Count; i++) {
				this._inputDevices[i].AddEventsSinceLastFrame(ref this._inputEvents);
			}



            // 2. SYNCHRONIZE INPUT EVENTS ACROSS ALL NODES
			// Synchronize with the server
			if (_netInterface != null) {
				_netInterface.SynchronizeInputEventsAcrossAllNodes(ref _inputEvents);
				_state = NetState.PostRenderNext;
			}


			// 3. CALL EVENT CALLBACK ROUTINES WITH THE SYNCHRONIZED LIST OF EVENTS
			for (int i = 0; i < _inputEvents.Count; i++) {
				List<string> aliases = new List<string>();
				vrDevice.FindAliasesForEvent(_inputEvents[i].Name, ref aliases);

				// Optionally, print out the events for debugging
				if (debugLogEvents) {
					string output = "";
					if (aliases.Count > 0)
					{
						output = "Aliases: ";
                        for (int j=0; j<aliases.Count; j++)
						{
							output = output + aliases[j] + " ";
						}   
					}
					output = output + _inputEvents[i].ToString();
					Debug.Log(output);
				}


                // Call all registered callback functions
				ProcessCallbacks(_inputEvents[i]);

				if (aliases.Count > 0)
				{
                    string tmp = _inputEvents[i].Name;
					for (int j = 0; j < aliases.Count; j++)
					{
                        _inputEvents[i].Name = aliases[j];
						ProcessCallbacks(_inputEvents[i]);
					}
                    _inputEvents[i].Name = tmp;
				}
			}
		}



		public void AddUnityInputEvents(ref List<VREvent> eventList)
		{
			// Convery Unity Mouse Button events to VREvent ButtonUp/Down events
			if (vrDevice.unityMouseBtnsToVREvents)
			{
				if (Input.GetMouseButtonDown(0))
				{
                    VREvent e = new VREvent("MouseBtnLeft_Down");
                    e.AddData("EventType", "ButtonDown");
					eventList.Add(e);
				}
				if (Input.GetMouseButtonDown(2))
				{
                    VREvent e = new VREvent("MouseBtnMiddle_Down");
                    e.AddData("EventType", "ButtonDown");
                    eventList.Add(e);
				}
				if (Input.GetMouseButtonDown(1))
				{
                    VREvent e = new VREvent("MouseBtnRight_Down");
                    e.AddData("EventType", "ButtonDown");
                    eventList.Add(e);
				}
				if (Input.GetMouseButtonUp(0))
				{
                    VREvent e = new VREvent("MouseBtnLeft_Up");
                    e.AddData("EventType", "ButtonUp");
                    eventList.Add(e);
				}
				if (Input.GetMouseButtonUp(2))
				{
                    VREvent e = new VREvent("MouseBtnMiddle_Up");
                    e.AddData("EventType", "ButtonUp");
                    eventList.Add(e);
				}
				if (Input.GetMouseButtonUp(1))
				{
                    VREvent e = new VREvent("MouseBtnRight_Up");
                    e.AddData("EventType", "ButtonUp");
                    eventList.Add(e);
				}
			}


            // Convert Unity mouse move events to VREvent Cursor events
            if (vrDevice.unityMouseMovesToVREvents)
			{
				Vector3 pos = Input.mousePosition;
				if (pos != lastMousePos)
				{
					Vector3 npos = new Vector3(pos[0] / Screen.width, pos[1] / Screen.height, 0.0f);
                    VREvent e = new VREvent("Mouse_Move");
                    e.AddData("EventType", "CursorMove");
					e.AddData("Position", VRConvert.ToFloatArray(pos));
					e.AddData("NormalizedPosition", VRConvert.ToFloatArray(npos));
					eventList.Add(e);
					lastMousePos = pos;
				}
			}


			// Convert Unity key up/down events to VREvent ButtonUp/Down events
			for (int i = 0; i < vrDevice.unityKeysToVREvents.Count; i++)
			{
				if (Input.GetKeyDown(vrDevice.unityKeysToVREvents[i]))
				{
                    VREvent e = new VREvent("Kbd" + vrDevice.unityKeysToVREvents[i] + "_Down");
					e.AddData("EventType", "ButtonDown");
					eventList.Add(e);
				}
				if (Input.GetKeyDown(vrDevice.unityKeysToVREvents[i]))
				{
                    VREvent e = new VREvent("Kbd" + vrDevice.unityKeysToVREvents[i] + "_Up");
                    e.AddData("EventType", "ButtonUp");
                    eventList.Add(e);
				}
			}

			// Convert Unity Axis values to VREvent AnalogUpdate events
			for (int i = 0; i < vrDevice.unityAxesToVREvents.Count; i++) {
				float current = Input.GetAxis(vrDevice.unityAxesToVREvents[i]);
				bool update = false;
				if (axesStates.ContainsKey(vrDevice.unityAxesToVREvents[i])) {
					float last = axesStates[vrDevice.unityAxesToVREvents[i]];
					if (current != last) {
						axesStates[vrDevice.unityAxesToVREvents[i]] = current;
						update = true;
					}
				}
				else {
					axesStates.Add(vrDevice.unityAxesToVREvents[i], current);
					update = true;
				}
				if (update) {
                    VREvent e = new VREvent(vrDevice.unityAxesToVREvents[i] + "_Update");
                    e.AddData("EventType", "AnalogUpdate");
                    e.AddData("AnalogValue", current);
                    eventList.Add(e);
				}
			}


            // TODO: Convert other Unity inputs as needed (touch, accelerometer, etc.)

		}


        private void ProcessCallbacks(VREvent e)
		{
            // These "unfiltered" callbacks are called for every event
            for (int ucb = 0; ucb < genericCallbacksUnfiltered.Count; ucb++) {
                genericCallbacksUnfiltered[ucb].Invoke(e);
            }


            // These callbacks are passed the enitre event data structure, which can really contain anything.
            if (genericCallbacks.ContainsKey(e.Name))
			{
				for (int cb = 0; cb < genericCallbacks[e.Name].Count; cb++)
				{
                    genericCallbacks[e.Name][cb].Invoke(e);
				}
			}


            // For these callbacks, we know the type of data, so we pull it out of the dataindex here to
            // make it a bit easier for the user.

			if ((analogCallbacks.ContainsKey(e.Name)) &&
				(e.ContainsStringField("EventType")) &&
				(e.GetStringData("EventType") == "AnalogUpdate"))
			{
				for (int cb = 0; cb < analogCallbacks[e.Name].Count; cb++)
				{
					analogCallbacks[e.Name][cb].Invoke(e.GetFloatData("AnalogValue"));
				}
			}

			if ((buttonDownCallbacks.ContainsKey(e.Name)) &&
                (e.ContainsStringField("EventType")) &&
                ((e.GetStringData("EventType") == "ButtonDown") || (e.GetStringData("EventType") == "ButtonTouch")))
			{
				for (int cb = 0; cb < buttonDownCallbacks[e.Name].Count; cb++)
				{
					buttonDownCallbacks[e.Name][cb].Invoke();
				}
			}

		    if ((buttonUpCallbacks.ContainsKey(e.Name)) &&
                (e.ContainsStringField("EventType")) &&
                ((e.GetStringData("EventType") == "ButtonUp") || (e.GetStringData("EventType") == "ButtonUntouch")))
			{
				for (int cb = 0; cb < buttonUpCallbacks[e.Name].Count; cb++)
				{
					buttonUpCallbacks[e.Name][cb].Invoke();
				}
			}
			
			if ((cursorCallbacks.ContainsKey(e.Name)) &&
                (e.ContainsStringField("EventType")) &&
                (e.GetStringData("EventType") == "CursorMove"))
			{
				float[] pos = e.GetFloatArrayData("Position");
				float[] npos = e.GetFloatArrayData("NormalizedPosition");
				for (int cb = 0; cb < cursorCallbacks[e.Name].Count; cb++)
				{
					cursorCallbacks[e.Name][cb].Invoke(new Vector3(pos[0], pos[1], pos[2]), new Vector3(npos[0], npos[1], npos[2]));
				}
			}
			
			if ((trackerCallbacks.ContainsKey(e.Name)) &&
                (e.ContainsStringField("EventType")) &&
                (e.GetStringData("EventType") == "TrackerMove"))
			{
				float[] data = e.GetFloatArrayData("Transform");
				Matrix4x4 m = VRConvert.ToMatrix4x4(data);
				Vector3 pos = m.GetTranslation();
				Quaternion rot = m.GetRotation();
				for (int cb = 0; cb < trackerCallbacks[e.Name].Count; cb++)
				{
					trackerCallbacks[e.Name][cb].Invoke(pos, rot);
				}
			}
		
		}




		// AT END OF EACH FRAME:  WAIT FOR THE SIGNAL THAT ALL CLIENTS ARE ALSO READY, THEN SWAPBUFFERS
		private void PostRender() {
			if (_netInterface != null) {
				_netInterface.SynchronizeSwapBuffersAcrossAllNodes();
                _state = NetState.PreUpdateNext;
            }
		}


        void Awake() {
            DontDestroyOnLoad(this.gameObject);
            if (instance == null) {
                instance = this;
            }
            else if (instance != this) {
                DestroyImmediate(this.gameObject);
            }
        }

        void Start() {
            if (!_initialized) {
                Initialize();
                StartCoroutine(EndOfFrameLoop());
            }
        }

        // See important note above... this Update() method MUST be called before any others in your Unity App.
        void Update() {
            if (_state == NetState.PreUpdateNext) {
                // Since we force this Script to be the first one that Unity calls, this gives us a hook to create
                // something like a "PreUpdate()" function.  It would have been nice if Unity provided this for us,
                // but they do not (yet) provide a PreUpdate() callback.
                PreUpdate();
            }
            // We also need a callback when the scene is done rendering, so we request that callback each frame.
            //StartCoroutine(EndOfFrameCallback());
        }


        IEnumerator EndOfFrameCallback() {
            // This is a fancy feature of Unity and C# and is the only way I know how to get a callback after Unity
            // has finished completely rendering the frame, which may include rendering more than one camera.
            // The yield command pauses execution of this function until the EndOfFrame is reached.
            yield return new WaitForEndOfFrame();

            if (_state == NetState.PostRenderNext) {
                PostRender();
            }
        }

        IEnumerator EndOfFrameLoop()
        {
            while (true)
            {
                // This is a fancy feature of Unity and C# and is the only way I know how to get a callback after Unity
                // has finished completely rendering the frame, which may include rendering more than one camera.
                // The yield command pauses execution of this function until the EndOfFrame is reached.
                yield return new WaitForEndOfFrame();
                if (_state == NetState.PostRenderNext)
                {
                    PostRender();
                }
            }
        }
        
        /*
        void OnPostRender()
        {
            if (_state == NetState.PostRenderNext)
            {
                PostRender();
            }
        }
        */
        


        // gets all objects in the scene, even if the objects are inactive
        List<GameObject> GetAllObjectsInScene() {
            List<GameObject> objectsInScene = new List<GameObject>();

            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]) {
                if (go.hideFlags != HideFlags.None)
                    continue;
                objectsInScene.Add(go);
            }
            return objectsInScene;
        }


        GameObject FindObjectByName(string goName, List<GameObject> gameObjects) {
            int index = 0;
            while ((index < gameObjects.Count) && (gameObjects[index].name != goName)) {
                index++;
            }
            if (index < gameObjects.Count) {
                // found the object!
                return gameObjects[index];
            }
            else {
                return null;
            }
        }


        List<GameObject> FindObjectsByTag(string goTag, List<GameObject> gameObjects) {
            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < gameObjects.Count; i++) {
                if (gameObjects[i].CompareTag(goTag)) {
                    list.Add(gameObjects[i]);
                }
            }
            return list;
        }

        


    } // class VRMain

} // namespace MinVR
