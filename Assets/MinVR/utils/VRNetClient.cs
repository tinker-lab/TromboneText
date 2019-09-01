using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;


namespace MinVR {

    public class VRNetClient : VRNetInterface {
	
	    TcpClient client;
	    NetworkStream stream;

	    public VRNetClient(string serverIP, int serverPort) {
            // continue trying to connect until we have success
            bool success = false;
            while (!success) {
                try {
                    client = new TcpClient(AddressFamily.InterNetwork);
                    client.NoDelay = true;
                    client.Connect(IPAddress.Parse(serverIP), serverPort);
                    stream = client.GetStream();
                    success = client.Connected;
                }
                catch (Exception e) {
                    Debug.Log(String.Format("Exception: {0}", e));
                    Console.WriteLine("Exception: {0}", e);
                }
                if (!success) {
                    Debug.Log("Having trouble connecting to the VRNetServer.  Trying again...");
                    Console.WriteLine("Having trouble connecting to the VRNetServer.  Trying again...");
                    Thread.Sleep(500);
                }
            }
	    }
	
	    ~VRNetClient() {
		    try {
			    stream.Close();         
			    client.Close();         
		    }
            catch (Exception e) {
                Debug.Log(String.Format("Exception: {0}", e));
                Console.WriteLine("Exception: {0}", e);
            }
        }


        public void SynchronizeInputEventsAcrossAllNodes(ref List<VREvent> inputEvents) {
            // 1. send inputEvents to server
            VRNet.SendEventData(ref client, in inputEvents);

            // 2. receive and parse serverInputEvents
            List<VREvent> serverInputEvents = new List<VREvent>();
            VRNet.ReceiveEventData(ref client, ref serverInputEvents);
		
		    // 3. inputEvents = serverInputEvents
		    inputEvents = serverInputEvents;
	    }
	
	    public void SynchronizeSwapBuffersAcrossAllNodes() {
            // 1. send a swap_buffers_request message to the server
            VRNet.SendSwapBuffersRequest(ref client);

            // 2. wait for and receive a swap_buffers_now message from the server
            VRNet.ReceiveSwapBuffersNow(ref client);
	    }
    }

} // namespace MinVR
