/* VRPNAnalog.cs
 * Author: Evan Suma Rosenberg, Ph.D., Modified by Bret Jackson
 * Email: suma@umn.edu, bjackson@macalester.edu
 * Copyright (c) 2019, University of Minnesota
 * 
 * NOTE: Requires the VRPNMeshClient libraries (VRPNMeshClient.dll for Windows
 * and libVRPNMeshClient.so for Magic Leap)
 */

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MinVR
{

    public class VRPNAnalog : MonoBehaviour
    {
        [Tooltip("The name of the VRPN device")]
        public string device = "Analog0";

        [Tooltip("The address of the server, leave as localhost if running on the same computer")]
        public string server = "localhost";

        // the channel number
        public int channel = 0;

        [Tooltip("The name of the VREvents to generate")]
        public string eventName;

        IntPtr analogDataPointer;
        AnalogData analogData;

        [Tooltip("If true, applies the event updates to the game object's transform based on the movement type.")]
        public bool applyUpdatesToGameObject = false;

        public enum MovementType { NONE, TRANSLATE, ROTATE, SCALE };
        [Tooltip("The movement effect to apply to the game object's transform")]
        public MovementType movementType = MovementType.TRANSLATE;

        public enum Axis { X, Y, Z, ALL };
        [Tooltip("The axis to apply the movement on")]
        public Axis axis = Axis.X;

        public float speed = 1.0f;

        static int lastAnalogUpdateFrame = -1;

        private float lastAnalogState = -1;

        private List<VREvent> pendingEvents;


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AnalogData
        {
            public double state;
        }

        [DllImport("VRPNMeshClient")]
        static extern IntPtr InitializeAnalog(string serverName, int channelNumber);

        [DllImport("VRPNMeshClient")]
        static extern void UpdateAnalogs();

        [DllImport("VRPNMeshClient")]
        static extern void RemoveAnalog(string serverName, int channelNumber);

        // Use this for initialization
        void Start()
        {
            analogDataPointer = InitializeAnalog(device + "@" + server, channel);

            pendingEvents = new List<VREvent>();
            if (String.IsNullOrEmpty(eventName))
            {
                eventName = device;
            }
        }

        // Update is called once per frame
        void Update()
        {
            // update the trackers only once per frame
            if (lastAnalogUpdateFrame != Time.frameCount)
            {
                UpdateAnalogs();
                lastAnalogUpdateFrame = Time.frameCount;
            }
            analogData = (AnalogData)Marshal.PtrToStructure(analogDataPointer, typeof(AnalogData));

            if (analogData.state != lastAnalogState)
            {
                lastAnalogState = (float)analogData.state;

                VREvent e = new VREvent(eventName);
                e.AddData("EventType", "AnalogUpdate");
                e.AddData("AnalogValue", (float)analogData.state);
                pendingEvents.Add(e);

                if (applyUpdatesToGameObject)
                {

                    float speedThisFrame = speed * Time.deltaTime * (float)analogData.state;

                    if (movementType == MovementType.TRANSLATE)
                    {
                        Vector3 position = transform.localPosition;
                        if (axis == Axis.X)
                            position.x += speedThisFrame;
                        else if (axis == Axis.Y)
                            position.y += speedThisFrame;
                        else if (axis == Axis.Z)
                            position.z += speedThisFrame;
                        else
                        {
                            position.x += speedThisFrame;
                            position.y += speedThisFrame;
                            position.z += speedThisFrame;
                        }

                        transform.localPosition = position;
                    }
                    else if (movementType == MovementType.ROTATE)
                    {
                        Quaternion newRotation = Quaternion.identity;
                        Vector3 newAngles = newRotation.eulerAngles;

                        if (axis == Axis.X)
                            newAngles.x += speedThisFrame;
                        else if (axis == Axis.Y)
                            newAngles.y += speedThisFrame;
                        else if (axis == Axis.Z)
                            newAngles.z += speedThisFrame;
                        else
                        {
                            newAngles.x += speedThisFrame;
                            newAngles.y += speedThisFrame;
                            newAngles.z += speedThisFrame;
                        }

                        if (newAngles.x > 360)
                            newAngles.x %= 360;

                        if (newAngles.y > 360)
                            newAngles.y %= 360;

                        if (newAngles.z > 360)
                            newAngles.z %= 360;

                        newRotation.eulerAngles = newAngles;


                        Quaternion rotation = transform.localRotation;
                        rotation = rotation * newRotation;
                        transform.localRotation = rotation;
                    }
                    else if (movementType == MovementType.SCALE)
                    {
                        Vector3 scale = transform.localScale;
                        if (axis == Axis.X)
                            scale.x += speedThisFrame;
                        else if (axis == Axis.Y)
                            scale.y += speedThisFrame;
                        else if (axis == Axis.Z)
                            scale.z += speedThisFrame;
                        else
                        {
                            scale.x += speedThisFrame;
                            scale.y += speedThisFrame;
                            scale.z += speedThisFrame;
                        }

                        transform.localScale = scale;
                    }
                }
            }

        }

        public void AddEventsSinceLastFrame(ref List<VREvent> eventList)
        {
            if (pendingEvents.Count > 0)
            {
                eventList.AddRange(pendingEvents);
                pendingEvents.Clear();
            }
        }

        void OnDestroy()
        {
            RemoveAnalog(device + "@" + server, channel);
        }

        public double GetAnalogState() {
            return this.analogData.state;
        }
    }
}
