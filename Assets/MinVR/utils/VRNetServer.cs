using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net; 
using System.Net.Sockets;

namespace MinVR {

    public class VRNetServer : VRNetInterface {

        TcpListener server;

        List<TcpClient> clients = new List<TcpClient>();

        public VRNetServer(int port, int numClients) {

            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            Debug.Log("Server waiting for " + numClients + " connection(s)...");
            while (clients.Count < numClients) {
                try {
                    // Blocking call to accept requests
                    TcpClient client = server.AcceptTcpClient();
                    if (client.Connected) {
                        client.NoDelay = true;
                        clients.Add(client);
                    }
                }
                catch (Exception e) {
                    Debug.Log(String.Format("Exception: {0}", e));
                    Console.WriteLine("Exception: {0}", e);
                }
            }
        }


        ~VRNetServer() {
            for (int i=0; i<clients.Count; i++) {
                clients[i].Close();
            }
        }

        public void SynchronizeInputEventsAcrossAllNodes(ref List<VREvent> inputEvents) {

            // 1. FOR EACH CLIENT, RECEIVE A LIST OF INPUT EVENTS GENERATED ON THE CLIENT
            // AND ADD THEM TO THE SERVER'S INPUTEVENTS LIST

            // the following section implements something similar to a socket select statement.
            // we need to receive data from all clients, but socket 4 may be ready to send data
            // before socket 1, so we loop through the sockets reading from the first we find
            // that is ready to send data, then continue looping until we have read from all.

            // initialize list to include all streams in the list to read from
            List<int> toRead = new List<int>(clients.Count);
            for (int i = 0; i < clients.Count; i++) {
                toRead.Add(i);
            }

            // loop until the list of streams to read from is empty
            while (toRead.Count > 0) {
                int i = 0;
                while (i < toRead.Count) {
                    if (clients[toRead[i]].GetStream().DataAvailable) {
                        // if ready to read, read data and remove from the list of streams to read from
                        TcpClient c = clients[toRead[i]];
                        VRNet.ReceiveEventData(ref c, ref inputEvents);
                        toRead.RemoveAt(i);
                    }
                    else {
                        // this stream not ready to read, move on to the next
                        i++;
                    }
                }
            }


            // 2. SEND THE COMBINED INPUT EVENTS LIST OUT TO ALL CLIENTS
            for (int i = 0; i < clients.Count; i++) {
                TcpClient c = clients[i];
                VRNet.SendEventData(ref c, in inputEvents);
            }
            
        }



        public void SynchronizeSwapBuffersAcrossAllNodes() {

            // 1. WAIT FOR A SWAP BUFFERS REQUEST MESSAGE FROM ALL CLIENTS

            // the following section implements something similar to a socket select statement.
            // we need to receive data from all clients, but socket 4 may be ready to send data
            // before socket 1, so we loop through the sockets reading from the first we find
            // that is ready to send data, then continue looping until we have read from all.

            // initialize list to include all streams in the list to read from
            List<int> toRead = new List<int>(clients.Count);
            for (int i = 0; i < clients.Count; i++) {
                toRead.Add(i);
            }

            // loop until the list of streams to read from is empty
            while (toRead.Count > 0) {
                int i = 0;
                while (i < toRead.Count) {
                    if (clients[toRead[i]].GetStream().DataAvailable) {
                        // if ready to read, read data and remove from the list of streams to read from
                        TcpClient c = clients[toRead[i]];
                        VRNet.ReceiveSwapBuffersRequest(ref c);
                        toRead.RemoveAt(i);
                    }
                    else {
                        // this stream not ready to read, move on to the next
                        i++;
                    }
                }
            }


            // 2. SEND A SWAP BUFFERS NOW MESSAGE TO ALL CLIENTS
            for (int i = 0; i < clients.Count; i++) {
                TcpClient c = clients[i];
                VRNet.SendSwapBuffersNow(ref c);
            }

        }


    }

}
