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
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MinVR
{

    public class VRPNTracker : MonoBehaviour
    {
        [Tooltip("The name of the VRPN device")]
        public string device = "Tracker0";

        [Tooltip("The address of the server, leave as localhost if running on the same computer")]
        public string server = "localhost";

        // the sensor number to track
        public int sensor = 0;

        [Tooltip("The name of the VREvents to generate")]
        public string eventName;

        [Tooltip("The origin of the Unity tracker coordinate space." +
        " The tracker transform will be relative to this transform," +
        " can be left as null if using (0, 0, 0)")]
        public Transform origin = null;

        [Tooltip("If true, applies the event updates to the game object's transform based on the movement type." +
            " Otherwise, events are addded to pendingEvents and can be retrieved by calling AddEventsSinceLastFrame()")]
        public bool applyUpdatesToGameObject = false;

        public enum TrackingMode { SIX_DOF, POSITION_ONLY, ORIENTATION_ONLY };
        [Tooltip("Update position, orientation, or both?")]
        public TrackingMode trackingMode = TrackingMode.SIX_DOF;

        public enum ReportType { MOST_RECENT, AVERAGE, SMOOTH };
        [Tooltip("How should multiple updates per frame be handled?")]
        public ReportType reportType = ReportType.MOST_RECENT;

        public enum ThreadingMode { SINGLE_THREADED, MULTI_THREADED };
        public ThreadingMode threadingMode = ThreadingMode.MULTI_THREADED;

        [Tooltip("For making manual adjustments to tracker position" +
        " (e.g. if there is an offset between tracker location and eyepoint)")]
        public Vector3 positionAdjustment = Vector3.zero;

        [Tooltip("For making manual adjustments to tracker rotation" +
        " (e.g. if the tracker is mounted in the wrong direction on a rigid object)")]
        public Vector3 rotationAdjustment = Vector3.zero;

        [Tooltip("For making manual adjustments to tracker rotation" +
        " (e.g. if the tracker is mounted in the wrong direction on a rigid object)")]
        public Vector3 scaleAdjustment = Vector3.one;

        IntPtr trackerDataPointer;
        TrackerData trackerData;
        static int lastTrackerUpdateFrame = -1;

        private const int MAX_QUEUE_LENGTH = 256;
        private int queueIndex = 0;
        private Vector3[] previousFramesPosition = new Vector3[MAX_QUEUE_LENGTH];
        private Quaternion[] previousFramesRotation = new Quaternion[MAX_QUEUE_LENGTH];
        private int initialFrames = 0;

        private List<VREvent> pendingEvents;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct TrackerData
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public double[] position;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public double[] rotation;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public double[] positionSum;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public double[] rotationSum;

            public int numReports;
        }

        [DllImport("VRPNMeshClient")]
        static extern IntPtr InitializeTracker(string serverName, int sensorNumber);

        [DllImport("VRPNMeshClient")]
        static extern void RemoveTracker(string serverName, int sensorNumber);

        // for single threading
        [DllImport("VRPNMeshClient")]
        static extern void UpdateTrackers();

        // for multithreading
        [DllImport("VRPNMeshClient")]
        static extern void UpdateTrackersMultiThreaded();

        [DllImport("VRPNMeshClient")]
        static extern void LockTrackerData(IntPtr trackerData);

        [DllImport("VRPNMeshClient")]
        static extern void UnlockTrackerData(IntPtr trackerData);

        // Use this for initialization
        void Start()
        {
            trackerDataPointer = InitializeTracker(device + "@" + server, sensor);

            pendingEvents = new List<VREvent>();

            if (String.IsNullOrEmpty(eventName))
            {
                eventName = device;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (threadingMode == ThreadingMode.SINGLE_THREADED)
            {
                // update the trackers only once per frame
                if (lastTrackerUpdateFrame != Time.frameCount)
                {
                    UpdateTrackers();
                    lastTrackerUpdateFrame = Time.frameCount;
                }

                trackerData = (TrackerData)Marshal.PtrToStructure(trackerDataPointer, typeof(TrackerData));
            }
            else
            {
                // update the trackers only once per frame
                if (lastTrackerUpdateFrame != Time.frameCount)
                {
                    UpdateTrackersMultiThreaded();
                    lastTrackerUpdateFrame = Time.frameCount;
                }

                LockTrackerData(trackerDataPointer);
                trackerData = (TrackerData)Marshal.PtrToStructure(trackerDataPointer, typeof(TrackerData));
                UnlockTrackerData(trackerDataPointer);
            }

            Vector3 transformPosition = transform.position;
            Quaternion transformRotation = transform.rotation;

            this.updateTransformFromTrackerData(ref transformPosition, ref transformRotation);

            // if a custom origin has been specified
            // then update the transform coordinate space
            if (origin != null)
            {
                if (trackingMode != TrackingMode.POSITION_ONLY)
                    transformRotation = origin.rotation * transformRotation;
                else
                    transformRotation = origin.rotation;

                if (trackingMode != TrackingMode.ORIENTATION_ONLY)
                    transformPosition = origin.position + origin.rotation * transformPosition;
                else
                    transformPosition = origin.position;
            }

            Matrix4x4 m3 = Matrix4x4.TRS(transformPosition, transformRotation, Vector3.one);
            float[] d3 = VRConvert.ToFloatArray(m3);
            VREvent e = new VREvent(eventName);
            e.AddData("EventType", "TrackerMove");
            e.AddData("Transform", d3);
            pendingEvents.Add(e);

            if (applyUpdatesToGameObject)
            {
                transform.position = transformPosition;
                transform.rotation = transformRotation;
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
            RemoveTracker(device + "@" + server, sensor);
        }

        void updateTransformFromTrackerData(ref Vector3 transformPosition, ref Quaternion transformRotation)
        {
            if (this.reportType == ReportType.SMOOTH || this.reportType == ReportType.MOST_RECENT)
            {
                Quaternion rotation = Quaternion.identity;
                if (trackingMode != TrackingMode.POSITION_ONLY)
                {
                    // convert Motive VRPN rotation into Unity coordinate system
                    rotation = transformRotation;
                    rotation.x = (float)trackerData.rotation[0];
                    rotation.y = (float)trackerData.rotation[1];
                    rotation.z = -(float)trackerData.rotation[2];
                    rotation.w = -(float)trackerData.rotation[3];

                    // apply tracker rotation and adjustment
                    transformRotation = rotation * Quaternion.Euler(rotationAdjustment);
                }

                if (trackingMode != TrackingMode.ORIENTATION_ONLY)
                {
                    // convert Motive VRPN position into Unity coordinate system
                    Vector3 position = transformPosition;
                    position.x = (float)trackerData.position[0] * scaleAdjustment.x;
                    position.y = (float)trackerData.position[1] * scaleAdjustment.y;
                    position.z = -(float)trackerData.position[2] * scaleAdjustment.z;

                    // apply tracker position and adjustment
                    transformPosition = position + rotation * positionAdjustment;
                }
            }
            else if (this.reportType == ReportType.AVERAGE && this.trackerData.numReports > 0)
            {
                Quaternion rotation = Quaternion.identity;
                if (trackingMode != TrackingMode.POSITION_ONLY)
                {
                    // convert VRPN rotation into Unity coordinate system
                    rotation = transformRotation;
                    rotation.x = -(float)trackerData.rotationSum[0] / trackerData.numReports;
                    rotation.y = -(float)trackerData.rotationSum[1] / trackerData.numReports;
                    rotation.z = (float)trackerData.rotationSum[2] / trackerData.numReports;
                    rotation.w = (float)trackerData.rotationSum[3] / trackerData.numReports;

                    // apply tracker rotation and adjustment
                    transformRotation = rotation * Quaternion.Euler(rotationAdjustment);
                }

                if (trackingMode != TrackingMode.ORIENTATION_ONLY)
                {
                    // convert VRPN position into Unity coordinate system
                    Vector3 position = transformPosition;
                    position.x = ((float)trackerData.positionSum[0] / trackerData.numReports) * scaleAdjustment.x;
                    position.y = ((float)trackerData.positionSum[1] / trackerData.numReports) * scaleAdjustment.y;
                    position.z = -((float)trackerData.positionSum[2] / trackerData.numReports) * scaleAdjustment.z;

                    // apply tracker position and adjustment
                    transformPosition = position + rotation * positionAdjustment;
                }
            }

            if (this.reportType == ReportType.SMOOTH)
            {
                this.previousFramesPosition[this.queueIndex] = transformPosition;
                this.previousFramesRotation[this.queueIndex] = transformRotation;
                this.queueIndex = (this.queueIndex + 1) % this.previousFramesPosition.Length;

                if (this.initialFrames >= this.previousFramesPosition.Length)
                {
                    transformPosition = GetSmoothedPosition();
                    transformRotation = GetSmoothedRotation();
                }
                else
                {
                    this.initialFrames++;
                }
            }
        }

        Vector3 GetSmoothedPosition()
        {
            // Unweighted average over n frames
            Vector3 suma = this.previousFramesPosition.Aggregate((v, r) => v + r);
            return suma / this.previousFramesPosition.Length;
        }

        Quaternion GetSmoothedRotation()
        {
            IEnumerable<float> xs = this.previousFramesRotation.Select(t => t.x);
            IEnumerable<float> ys = this.previousFramesRotation.Select(t => t.y);
            IEnumerable<float> zs = this.previousFramesRotation.Select(t => t.z);
            IEnumerable<float> ws = this.previousFramesRotation.Select(t => t.w);

            return new Quaternion(
                WeightSumComponents(xs),
                WeightSumComponents(ys),
                WeightSumComponents(zs),
                WeightSumComponents(ws)
            );
        }

        static float WeightSumComponents(IEnumerable<float> components)
        {
            return components.Aggregate((c, r) => c + r);
        }
    }
}
